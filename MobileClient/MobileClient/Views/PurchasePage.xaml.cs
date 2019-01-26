using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
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
        private readonly IPurchasingService _purchaseService;
        private readonly ICache<SubscriptionModel> _subCache;
        private readonly ISubscriptionService _subService;
        private readonly ICurrentUserService _userCache;
        private readonly IOrderService _orderService;
        private const string _subNamePremium = "premium_subscription_monthly";
        private const string _subNameBasic = "basic_subscription_monthly";
        private const string _subNameUnlimited = "unlimited_subscription_monthly";

        public PurchasePage()
        {
            InitializeComponent();

            _subService = App.Container.GetInstance<ISubscriptionService>();
            _purchaseService = App.Container.GetInstance<IPurchasingService>();
            _userCache = App.Container.GetInstance<ICurrentUserService>();
            _orderService = App.Container.GetInstance<IOrderService>();
            _subCache = App.Container.GetInstance<ICache<SubscriptionModel>>();
            ErrorCol.Height = 0;
            BasicButton.Clicked += (s, e) => { PurchaseSubscription(SubscriptionType.Basic); };
            PremiumButton.Clicked += (s, e) => { PurchaseSubscription(SubscriptionType.Premium); };
            UnlimitedButton.Clicked += (s, e) => { PurchaseSubscription(SubscriptionType.Unlimited); };
            SetFreeReportButton();
        }

        private async void PurchaseSubscription(SubscriptionType subType)
        {
            try
            {
                ErrorCol.Height = 0;
                var subCode = _subNameBasic;
                switch (subType)
                {
                    case SubscriptionType.Basic:
                        subCode = _subNameBasic;
                        break;
                    case SubscriptionType.Premium:
                        subCode = _subNamePremium;
                        break;
                    case SubscriptionType.Unlimited:
                        subCode = _subNameUnlimited;
                        break;
                }
                var sub = await _purchaseService.PurchaseSubscription(subCode, "");
                var model = new Models.SubscriptionModel()
                {
                    PurchaseId = sub.Id,
                    PurchaseToken = sub.PurchaseToken,
                    SubscriptionId = "0",
                    StartDateTime = DateTimeOffset.Now,
                    PurchasedDateTime = DateTimeOffset.Now,
                    EndDateTime = DateTimeOffset.Now.AddMonths(1),
                    PurchaseSource = Device.RuntimePlatform == Device.Android ? Models.PurchaseSource.GooglePlay : Models.PurchaseSource.AppStore,
                    UserId = _userCache.GetLoggedInAccount().UserId
                };
                _subCache.Put(_userCache.GetLoggedInAccount().UserId, model);
                _subService.AddSubscription(model);
            }
            catch (Exception ex)
            {
                ErrorCol.Height = 50;
                ErrorLabel.Text = ex.Message;
            }
        }

        private void SetFreeReportButton()
        {
            // TODO: Remodel how this is being fetched. It's messy.
            // If they've ordered before, hide free report button.
            var orders = Task.Run(() => _orderService.GetMemberOrders(_userCache.GetLoggedInAccount().UserId)).Result;
            if (orders != null && orders.Any())
            {
                FreeReportCol.Height = 0;
                BasicButton.StyleClass = new List<string>() { "Success" };
            }
            else
            {
                FreeReportCol.Height = GridLength.Star;
                BasicButton.StyleClass = new List<string>() { "Info" };
            }
        }
    }

    enum SubscriptionType
    {
        Basic = 0,
        Premium = 1,
        Unlimited = 2
    }
}