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
        private string _submitButtonText;
        private bool _cannotSubmitLayoutVisible;
        private string _cannotSubmitHeaderText;
        private string _cannotSubmitLabelText;
        private bool _purchaseOptionsVisible;
        private string _purchaseOptionsText;
        private bool _mainLayoutVisible;
        private readonly LocationModel _location;
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
        private readonly Action<string> _changeSubmitButtonStyle;

        public ICommand OnAppearingBehavior { get; private set; }

        public OrderConfirmationViewModel(LocationModel location,
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
                                          Action<string> changeSubmitButtonStyle,
                                          ILogger<OrderConfirmationViewModel> logger)
        {
            _location = location;
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
            _changeSubmitButtonStyle = changeSubmitButtonStyle;
            OnAppearingBehavior = new Command(async () => await SetViewState());
            PurchaseOptionsCommand = new Command(async () =>
            {
                var val = await _orderValidation.ValidateOrderRequest(_userService.GetLoggedInAccount());
                if (SubscriptionUtility.SubscriptionActive(val.Subscription))
                    _nav.Push(_pageFactory.GetPage(PageType.SingleReportPurchase, val));
                else
                    _nav.Push(_pageFactory.GetPage(PageType.PurchaseOptions, val));
            });
        }

        private async Task SetViewState()
        {
            _changeSubmitButtonStyle(_userService.GetLoggedInAccount() == null ? "btn-primary" : "btn-success");
            SubmitButtonText = _userService.GetLoggedInAccount() == null ? "Log In" : "Submit Order";
            AddressLine1 = $"{_location.Placemark.SubThoroughfare} {_location.Placemark.Thoroughfare}";
            AddressLine2 = $"{_location.Placemark.SubLocality + " "}{_location.Placemark.Locality}, {_location.Placemark.AdminArea} {_location.Placemark.PostalCode}";
            await SetVisualStateForValidation();
        }

        private async Task Submit()
        {
            try
            {
                var user = _userService.GetLoggedInAccount();
                SubmitButtonEnabled = false;
                if (user == null)
                {
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
            finally
            {
                try
                {
                    await SetVisualStateForValidation();
                }
                catch { }
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
                PlatformType = _deviceType == Device.Android ? Models.PlatformType.Android : Models.PlatformType.iOS,
                Status = new StatusModel() { Status = Status.Pending },
                AddressPosition = new PositionModel() { Latitude = _location.Position.Latitude, Longitude = _location.Position.Longitude }
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
            await _alertUtility.Display("Order Submitted", "Thank you for your order!", "Ok");
            _nav.Pop();
            _baseNavAction(BaseNavPageType.MyOrders);
        }
        private async Task SetVisualStateForValidation()
        {
            try
            {
                var user = _userService.GetLoggedInAccount();
                if (user == null)
                {
                    MainLayoutVisible = true;
                    CannotSubmitLayoutVisible = false;
                    return;
                }
                var validation = await _orderValidation.ValidateOrderRequest(user);
                var activeSub = SubscriptionUtility.SubscriptionActive(validation.Subscription);
                switch (validation.State)
                {
                    case ValidationState.NoReportsLeftInPeriod:
                        MainLayoutVisible = false;
                        CannotSubmitHeaderText = "You've been busy!";
                        CannotSubmitLabelText = $"Sorry, you have used all of your reports for this month.";
                        CannotSubmitLayoutVisible = true;
                        PurchaseOptionsText = $"Additional reports can be purchased at a reduced price of " +
                            $"${SubscriptionUtility.GetSingleReportInfo(validation).Price} per report.";
                        PurchaseOptionsVisible = true;
                        break;
                    case ValidationState.NoSubscriptionAndTrialValid:
                        MainLayoutVisible = false;
                        CannotSubmitHeaderText = "Thanks for trying Fair Squares!";
                        CannotSubmitLabelText = $"Please claim your free one month subscription trial, or click below to view other options.";
                        CannotSubmitLayoutVisible = true;
                        PurchaseOptionsVisible = false;
                        break;
                    case ValidationState.NoSubscriptionAndTrialAlreadyUsed:
                        MainLayoutVisible = false;
                        CannotSubmitHeaderText = "Thanks for trying Fair Squares!";
                        CannotSubmitLabelText = $"Click below to view options for getting more reports.";
                        CannotSubmitLayoutVisible = true;
                        PurchaseOptionsVisible = false;
                        break;
                    default:
                        MainLayoutVisible = true;
                        CannotSubmitLayoutVisible = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to set visual state on load.", ex);
            }
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
        public string SubmitButtonText
        {
            get
            {
                return _submitButtonText;
            }
            set
            {
                _submitButtonText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubmitButtonText)));
            }
        }
        public bool CannotSubmitLayoutVisible
        {
            get
            {
                return _cannotSubmitLayoutVisible;
            }
            set
            {
                _cannotSubmitLayoutVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CannotSubmitLayoutVisible)));
            }
        }
        public string CannotSubmitHeaderText
        {
            get
            {
                return _cannotSubmitHeaderText;
            }
            set
            {
                _cannotSubmitHeaderText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CannotSubmitHeaderText)));
            }
        }
        public string CannotSubmitLabelText
        {
            get
            {
                return _cannotSubmitLabelText;
            }
            set
            {
                _cannotSubmitLabelText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CannotSubmitLabelText)));
            }
        }
        public string PurchaseOptionsText
        {
            get
            {
                return _purchaseOptionsText;
            }
            set
            {
                _purchaseOptionsText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PurchaseOptionsText)));
            }
        }
        public bool PurchaseOptionsVisible
        {
            get
            {
                return _purchaseOptionsVisible;
            }
            set
            {
                _purchaseOptionsVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PurchaseOptionsVisible)));
            }
        }
        public bool MainLayoutVisible
        {
            get
            {
                return _mainLayoutVisible;
            }
            set
            {
                _mainLayoutVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MainLayoutVisible)));
            }
        }
        public ICommand PurchaseOptionsCommand { get; private set; }

        private readonly List<OptionViewModel> Options = new List<OptionViewModel>()
        {
            new OptionViewModel() { RoofOption = Models.RoofOption.PrimaryOnly, Text = "Primary Roof Only" },
            new OptionViewModel() { RoofOption = Models.RoofOption.RoofDetachedGarage, Text = "Include Detached Garage" },
            new OptionViewModel() { RoofOption = Models.RoofOption.RoofShedBarn, Text = "Include Barn/Shed" }
        };
    }
}
