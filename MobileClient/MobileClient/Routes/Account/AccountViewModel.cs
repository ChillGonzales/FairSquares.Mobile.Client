using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes.Account
{
    public class AccountViewModel : INotifyPropertyChanged
    {
        private readonly INavigation _navigation;
        private readonly ICurrentUserService _userCache;
        private readonly ICache<SubscriptionModel> _subCache;
        private readonly IOrderValidationService _orderValidator;
        private readonly AccountModel _user;
        private readonly ILogger<AccountViewModel> _logger;
        private string _email;
        private string _subscriptionLabel;
        private string _subscriptionButtonText;
        private ObservableCollection<string> _logoutStyleClass;
        private ObservableCollection<string> _subscriptionButtonStyleClass;
        private string _logoutText;

        public AccountViewModel(ICurrentUserService userCache, 
                                ICache<SubscriptionModel> subCache, 
                                IOrderValidationService orderValidator, 
                                INavigation navigation,
                                ILogger<AccountViewModel> logger)
        {
            _navigation = navigation;
            _userCache = userCache;
            _subCache = subCache;
            _orderValidator = orderValidator;
            _user = userCache.GetLoggedInAccount();
            _logger = logger;
            SetInitialState();
        }

        private void SetInitialState()
        {
            ToolbarInfoCommand = new Command(async () => await _navigation.PushAsync(PageFactory.GetPage(PageType.Instruction)));
            SetAccountState(_user);
            LogOutCommand = new Command(() =>
            {
                _userCache.LogOut();
                SetAccountState(null);
            });

            Task.Run(async () => await SetSubState()).Wait();
            SubscriptionCommand = new Command(async () => await SubscriptionPressed());

            OnPageAppearing = new Action(async () => await SetSubState());
            FeedbackCommand = new Command(async () => await _navigation.PushAsync(new FeedbackPage()));
        }

        private void SetAccountState(AccountModel user)
        {
            if (_user == null)
            {
                Email = "Please Log In To Continue";
                LogOutStyleClass = new ObservableCollection<string>() { "Info" };
                LogOutText = "Log In";
            }
            else
            {
                Email = _user.Email;
                LogOutStyleClass = new ObservableCollection<string>() { "Danger" };
                LogOutText = "Sign Out";
            }
        }

        private async Task SetSubState()
        {
            var validity = await _orderValidator.ValidateOrderRequest(_user);
            if (validity.State == ValidationState.NoReportsLeftInPeriod || validity.State == ValidationState.SubscriptionReportValid)
            {
                SubscriptionLabel = $"Reports remaining this period: {validity.RemainingOrders.ToString()}";
                SubscriptionButtonStyleClass = new ObservableCollection<string>() { "Info" };
                SubscriptionButtonText = "Manage";
            }
            else
            {
                SubscriptionLabel = "Purchase a monthly subscription that fits your needs.";
                SubscriptionButtonStyleClass = new ObservableCollection<string>() { "Success" };
                SubscriptionButtonText = "Purchase";
            }
        }

        private async Task SubscriptionPressed()
        {
            try
            {
                var validation = await _orderValidator.ValidateOrderRequest(_userCache.GetLoggedInAccount());
                if (new[] { ValidationState.SubscriptionReportValid, ValidationState.NoReportsLeftInPeriod }.Contains(validation.State))
                {
                    await _navigation.PushAsync(new ManageSubscriptionPage(new ViewModels.ManageSubscriptionViewModel()
                    {
                        RemainingOrders = validation.RemainingOrders,
                        SubscriptionType = validation.Subscription.SubscriptionType,
                        EndDateTime = validation.Subscription.EndDateTime
                    }));
                }
                else
                {
                    await _navigation.PushAsync(new PurchasePage(DependencyService.Get<IAlertService>(),
                                                                          App.Container.GetInstance<IPurchasingService>(),
                                                                          App.Container.GetInstance<ICache<SubscriptionModel>>(),
                                                                          App.Container.GetInstance<ISubscriptionService>(),
                                                                          App.Container.GetInstance<ICurrentUserService>(),
                                                                          validation));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurred attempting to purchase subscription.{ex.ToString()}");
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
        public ObservableCollection<string> LogOutStyleClass
        {
            get
            {
                return _logoutStyleClass;
            }
            private set
            {
                _logoutStyleClass = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LogOutStyleClass)));
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
        public ObservableCollection<string> SubscriptionButtonStyleClass
        {
            get
            {
                return _subscriptionButtonStyleClass;
            }
            private set
            {
                _subscriptionButtonStyleClass = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubscriptionButtonStyleClass)));
            }
        }
        public ICommand FeedbackCommand { get; private set; }
        public Action OnPageAppearing { get; private set; }
        #endregion
    }
}
