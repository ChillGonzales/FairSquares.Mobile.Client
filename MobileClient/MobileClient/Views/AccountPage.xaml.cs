using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage
    {
        private readonly ICurrentUserService _userCache;
        private readonly ICache<SubscriptionModel> _subCache;
        private readonly IOrderValidationService _orderValidator;
        private readonly AccountModel _user;
        private readonly ILogger<AccountPage> _logger;

        public AccountPage()
        {
            InitializeComponent();
            _userCache = App.Container.GetInstance<ICurrentUserService>();
            _logger = App.Container.GetInstance<EmailLogger<AccountPage>>();
            _user = _userCache.GetLoggedInAccount();
            _subCache = App.Container.GetInstance<ICache<SubscriptionModel>>();
            _orderValidator = App.Container.GetInstance<IOrderValidationService>();
            _userCache.OnLoggedIn += (s, e) =>
            {
                SetUIToAccount(e.Account);
                SetSubUI(e.Account);
            };
            _userCache.OnLoggedOut += (s, e) =>
            {
                SetUIToAccount(null);
                SetSubUI(null);
            };
            SetUIToAccount(_user);
            SetSubUI(_user);
            LogoutButton.Clicked += LogoutButton_Clicked;
            FeedbackButton.Clicked += FeedbackButton_Clicked;
            SubscribeButton.Clicked += SubscribeButton_Clicked;
        }

        private async void FeedbackButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FeedbackPage());
        }

        protected override void OnAppearing()
        {
            var user = _userCache.GetLoggedInAccount();
            SetUIToAccount(user);
            SetSubUI(user);
        }

        private async void SubscribeButton_Clicked(object sender, EventArgs e)
        {
            if (_userCache.GetLoggedInAccount() == null)
                return;
            try
            {
                var validation = await _orderValidator.ValidateOrderRequest(_userCache.GetLoggedInAccount());
                if (new[] { ValidationState.SubscriptionReportValid, ValidationState.NoReportsLeftInPeriod }.Contains(validation.State))
                {
                    await Navigation.PushAsync(new ManageSubscriptionPage(new ViewModels.ManageSubscriptionViewModel()
                    {
                        RemainingOrders = validation.RemainingOrders,
                        SubscriptionType = validation.Subscription.SubscriptionType,
                        EndDateTime = validation.Subscription.EndDateTime
                    }));
                }
                else
                {
                    await Navigation.PushAsync(new PurchasePageRefactored(DependencyService.Get<IAlertService>(),
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

        private async void LogoutButton_Clicked(object sender, EventArgs e)
        {
            if (_userCache.GetLoggedInAccount() == null)
            {
                await this.Navigation.PushAsync(new LandingPage());
            }
            else
            {
                _userCache.LogOut();
                SetUIToAccount(null);
                SetSubUI(null);
            }
        }

        private async void ToolbarItem_Activated(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new InstructionPage(null, false));
        }

        private void SetUIToAccount(AccountModel account)
        {
            if (account == null)
            {
                EmailLabel.Text = "Log in to manage your account.";
                LogoutButton.Text = "Log In";
                LogoutButton.StyleClass = new List<string>() { "Info" };
            }
            else
            {
                EmailLabel.Text = $"Email: {account.Email}";
                LogoutButton.Text = "Sign Out";
                LogoutButton.StyleClass = new List<string>() { "Danger" };
            }
        }

        private void SetSubUI(AccountModel user)
        {
            if (user == null)
            {
                SubscriptionLabel.Text = $"Log in to manage your subscription.";
                SubscribeButton.StyleClass.Clear();
                SubscribeButton.StyleClass.Add("Info");
                SubscribeButton.Text = "Manage";
                SubscribeButton.IsEnabled = false;
                return;
            }
            var valid = Task.Run(async () => await _orderValidator.ValidateOrderRequest(user)).Result;
            if (valid.State == ValidationState.NoReportsLeftInPeriod || valid.State == ValidationState.SubscriptionReportValid)
            {
                SubscriptionLabel.Text = $"Reports remaining this period: {valid.RemainingOrders.ToString()}";
                SubscribeButton.StyleClass.Clear();
                SubscribeButton.StyleClass.Add("Info");
                SubscribeButton.Text = "Manage";
                SubscribeButton.IsEnabled = true;
            }
            else
            {
                SubscriptionLabel.Text = "Purchase a monthly subscription that fits your needs.";
                SubscribeButton.StyleClass = new List<string>() { "Success" };
                SubscribeButton.Text = "Purchase";
                SubscribeButton.IsEnabled = true;
            }
        }
    }
}