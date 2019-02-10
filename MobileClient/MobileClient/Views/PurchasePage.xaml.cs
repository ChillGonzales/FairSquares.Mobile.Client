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
        private string _successGreen = "449d44";
        private string _infoBlue = "31b0d5";

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
            TryForFreeButton.Clicked += (s, e) => 
            {
                RootPage.NavigateFromMenu(ViewModels.PageType.Order);
            };
            BasicButton.Clicked += (s, e) => { PurchaseSubscription(SubscriptionType.Basic); };
            PremiumButton.Clicked += (s, e) => { PurchaseSubscription(SubscriptionType.Premium); };
            UnlimitedButton.Clicked += (s, e) => { PurchaseSubscription(SubscriptionType.Unlimited); };
            SetFreeReportButton(_showFreeReportButton);
        }
        private async void PurchaseSubscription(SubscriptionType subType)
        {
            try
            {
                ErrorCol.Height = 0;
                var subCode = SubscriptionUtilities.SUB_NAME_BASIC;
                switch (subType)
                {
                    case SubscriptionType.Basic:
                        subCode = SubscriptionUtilities.SUB_NAME_BASIC;
                        break;
                    case SubscriptionType.Premium:
                        subCode = SubscriptionUtilities.SUB_NAME_PREMIUM;
                        break;
                    case SubscriptionType.Unlimited:
                        subCode = SubscriptionUtilities.SUB_NAME_UNLIMITED;
                        break;
                }
#if RELEASE
                var sub = await _purchaseService.PurchaseSubscription(subCode, "payload");
#else
                var sub = new InAppBillingPurchase()
                {
                    PurchaseToken = "PurchaseToken",
                    ProductId = subCode,
                    Id = "12345"
                };
#endif
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
                _alertService.LongAlert($"Thank you for your purchase!");
                Device.BeginInvokeOnMainThread(async () => await Navigation.PopAsync());
                RootPage.NavigateFromMenu(ViewModels.PageType.Order);
            }
            catch (Exception ex)
            {
                ErrorCol.Height = 50;
                ErrorLabel.Text = ex.Message;
            }
        }

        private void SetFreeReportButton(bool showButton)
        {
            // If they've ordered before, hide free report button.
            if (!showButton)
            {
                FreeReportCol.Height = 0;
                FreeReportButtonCol.Height = 0;
                BasicButton.BackgroundColor = Color.FromHex(_successGreen);
            }
            else
            {
                FreeReportCol.Height = GridLength.Star;
                FreeReportButtonCol.Height = GridLength.Star;
                TryForFreeButton.BackgroundColor = Color.FromHex(_successGreen);
                BasicButton.BackgroundColor = Color.FromHex(_infoBlue);
            }
            PremiumButton.BackgroundColor = Color.FromHex(_infoBlue);
            UnlimitedButton.BackgroundColor = Color.FromHex(_infoBlue);
        }
    }

    enum SubscriptionType
    {
        Basic = 0,
        Premium = 1,
        Unlimited = 2
    }
}