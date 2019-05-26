using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using MobileClient.ViewModels;
using Plugin.InAppBilling.Abstractions;
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
    public class PurchaseViewModel : INotifyPropertyChanged
    {
        private readonly IAlertService _alertService;
        private readonly IPurchasingService _purchaseService;
        private readonly ICache<SubscriptionModel> _subCache;
        private readonly ISubscriptionService _subService;
        private readonly ICurrentUserService _userCache;
        private readonly ValidationModel _validationModel;
        private readonly string _runtimePlatform;
        private readonly Action<Uri> _openUri;
        private readonly Func<string, string, string, string, Task<bool>> _displayAlert;
        private readonly INavigation _nav;
        private readonly Action<BaseNavPageType> _navigateFromMenu;
        private List<SubscriptionType> _subscriptionSource = new List<SubscriptionType>()
        {
            SubscriptionType.Basic,
            SubscriptionType.Premium,
            SubscriptionType.Enterprise
        };

        // Local binding vars
        private int _selectedSubscriptionIndex;
        private string _marketingDescText;
        private bool _marketingDescVisible;
        private string _reportsDescText;
        private bool _reportsDescVisible;
        private string _costDescText;
        private bool _costDescVisible;
        private string _avgCostText;
        private bool _avgCostVisible;
        private string _purchaseButtonText;
        private bool _purchaseButtonEnabled;
        private bool _loadAnimationRunning;
        private bool _loadAnimationVisible;
        private string _legalText;

        public PurchaseViewModel(IAlertService alertService,
                                 IPurchasingService purchaseService,
                                 ICache<SubscriptionModel> subCache,
                                 ISubscriptionService subService,
                                 ICurrentUserService userCache,
                                 INavigation nav,
                                 ValidationModel validationModel,
                                 string runtimePlatform,
                                 Action<BaseNavPageType> navigateFromMenu,
                                 Func<string, string, string, string, Task<bool>> displayAlert,
                                 Action<Uri> openUri)
        {
            _alertService = alertService;
            _purchaseService = purchaseService;
            _subCache = subCache;
            _subService = subService;
            _userCache = userCache;
            _validationModel = validationModel;
            _runtimePlatform = runtimePlatform;
            _openUri = openUri;
            _displayAlert = displayAlert;
            _nav = nav;
            _navigateFromMenu = navigateFromMenu;
            LegalLinkCommand = new Command(() => _openUri(new Uri(Configuration.PrivacyPolicyUrl)));
            PurchaseButtonCommand = new Command(() => PurchaseButtonClicked());
            SetVisualState(SubscriptionType.Basic, _validationModel);
        }

        private void HandlePickedSubChange(int newIndex)
        {
            var selected = SubscriptionType.Basic;
            try
            {
                selected = _subscriptionSource[newIndex];
            }
            catch { }
            SetVisualState(selected, _validationModel);
        }

        private void SetVisualState(SubscriptionType selected, ValidationModel validation)
        {
            try
            {
                PurchaseButtonText = $"Purchase {selected.ToString()} Plan";
                LegalText = GetLegalJargon(selected, validation);
            }
            catch { }
            try
            {
                ReportsDescVisible = true;
                CostDescVisible = true;
                AvgCostVisible = true;
                PurchaseButtonEnabled = true;
            }
            catch { }
            switch (selected)
            {
                case SubscriptionType.Premium:
                    MarketingDescVisible = true;
                    MarketingDescText = $"A 25% Cost Savings!";
                    ReportsDescText = $"{SubscriptionUtility.PremiumOrderCount} reports per month";
                    CostDescText = $"${SubscriptionUtility.PremiumPrice} per month";
                    AvgCostText = $"Value of $6.25 per report";
                    break;
                case SubscriptionType.Enterprise:
                    MarketingDescVisible = true;
                    MarketingDescText = $"Over 50% in Cost Savings!";
                    ReportsDescText = $"{SubscriptionUtility.EnterpriseOrderCount} reports per month";
                    CostDescText = $"${SubscriptionUtility.EnterprisePrice} per month";
                    AvgCostText = $"Value of $4.00 per report";
                    break;
                default:
                    if (new[] { ValidationState.NoSubscriptionAndTrialValid, ValidationState.FreeReportValid }.Contains(validation.State))
                    {
                        MarketingDescVisible = true;
                        MarketingDescText = $"One month free trial!";
                        CostDescText = $"${SubscriptionUtility.BasicPrice} per month after trial period ends";
                    }
                    else
                    {
                        MarketingDescVisible = false;
                        CostDescText = $"${SubscriptionUtility.BasicPrice} per month";
                    }
                    ReportsDescText = $"{SubscriptionUtility.BasicOrderCount} reports per month";
                    AvgCostText = $"Value of $8.33 per report";
                    break;
            }
        }

        private async void PurchaseButtonClicked()
        {
            LoadAnimationRunning = true;
            LoadAnimationVisible = true;
            PurchaseButtonEnabled = false;
            try
            {

                if (_validationModel.State == ValidationState.FreeReportValid)
                {
                    var result = await _displayAlert("Are you sure?", "You are still eligible for a free report, are you sure you want to continue?", "Ok", "Cancel");
                    if (!result)
                        return;
                }
                SubscriptionType selected = SubscriptionType.Basic;
                try
                {
                    selected = _subscriptionSource[_selectedSubscriptionIndex];
                }
                catch { }

                var subCode = SubscriptionUtility.GetInfoFromSubType(selected).SubscriptionCode;
                try
                {
                    await PurchaseSubscription(subCode);
                    Device.BeginInvokeOnMainThread(async () => await _nav.PopAsync());
                    _alertService.LongAlert($"Thank you for your purchase!");
                    _navigateFromMenu(BaseNavPageType.Order);
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
                    PurchaseButtonEnabled = true;
                    LoadAnimationRunning = false;
                    LoadAnimationVisible = false;
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

        private string GetLegalJargon(SubscriptionType selected, ValidationModel validation)
        {
            string costDesc = "";
            string periodText = (new[] { ValidationState.FreeReportValid, ValidationState.NoSubscriptionAndTrialValid }.Contains(validation.State)
                                && selected == SubscriptionType.Basic)
                                ? "at the end of the trial on"
                                : "upon";
            string platformText = _runtimePlatform == Device.Android ? "Play Store" : "iTunes";
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

        // Bound handlers
        public event PropertyChangedEventHandler PropertyChanged;
        public List<string> SubscriptionOptions => _subscriptionSource.Select(x => x.ToString() + " Subscription").ToList();
        public int SelectedSubscriptionIndex
        {
            get
            {
                return _selectedSubscriptionIndex;
            }
            set
            {
                if (_selectedSubscriptionIndex == value)
                    return;
                _selectedSubscriptionIndex = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSubscriptionIndex)));
                HandlePickedSubChange(value);
            }
        }
        public string MarketingDescText
        {
            get
            {
                return _marketingDescText;
            }
            set
            {
                _marketingDescText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MarketingDescText)));
            }
        }
        public bool MarketingDescVisible
        {
            get
            {
                return _marketingDescVisible;
            }
            set
            {
                _marketingDescVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MarketingDescVisible)));
            }
        }
        public string ReportsDescText
        {
            get
            {
                return _reportsDescText;
            }
            set
            {
                _reportsDescText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReportsDescText)));
            }
        }
        public bool ReportsDescVisible
        {
            get
            {
                return _reportsDescVisible;
            }
            set
            {
                _reportsDescVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReportsDescVisible)));
            }
        }
        public string CostDescText
        {
            get
            {
                return _costDescText;
            }
            set
            {
                _costDescText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CostDescText)));
            }
        }
        public bool CostDescVisible
        {
            get
            {
                return _costDescVisible;
            }
            set
            {
                _costDescVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CostDescVisible)));
            }
        }
        public string AvgCostText
        {
            get
            {
                return _avgCostText;
            }
            set
            {
                _avgCostText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvgCostText)));
            }
        }
        public bool AvgCostVisible
        {
            get
            {
                return _avgCostVisible;
            }
            set
            {
                _avgCostVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AvgCostVisible)));
            }
        }
        public string PurchaseButtonText
        {
            get
            {
                return _purchaseButtonText;
            }
            set
            {
                _purchaseButtonText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PurchaseButtonText)));
            }
        }
        public ICommand PurchaseButtonCommand { get; private set; }
        public bool PurchaseButtonEnabled
        {
            get
            {
                return _purchaseButtonEnabled;
            }
            set
            {
                _purchaseButtonEnabled = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PurchaseButtonEnabled)));
            }
        }
        public bool LoadAnimationRunning
        {
            get
            {
                return _loadAnimationRunning;
            }
            set
            {
                _loadAnimationRunning = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadAnimationRunning)));
            }
        }
        public bool LoadAnimationVisible
        {
            get
            {
                return _loadAnimationVisible;
            }
            set
            {
                _loadAnimationVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadAnimationVisible)));
            }
        }
        public string LegalText
        {
            get
            {
                return _legalText;
            }
            set
            {
                _legalText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LegalText)));
            }
        }
        public ICommand LegalLinkCommand { get; private set; }
    }
}
