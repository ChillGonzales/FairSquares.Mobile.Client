using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class AccountViewModel : INotifyPropertyChanged
    {
        public readonly ICommand OnAppearingBehavior;
        private readonly INavigation _navigation;
        private readonly IPageFactory _pageFactory;
        private readonly ICurrentUserService _userCache;
        private readonly IOrderValidationService _orderValidator;
        private readonly Action<string> _changeLoginStyleClass;
        private readonly Action<string> _changeSubStyleClass;
        private readonly ILogger<AccountViewModel> _logger;
        private string _email;
        private string _subscriptionLabel;
        private string _subscriptionButtonText;
        private string _logoutText;
        private bool _subscriptionButtonEnabled;

        public AccountViewModel(ICurrentUserService userCache,
                                IOrderValidationService orderValidator,
                                INavigation navigation,
                                IPageFactory pageFactory,
                                Action<string> changeLogInStyleClass,
                                Action<string> changeSubStyleClass,
                                ILogger<AccountViewModel> logger)
        {
            _navigation = navigation;
            _pageFactory = pageFactory;
            _userCache = userCache;
            _userCache.OnLoggedIn += async (s, e) =>
            {
                SetAccountState(e.Account);
                await SetSubState(e.Account);
            };
            _userCache.OnLoggedOut += async (s, e) =>
            {
                SetAccountState(null);
                await SetSubState(null);
            };
            _orderValidator = orderValidator;
            _changeLoginStyleClass = changeLogInStyleClass;
            _changeSubStyleClass = changeSubStyleClass;
            var user = userCache.GetLoggedInAccount();
            _logger = logger;
            OnAppearingBehavior = new Command(async () =>
            {
                var u = _userCache.GetLoggedInAccount();
                SetAccountState(u);
                await SetSubState(u);
            });
            SetInitialState(user);
        }

        private void SetInitialState(AccountModel user)
        {
            ToolbarInfoCommand = new Command(async () => await _navigation.PushAsync(_pageFactory.GetPage(PageType.Instruction, false)));
            SetAccountState(user);
            LogOutCommand = new Command(async () =>
            {
                if (_userCache.GetLoggedInAccount() == null)
                {
                    await _navigation.PushAsync(_pageFactory.GetPage(PageType.Landing));
                }
                else
                {
                    _userCache.LogOut();
                    SetAccountState(null);
                    await SetSubState(null);
                }
            });

            Task.Run(async () => await SetSubState(user)).Wait();
            SubscriptionCommand = new Command(async () => await SubscriptionPressed());
            FeedbackCommand = new Command(async () => await _navigation.PushAsync(_pageFactory.GetPage(PageType.Feedback)));
        }

        private void SetAccountState(AccountModel user)
        {
            if (user == null)
            {
                Email = "Please log in to continue";
                _changeLoginStyleClass("Info");
                LogOutText = "Log In";
            }
            else
            {
                Email = user.Email;
                _changeLoginStyleClass("Danger");
                LogOutText = "Sign Out";
            }
        }

        private async Task SetSubState(AccountModel user)
        {
            if (user == null)
            {
                SubscriptionLabel = $"Please log in first.";
                _changeSubStyleClass("Success");
                SubscriptionButtonText = "Manage";
                SubscriptionButtonEnabled = false;
                return;
            }
            var validity = await _orderValidator.ValidateOrderRequest(user);
            if (new[] { ValidationState.NoReportsLeftInPeriod,
                       ValidationState.SubscriptionReportValid,
                       ValidationState.FreeReportValid,
                       ValidationState.NoSubscriptionAndReportValid }.Contains(validity.State))
            {
                SubscriptionLabel = $"Reports remaining: {validity.RemainingOrders.ToString()}";
                _changeSubStyleClass("Info");
                SubscriptionButtonText = "Manage";
                SubscriptionButtonEnabled = true;
            }
            else
            {
                SubscriptionLabel = "View our purchasing options and choose the right one for you.";
                _changeSubStyleClass("Success");
                SubscriptionButtonText = "Learn More";
                SubscriptionButtonEnabled = true;
            }
        }

        private async Task SubscriptionPressed()
        {
            if (_userCache.GetLoggedInAccount() == null)
                return;
            try
            {
                var validation = await _orderValidator.ValidateOrderRequest(_userCache.GetLoggedInAccount());
                if (new[] { ValidationState.SubscriptionReportValid, ValidationState.NoReportsLeftInPeriod }.Contains(validation.State))
                {
                    await _navigation.PushAsync(_pageFactory.GetPage(PageType.ManageSubscription, validation));
                }
                else
                {
                    await _navigation.PushAsync(_pageFactory.GetPage(PageType.PurchaseOptions, validation));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred while navigating to purchase options page.{ex.ToString()}");
            }
        }

        #region Bound Properties
        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand ToolbarInfoCommand { get; private set; }
        public string Email
        {
            get
            {
                return _email;
            }
            private set
            {
                _email = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Email)));
            }
        }
        public ICommand LogOutCommand { get; private set; }
        public string LogOutText
        {
            get
            {
                return _logoutText;
            }
            private set
            {
                _logoutText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogOutText)));
            }
        }
        public string SubscriptionLabel
        {
            get
            {
                return _subscriptionLabel;
            }
            private set
            {
                _subscriptionLabel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubscriptionLabel)));
            }
        }
        public ICommand SubscriptionCommand { get; private set; }
        public string SubscriptionButtonText
        {
            get
            {
                return _subscriptionButtonText;
            }
            private set
            {
                _subscriptionButtonText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubscriptionButtonText)));
            }
        }
        public bool SubscriptionButtonEnabled
        {
            get
            {
                return _subscriptionButtonEnabled;
            }
            private set
            {
                _subscriptionButtonEnabled = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubscriptionButtonEnabled)));
            }
        }
        public ICommand FeedbackCommand { get; private set; }
        #endregion
    }
}
