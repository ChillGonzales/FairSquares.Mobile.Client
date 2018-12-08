using MobileClient.Authentication;
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
        private IPurchasingService _purchaseService;
        private ISubscriptionStatus _subStatus;
        private ISubscriptionService _subService;
        private ICurrentUserService _userCache;
        private const string _subName = "premium_subscription_monthly";

        public PurchasePage()
        {
            Init();
        }
        public PurchasePage(ISubscriptionStatus subStatus)
        {
            Init();

            _subStatus = subStatus;
        }

        private void Init()
        {
            InitializeComponent();

            _subService = App.Container.GetInstance<ISubscriptionService>();
            _purchaseService = App.Container.GetInstance<IPurchasingService>();
            _userCache = App.Container.GetInstance<ICurrentUserService>();
            PurchaseButton.Clicked += PurchaseButton_Clicked;
        }

        private async void PurchaseButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                var sub = await _purchaseService.PurchaseSubscription(_subName, "");
                var model = new Models.SubscriptionModel()
                {
                    PurchaseId = sub.Id,
                    SubscriptionId = "0",
                    StartDateTime = DateTimeOffset.Now,
                    PurchasedDateTime = DateTimeOffset.Now,
                    EndDateTime = DateTimeOffset.Now.AddMonths(1),
                    PurchaseSource = Device.RuntimePlatform == Device.Android ? Models.PurchaseSource.GooglePlay : Models.PurchaseSource.AppStore,
                    UserId = _userCache.GetLoggedInAccount().UserId
                };
                _subService.AddSubscription(model);
                _subStatus.Subscription = model;
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = ex.Message;
            }
        }
    }
}