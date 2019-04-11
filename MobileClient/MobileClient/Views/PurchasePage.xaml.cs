using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PurchasePage : ContentPage
    {
        private BaseTabPage RootPage { get => Application.Current.MainPage as BaseTabPage; }
        private IPurchasingService _purchaseService;
        private ICache<SubscriptionModel> _subCache;
        private ISubscriptionService _subService;
        private ICurrentUserService _userCache;
        private IOrderService _orderService;
        private IAlertService _alertService;
        private bool _showFreeReportButton;
        private bool _paymentConfirmationShown = false;
        private string _selectedSubCode;
        private List<LoadingButtonsModel> _loadingButtons;
        private bool _paymentConfirmed = false;

        public PurchasePage(bool showFreeReportButton)
        {
            _showFreeReportButton = showFreeReportButton;
            Init();
        }

        private void Init()
        {
            InitializeComponent();

            _subService = App.Container.GetInstance<ISubscriptionService>();
            _purchaseService = App.Container.GetInstance<IPurchasingService>();
            _userCache = App.Container.GetInstance<ICurrentUserService>();
            _orderService = App.Container.GetInstance<IOrderService>();
            _alertService = DependencyService.Get<IAlertService>();
            _subCache = App.Container.GetInstance<ICache<SubscriptionModel>>();
            ErrorCol.Height = 0;
            TryForFreeButton.Clicked += async (s, e) =>
            {
                await this.Navigation.PopAsync();
                RootPage.NavigateFromMenu(ViewModels.PageType.Order);
            };
            BasicButton.Clicked += (s, e) => { PurchaseSubscription(s as Button, SubscriptionType.Basic); };
            PremiumButton.Clicked += (s, e) => { PurchaseSubscription(s as Button, SubscriptionType.Premium); };
            EnterpriseButton.Clicked += (s, e) => { PurchaseSubscription(s as Button, SubscriptionType.Enterprise); };
            SetFreeReportButton(_showFreeReportButton);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if (_paymentConfirmationShown)
            {
                try
                {
                    _paymentConfirmationShown = false;
                    if (_paymentConfirmed)
                    {
                        _paymentConfirmed = false;
                        await PurchaseSubscription(_selectedSubCode);
                        Device.BeginInvokeOnMainThread(async () => await Navigation.PopAsync());
                        _alertService.LongAlert($"Thank you for your purchase!");
                        RootPage.NavigateFromMenu(ViewModels.PageType.Order);
                    }
                }
                catch (Exception ex)
                {
                    _alertService.LongAlert($"Failed to purchase subscription. Error: {ex.Message}");
                }
                finally
                {
                    foreach (var obj in _loadingButtons)
                    {
                        obj.Button.IsEnabled = true;
                        if (obj.Loader != null)
                        {
                            obj.Loader.IsVisible = false;
                            obj.Loader.IsRunning = false;
                        }
                    }
                }
            }
        }

        private async void PurchaseSubscription(Button sender, SubscriptionType subType)
        {
            _loadingButtons = new List<LoadingButtonsModel>()
            {
                new LoadingButtonsModel() { Button = BasicButton, Loader = BasicLoader, Selected = sender == BasicButton },
                new LoadingButtonsModel() { Button = PremiumButton, Loader = PremiumLoader, Selected = sender == PremiumButton },
                new LoadingButtonsModel() { Button = EnterpriseButton, Loader = EnterpriseLoader, Selected = sender == EnterpriseButton },
                new LoadingButtonsModel() { Button = TryForFreeButton, Loader = null as ActivityIndicator, Selected = sender == TryForFreeButton }
            };
            try
            {
                ErrorCol.Height = 0;
                foreach (var obj in _loadingButtons)
                    obj.Button.IsEnabled = false;
                var selected = _loadingButtons.First(x => x.Selected);
                if (selected.Loader != null)
                {
                    selected.Loader.IsVisible = true;
                    selected.Loader.IsRunning = true;
                }

                var subCode = SubscriptionUtilities.SUB_NAME_BASIC;
                switch (subType)
                {
                    case SubscriptionType.Basic:
                        subCode = SubscriptionUtilities.SUB_NAME_BASIC;
                        break;
                    case SubscriptionType.Premium:
                        subCode = SubscriptionUtilities.SUB_NAME_PREMIUM;
                        break;
                    case SubscriptionType.Enterprise:
                        subCode = SubscriptionUtilities.SUB_NAME_ENTERPRISE;
                        break;
                }
                _selectedSubCode = subCode;
                _paymentConfirmationShown = true;
                await Navigation.PushAsync(new PaymentConfirmationPage(() => _paymentConfirmed = true));
            }
            catch (Exception ex)
            {
                _alertService.LongAlert(ex.Message);
            }
        }

        private async Task PurchaseSubscription(string subCode)
        {
#if RELEASE
            var sub = await _purchaseService.PurchaseSubscription(subCode, "payload");
#else
            var sub = new InAppBillingPurchase()
            {
                PurchaseToken = "PurchaseToken",
                ProductId = subCode,
                Id = "12345"
            };
            await Task.Delay(5000);
#endif
            if (sub == null)
                throw new InvalidOperationException($"Something went wrong when attempting to purchase. Please try again.");

            var model = new Models.SubscriptionModel()
            {
                PurchaseId = sub.Id,
                PurchaseToken = sub.PurchaseToken,
                SubscriptionType = SubscriptionUtilities.GetTypeFromProductId(sub.ProductId),
                StartDateTime = DateTimeOffset.Now,
                PurchasedDateTime = DateTimeOffset.Now,
                EndDateTime = DateTimeOffset.Now.AddMonths(1),
                PurchaseSource = Device.RuntimePlatform == Device.Android ? Models.PurchaseSource.GooglePlay : Models.PurchaseSource.AppStore,
                UserId = _userCache.GetLoggedInAccount().UserId
            };
            _subCache.Put(_userCache.GetLoggedInAccount().UserId, model);
#if RELEASE
                _subService.AddSubscription(model);
#endif
        }

        private void SetFreeReportButton(bool showButton)
        {
            // If they've ordered before, hide free report button.
            if (!showButton)
            {
                FreeReportCol.Height = 0;
                FreeReportButtonCol.Height = 0;
                Pad1.Height = 0;
                BasicButton.StyleClass = new List<string>() { "Success" };
            }
            else
            {
                FreeReportCol.Height = GridLength.Star;
                Pad1.Height = GridLength.Star;
                FreeReportButtonCol.Height = GridLength.Star;
            }
            BasicButton.IsEnabled = !showButton;
            PremiumButton.IsEnabled = !showButton;
            EnterpriseButton.IsEnabled = !showButton;
        }
    }

    internal class LoadingButtonsModel
    {
        public Button Button { get; set; }
        public ActivityIndicator Loader { get; set; }
        public bool Selected { get; set; }
    }
}