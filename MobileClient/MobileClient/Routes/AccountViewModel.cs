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
        private readonly MainThreadNavigator _navigation;
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
        private bool _feedbackButtonEnabled = true;

        public AccountViewModel(ICurrentUserService userCache,
                                IOrderValidationService orderValidator,
                                MainThreadNavigator navigation,
                                IPageFactory pageFactory,
                                Action<string> changeLogInStyleClass,
                                Action<string> changeSubStyleClass,
                                ILogger<AccountViewModel> logger)
        {
            _navigation = navigation;
            _pageFactory = pageFactory;
            _userCache = userCache;
            _logger = logger;
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

        private async Task SetInitialState(AccountModel user)
        {
            ToolbarInfoCommand = new Command(() => _navigation.Push(_pageFactory.GetPage(PageType.Instruction, false)));
            LogOutCommand = new Command(async () =>
            {
                if (_userCache.GetLoggedInAccount() == null)
                {
                    _navigation.Push(_pageFactory.GetPage(PageType.Landing));
                }
                else
                {
                    _userCache.LogOut();
                    SetAccountState(null);
                    await SetSubState(null);
                }
            });
            SubscriptionCommand = new Command(async () =>
            {
                if (_userCache.GetLoggedInAccount() == null)
                    return;
                var valid = await _orderValidator.ValidateOrderRequest(_userCache.GetLoggedInAccount());
                try
                {
                    SubscriptionButtonEnabled = false;
                    if (SubscriptionUtility.SubscriptionActive(valid.Subscription))
                        _navigation.Push(_pageFactory.GetPage(PageType.ManageSubscription, valid));
                    else
                        _navigation.Push(_pageFactory.GetPage(PageType.PurchaseOptions, valid));
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to handle subscription click command.", ex);
                }
                finally
                {
                    SubscriptionButtonEnabled = true;
                }
            });
            FeedbackCommand = new Command(() =>
            {
                try
                {
                    FeedbackButtonEnabled = false;
                    _navigation.Push(_pageFactory.GetPage(PageType.Feedback, _navigation));
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to handle feedback click command.", ex);
                }
                finally
                {
                    FeedbackButtonEnabled = true;
                }
            });
            SetAccountState(user);
            await SetSubState(user, true);
        }

        private void SetAccountState(AccountModel user)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError("Failed to set acccount state.", ex);
            }
        }

        private async Task SetSubState(AccountModel user, bool startUp = false)
        {
            try
            {
                if (user == null)
                {
                    SubscriptionLabel = $"Please log in to view purchasing options.";
                    _changeSubStyleClass("Success");
                    SubscriptionButtonText = "View Options";
                    SubscriptionButtonEnabled = false;
                    return;
                }
                var validity = await _orderValidator.ValidateOrderRequest(user, !startUp);
                var activeSub = SubscriptionUtility.SubscriptionActive(validity.Subscription);
                _changeSubStyleClass(activeSub ? "Info" : "Success");
                SubscriptionButtonText = activeSub ? "Manage" : "View Options";
                SubscriptionButtonEnabled = true;
                if (validity.RemainingOrders > 0)
                {
                    SubscriptionLabel = $"Reports remaining: {validity.RemainingOrders.ToString()}";
                }
                else
                {
                    SubscriptionLabel = $"No reports remaining. " +
                        $"{(activeSub ? "Additional reports can be purchased at a reduced price. Click below to find out more." : "Click below to view purchase options.")}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to set subscription state.", ex);
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
        public bool FeedbackButtonEnabled
        {
            get
            {
                return _feedbackButtonEnabled;
            }
            set
            {
                _feedbackButtonEnabled = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FeedbackButtonEnabled)));
            }
        }
        #endregion
    }
}
