using FairSquares.Measurement.Core.Models;
using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class OrderConfirmationViewModel : INotifyPropertyChanged
    {
        private string _addressLine1;
        private string _addressLine2;
        private bool _submitButtonEnabled;
        private int _selectedOptionIndex;
        private string _comments;
        private readonly Placemark _placemark;
        private readonly ICurrentUserService _userService;
        private readonly AlertUtility _alertUtility;
        private readonly Action<BaseNavPageType> _baseNavAction;
        private readonly IOrderValidationService _orderValidation;
        private readonly IOrderService _orderService;
        private readonly IPageFactory _pageFactory;
        private readonly IMessagingSubscriber _topicSubscriber;
        private readonly string _deviceType;
        private readonly ICache<Models.Order> _orderCache;
        private readonly IToastService _toast;
        private readonly MainThreadNavigator _nav;
        private readonly ILogger<OrderConfirmationViewModel> _logger;

        public OrderConfirmationViewModel(Placemark placemark,
                                          ICurrentUserService userService,
                                          AlertUtility alertUtility,
                                          Action<BaseNavPageType> baseNavAction,
                                          IOrderValidationService orderValidation,
                                          IOrderService orderService,
                                          IPageFactory pageFactory,
                                          IMessagingSubscriber topicSubscriber,
                                          IToastService toast,
                                          string deviceType,
                                          MainThreadNavigator nav,
                                          ICache<Models.Order> orderCache,
                                          ILogger<OrderConfirmationViewModel> logger)
        {
            _placemark = placemark;
            _userService = userService;
            _alertUtility = alertUtility;
            _baseNavAction = baseNavAction;
            _orderValidation = orderValidation;
            _orderService = orderService;
            _pageFactory = pageFactory;
            _deviceType = deviceType;
            _topicSubscriber = topicSubscriber;
            _orderCache = orderCache;
            _toast = toast;
            _nav = nav;
            _logger = logger;
            SubmitButtonEnabled = true;
            SubmitCommand = new Command(async () => await Submit());
            AddressLine1 = $"{_placemark.SubThoroughfare} {_placemark.Thoroughfare}";
            AddressLine2 = $"{_placemark.SubLocality + " "}{_placemark.Locality}, {_placemark.AdminArea} {_placemark.PostalCode}";
        }

        private async Task Submit()
        {
            try
            {
                var user = _userService.GetLoggedInAccount();
                SubmitButtonEnabled = false;
                if (user == null)
                {
                    var answer = await _alertUtility.Display("Please Log In", "Please log in first to submit a report.", "Login", "Cancel");
                    if (!answer)
                    {
                        SubmitButtonEnabled = true;
                        return;
                    }
                    _nav.Push(_pageFactory.GetPage(PageType.Landing));
                    SubmitButtonEnabled = true;
                    return;
                }
                await SubmitOrder(user.UserId, user.Email);
            }
            catch (Exception ex)
            {
                SubmitButtonEnabled = true;
                _toast.LongToast($"Failed to submit order with error '{ex.Message}'. Please contact Fair Squares support.");
                _logger.LogError($"Failed to submit order.", ex);
            }
        }

        private async Task SubmitOrder(string userId, string email)
        {
            var newOrder = new Models.Order()
            {
                StreetAddress = $"{AddressLine1}{Environment.NewLine}{AddressLine2}",
                ReportType = ReportType.Basic,
                MemberId = userId,
                MemberEmail = email,
                RoofOption = Options[SelectedOptionIndex].RoofOption,
                Comments = Comments,
                PlatformType = _deviceType == Device.Android ? Models.PlatformType.Android : Models.PlatformType.iOS
            };
            newOrder.OrderId = await _orderService.AddOrder(newOrder);
            try
            {
                _topicSubscriber.Subscribe(new List<string>() { newOrder.OrderId });
            }
            catch { }
            try
            {
                _orderCache.Put(newOrder.OrderId, newOrder);
            }
            catch { }
            await _alertUtility.Display("Order submitted", "Thank you for your order!", "Ok");
            _nav.Pop();
            _baseNavAction(BaseNavPageType.MyOrders);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public string AddressLine1
        {
            get
            {
                return _addressLine1;
            }
            set
            {
                _addressLine1 = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AddressLine1)));
            }
        }
        public string AddressLine2
        {
            get
            {
                return _addressLine2;
            }
            set
            {
                _addressLine2 = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AddressLine2)));
            }
        }
        public bool SubmitButtonEnabled
        {
            get
            {
                return _submitButtonEnabled;
            }
            set
            {
                _submitButtonEnabled = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubmitButtonEnabled)));
            }
        }
        public ICommand SubmitCommand { get; private set; }
        public List<string> OptionsSource => Options.Select(x => x.Text).ToList();
        public int SelectedOptionIndex
        {
            get
            {
                return _selectedOptionIndex;
            }
            set
            {
                _selectedOptionIndex = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedOptionIndex)));
            }
        }
        public string Comments
        {
            get
            {
                return _comments;
            }
            set
            {
                _comments = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Comments)));
            }
        }
        private readonly List<OptionViewModel> Options = new List<OptionViewModel>()
        {
            new OptionViewModel() { RoofOption = Models.RoofOption.PrimaryOnly, Text = "Primary Roof Only" },
            new OptionViewModel() { RoofOption = Models.RoofOption.RoofDetachedGarage, Text = "Include Detached Garage" },
            new OptionViewModel() { RoofOption = Models.RoofOption.RoofShedBarn, Text = "Include Barn/Shed" }
        };
    }
}
