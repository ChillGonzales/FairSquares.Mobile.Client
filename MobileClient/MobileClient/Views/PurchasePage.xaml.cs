using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PurchasePage : ContentPage
    {
        private readonly ValidationModel _validationResponse;
        private List<SubscriptionType> _subscriptionSource = new List<SubscriptionType>()
        {
            SubscriptionType.Basic,
            SubscriptionType.Premium,
            SubscriptionType.Enterprise
        };
        private readonly IAlertService _alertService;
        private readonly IPurchasingService _purchaseService;
        private readonly ICache<SubscriptionModel> _subCache;
        private readonly ISubscriptionService _subService;
        private readonly ICurrentUserService _userCache;

        public PurchasePage(IAlertService alertService,
                                      IPurchasingService purchaseService,
                                      ICache<SubscriptionModel> subCache,
                                      ISubscriptionService subService,
                                      ICurrentUserService userCache,
                                      ValidationModel validationResponse)
        {
            InitializeComponent();
            _validationResponse = validationResponse;
            _alertService = alertService;
            _purchaseService = purchaseService;
            _subCache = subCache;
            _subService = subService;
            _userCache = userCache;

            SubscriptionPicker.ItemsSource = _subscriptionSource.Select(x => x.ToString() + " Subscription").ToList();
            SubscriptionPicker.SelectedIndex = 0;
            SubscriptionPicker.SelectedIndexChanged += SelectedSubscriptionChanged;
            LinkTapGesture.Command = new Command(() => Device.OpenUri(new Uri(Configuration.PrivacyPolicyUrl)));
            PurchaseButton.Clicked += (s, e) => PurchaseButtonClicked();

            SetVisualState(SubscriptionType.Basic, _validationResponse);
        }

        private void SelectedSubscriptionChanged(object sender, EventArgs e)
        {
            var selected = SubscriptionType.Basic;
            try
            {
                selected = _subscriptionSource[SubscriptionPicker.SelectedIndex];
            }
            catch { }
            SetVisualState(selected, _validationResponse);
        }

        private void SetVisualState(SubscriptionType selected, ValidationModel validation)
        {
            try
            {
                PurchaseButton.Text = $"Purchase {selected.ToString()} Plan";
                LegalText.Text = GetLegalJargon(selected, validation);
            }
            catch { }
            switch (selected)
            {
                case SubscriptionType.Premium:
                    MarketingDesc.IsVisible = true;
                    MarketingDesc.Text = $"A 25% Cost Savings!";
                    ReportsDesc.Text = $"{SubscriptionUtility.PremiumOrderCount} reports per month";
                    CostDesc.Text = $"${SubscriptionUtility.PremiumPrice} per month";
                    AvgCostDesc.Text = $"Value of $6.25 per report";
                    break;
                case SubscriptionType.Enterprise:
                    MarketingDesc.IsVisible = true;
                    MarketingDesc.Text = $"Over 50% in Cost Savings!";
                    ReportsDesc.Text = $"{SubscriptionUtility.EnterpriseOrderCount} reports per month";
                    CostDesc.Text = $"${SubscriptionUtility.EnterprisePrice} per month";
                    AvgCostDesc.Text = $"Value of $4.00 per report";
                    break;
                default:
                    if (new[] { ValidationState.NoSubscriptionAndTrialValid, ValidationState.FreeReportValid }.Contains(validation.State))
                    {
                        MarketingDesc.IsVisible = true;
                        MarketingDesc.Text = $"One month free trial!";
                        CostDesc.Text = $"${SubscriptionUtility.BasicPrice} per month after trial period ends";
                    }
                    else
                    {
                        MarketingDesc.IsVisible = false;
                        CostDesc.Text = $"${SubscriptionUtility.BasicPrice} per month";
                    }
                    ReportsDesc.Text = $"{SubscriptionUtility.BasicOrderCount} reports per month";
                    AvgCostDesc.Text = $"Value of $8.33 per report";
                    break;
            }
        }

        private async void PurchaseButtonClicked()
        {
            LoadAnimation.IsRunning = true;
            LoadAnimation.IsVisible = true;
            PurchaseButton.IsEnabled = false;
            try
            {

                if (_validationResponse.State == ValidationState.FreeReportValid)
                {
                    var result = await DisplayAlert("Are you sure?", "You are still eligible for a free report, are you sure you want to continue?", "Ok", "Cancel");
                    if (!result)
                        return;
                }
                SubscriptionType selected = SubscriptionType.Basic;
                try
                {
                    selected = _subscriptionSource[SubscriptionPicker.SelectedIndex];
                }
                catch { }

                var subCode = SubscriptionUtility.GetInfoFromSubType(selected).SubscriptionCode;
                try
                {
                    await PurchaseSubscription(subCode);
                    Device.BeginInvokeOnMainThread(async () => await Navigation.PopAsync());
                    _alertService.LongAlert($"Thank you for your purchase!");
                    (Application.Current.MainPage as BaseTabPage).NavigateFromMenu(ViewModels.BaseNavPageType.Order);
                }
                catch (Exception ex)
                {
                    _alertService.LongAlert($"Failed to purchase subscription. {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _alertService.LongAlert($"Something went wrong when trying to purchase subscription. {ex.Message}");
            }
            finally
            {
                try
                {
                    PurchaseButton.IsEnabled = true;
                    LoadAnimation.IsRunning = false;
                    LoadAnimation.IsVisible = false;
                }
                catch { }
            }
        }

        private async Task PurchaseSubscription(string subCode)
        {
            if (_userCache.GetLoggedInAccount() == null)
            {
                throw new InvalidOperationException("User must be logged in to purchase a subscription.");
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
            await Task.Delay(5000);
#endif
            if (sub == null)
                throw new InvalidOperationException($"Something went wrong when attempting to purchase. Please try again.");

            var model = new Models.SubscriptionModel()
            {
                PurchaseId = sub.Id,
                PurchaseToken = sub.PurchaseToken,
                SubscriptionType = SubscriptionUtility.GetTypeFromProductId(sub.ProductId),
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

        private static string GetLegalJargon(SubscriptionType selected, ValidationModel validation)
        {
            string costDesc = "";
            string periodText = (new[] { ValidationState.FreeReportValid, ValidationState.NoSubscriptionAndTrialValid }.Contains(validation.State)
                                && selected == SubscriptionType.Basic)
                                ? "at the end of the trial on"
                                : "upon";
            string platformText = Device.RuntimePlatform == Device.Android ? "Play Store" : "iTunes";
            switch (selected)
            {
                case SubscriptionType.Premium:
                    costDesc = $"${SubscriptionUtility.PremiumPrice.ToString()} monthly";
                    break;
                case SubscriptionType.Enterprise:
                    costDesc = $"${SubscriptionUtility.EnterprisePrice.ToString()} monthly";
                    break;
                default:
                    costDesc = $"${SubscriptionUtility.BasicPrice.ToString()} monthly";
                    break;
            }
            return $@"A {costDesc} purchase will be applied to your {platformText} account {periodText} confirmation. " +
                    $@"Subscriptions will automatically renew unless canceled within 24-hours before the end of the current period. " +
                    $@"You can cancel anytime with your {platformText} account settings. " +
                    $@"Any unused portion of a free trial will be forfeited if you purchase a subscription. " +
                    $@"For more information, see our terms and conditions on our website ";
        }
    }
}