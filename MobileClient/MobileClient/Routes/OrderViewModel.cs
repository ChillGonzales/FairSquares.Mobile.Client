using FairSquares.Measurement.Core.Models;
using MobileClient.Authentication;
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
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        private readonly IOrderValidationService _orderValidator;
        private readonly ICurrentUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IToastService _toast;
        private readonly MainThreadNavigator _nav;
        private readonly IPageFactory _pageFactory;
        private readonly ICache<Models.Order> _orderCache;
        private readonly Func<string, string, string, string, Task<bool>> _alertTask;
        private readonly IMessagingSubscriber _topicSubscriber;
        private readonly Action<BaseNavPageType> _baseNavigationAction;
        private readonly string _deviceType;
        private GridLength _errorMessageRowHeight;
        private bool _cannotSubmitLayoutVisible;
        private string _cannotSubmitHeaderText;
        private string _cannotSubmitLabelText;
        private bool _mainLayoutVisible;
        private string _addressLine1;
        private string _addressLine2;
        private string _city;
        private int _selectedStateIndex;
        private string _zip;
        private int _selectedOptionIndex;
        private string _comments;
        private string _errorMessage;
        private bool _submitButtonEnabled = true;
        private string _purchaseOptionsText;
        private bool _purchaseOptionsVisible;

        public OrderViewModel(IOrderValidationService validator,
                              ICurrentUserService userCache,
                              IOrderService orderService,
                              IToastService toast,
                              IPageFactory pageFactory,
                              MainThreadNavigator nav,
                              IMessagingSubscriber topicSubscriber,
                              string deviceType,
                              Func<string, string, string, string, Task<bool>> alertTask,
                              Action<BaseNavPageType> baseNavigationAction,
                              ICache<Models.Order> orderCache)
        {
            _orderValidator = validator;
            _userService = userCache;
            _toast = toast;
            _nav = nav;
            _orderService = orderService;
            _pageFactory = pageFactory;
            _orderCache = orderCache;
            _alertTask = alertTask;
            _topicSubscriber = topicSubscriber;
            _baseNavigationAction = baseNavigationAction;
            _deviceType = deviceType;

            ErrorMessageRowHeight = 0;
            SelectedOptionIndex = 0;
            SelectedStateIndex = -1;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            SetVisualStateForValidation();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private async Task SetVisualStateForValidation()
        {
            var user = _userService.GetLoggedInAccount();
            if (user == null)
            {
                MainLayoutVisible = true;
                CannotSubmitLayoutVisible = false;
                return;
            }
            var validation = await _orderValidator.ValidateOrderRequest(user);
            var activeSub = SubscriptionUtility.SubscriptionActive(validation.Subscription);
            PurchaseOptionsCommand = new Command(async () =>
            {
                var val = await _orderValidator.ValidateOrderRequest(user);
                if (SubscriptionUtility.SubscriptionActive(val.Subscription))
                    _nav.Push(_pageFactory.GetPage(PageType.SingleReportPurchase, val));
                else
                    _nav.Push(_pageFactory.GetPage(PageType.PurchaseOptions, val));
            });
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

        private async Task HandleSubmitClick()
        {
            try
            {
                var user = _userService.GetLoggedInAccount();

                SubmitButtonEnabled = false;
                ErrorMessage = "";
                if (string.IsNullOrWhiteSpace(AddressLine1) ||
                    string.IsNullOrWhiteSpace(City) ||
                    SelectedStateIndex < 0 ||
                    string.IsNullOrWhiteSpace(Zip))
                {
                    ErrorMessageRowHeight = GridLength.Star;
                    ErrorMessage = "Please fill out all fields before submitting.";
                    SubmitButtonEnabled = true;
                    return;
                }
                if (user == null)
                {
                    var answer = await _alertTask("Please Log In", "Please log in first to submit a report.", "Login", "Cancel");
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
                ErrorMessageRowHeight = GridLength.Star;
                SubmitButtonEnabled = true;
                ErrorMessage = $"Failed to submit order with error {ex.ToString()}";
            }
        }

        private async Task SubmitOrder(string userId, string email)
        {
            var newOrder = new Models.Order()
            {
                StreetAddress = $"{AddressLine1}\n{(string.IsNullOrWhiteSpace(AddressLine2) ? "" : AddressLine2 + "\n")}\n" +
                                $"{City}, {States[SelectedStateIndex].Code} {Zip}",
                ReportType = ReportType.Basic,
                MemberId = userId,
                MemberEmail = email,
                RoofOption = Options[SelectedOptionIndex].RoofOption,
                Comments = Comments,
                PlatformType = _deviceType == Device.Android ? Models.PlatformType.Android : Models.PlatformType.iOS
            };
            newOrder.OrderId = await _orderService.AddOrder(newOrder);
            _topicSubscriber.Subscribe(new List<string>() { newOrder.OrderId });
            _orderCache.Put(newOrder.OrderId, newOrder);
            _toast.ShortToast($"Your address has been submitted!");

            // Clear all fields
            AddressLine1 = "";
            AddressLine2 = "";
            City = "";
            ErrorMessageRowHeight = 0;
            SelectedStateIndex = -1;
            SelectedOptionIndex = 0;
            Zip = "";
            SubmitButtonEnabled = true;
            Comments = "";
            _baseNavigationAction(BaseNavPageType.MyOrders);
        }

        // Bound properties
        public event PropertyChangedEventHandler PropertyChanged;
        public GridLength ErrorMessageRowHeight
        {
            get
            {
                return _errorMessageRowHeight;
            }
            set
            {
                _errorMessageRowHeight = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessageRowHeight)));
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
        public string City
        {
            get
            {
                return _city;
            }
            set
            {
                _city = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(City)));
            }
        }
        public List<string> StatesSource => States.Select(x => x.Text).ToList();
        public ICommand ToolbarInfo_Command => new Command(() => _nav.Push(_pageFactory.GetPage(PageType.Instruction, false)));
        public ICommand OnAppearingBehavior => new Command(async () => await SetVisualStateForValidation());
        public int SelectedStateIndex
        {
            get
            {
                return _selectedStateIndex;
            }
            set
            {
                _selectedStateIndex = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedStateIndex)));
            }
        }
        public string Zip
        {
            get
            {
                return _zip;
            }
            set
            {
                _zip = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Zip)));
            }
        }
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
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorMessage)));
            }
        }
        public ICommand SubmitCommand => new Command(async () => await HandleSubmitClick());
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
        public ICommand PurchaseOptionsCommand { get; private set; }

        private readonly List<OptionViewModel> Options = new List<OptionViewModel>()
        {
            new OptionViewModel() { RoofOption = Models.RoofOption.PrimaryOnly, Text = "Primary Roof Only" },
            new OptionViewModel() { RoofOption = Models.RoofOption.RoofDetachedGarage, Text = "Include Detached Garage" },
            new OptionViewModel() { RoofOption = Models.RoofOption.RoofShedBarn, Text = "Include Barn/Shed" }
        };
        private readonly List<StateViewModel> States = new List<StateViewModel>()
        {
            new StateViewModel() {Code = "AL", Text = "Alabama"},
            new StateViewModel() {Code = "AK", Text = "Alaska"},
            new StateViewModel() {Code = "AZ", Text = "Arizona"},
            new StateViewModel() {Code = "AR", Text = "Arkansas"},
            new StateViewModel() {Code = "CA", Text = "California"},
            new StateViewModel() {Code = "CO", Text = "Colorado"},
            new StateViewModel() {Code = "CT", Text = "Connecticut"},
            new StateViewModel() {Code = "DE", Text = "Delaware"},
            new StateViewModel() {Code = "FL", Text = "Florida"},
            new StateViewModel() {Code = "GA", Text = "Georgia"},
            new StateViewModel() {Code = "HI", Text = "Hawaii"},
            new StateViewModel() {Code = "ID", Text = "Idaho"},
            new StateViewModel() {Code = "IL", Text = "Illinois"},
            new StateViewModel() {Code = "IN", Text = "Indiana"},
            new StateViewModel() {Code = "IA", Text = "Iowa"},
            new StateViewModel() {Code = "KS", Text = "Kansas"},
            new StateViewModel() {Code = "KY", Text = "Kentucky"},
            new StateViewModel() {Code = "LA", Text = "Louisiana"},
            new StateViewModel() {Code = "ME", Text = "Maine"},
            new StateViewModel() {Code = "MD", Text = "Maryland"},
            new StateViewModel() {Code = "MA", Text = "Massachusetts"},
            new StateViewModel() {Code = "MI", Text = "Michigan"},
            new StateViewModel() {Code = "MN", Text = "Minnesota"},
            new StateViewModel() {Code = "MS", Text = "Mississippi"},
            new StateViewModel() {Code = "MO", Text = "Missouri"},
            new StateViewModel() {Code = "MT", Text = "Montana"},
            new StateViewModel() {Code = "NE", Text = "Nebraska"},
            new StateViewModel() {Code = "NV", Text = "Nevada"},
            new StateViewModel() {Code = "NH", Text = "New Hampshire"},
            new StateViewModel() {Code = "NJ", Text = "New Jersey"},
            new StateViewModel() {Code = "NM", Text = "New Mexico"},
            new StateViewModel() {Code = "NY", Text = "New York"},
            new StateViewModel() {Code = "NC", Text = "North Carolina"},
            new StateViewModel() {Code = "ND", Text = "North Dakota"},
            new StateViewModel() {Code = "OH", Text = "Ohio"},
            new StateViewModel() {Code = "OK", Text = "Oklahoma"},
            new StateViewModel() {Code = "OR", Text = "Oregon"},
            new StateViewModel() {Code = "PA", Text = "Pennsylvania"},
            new StateViewModel() {Code = "RI", Text = "Rhode Island"},
            new StateViewModel() {Code = "SC", Text = "South Carolina"},
            new StateViewModel() {Code = "SD", Text = "South Dakota"},
            new StateViewModel() {Code = "TN", Text = "Tennessee"},
            new StateViewModel() {Code = "TX", Text = "Texas"},
            new StateViewModel() {Code = "UT", Text = "Utah"},
            new StateViewModel() {Code = "VT", Text = "Vermont"},
            new StateViewModel() {Code = "VA", Text = "Virginia"},
            new StateViewModel() {Code = "WA", Text = "Washington"},
            new StateViewModel() {Code = "WV", Text = "West Virginia"},
            new StateViewModel() {Code = "WI", Text = "Wisconsin"},
            new StateViewModel() {Code = "WY", Text = "Wyoming"}
        };
    }
}
