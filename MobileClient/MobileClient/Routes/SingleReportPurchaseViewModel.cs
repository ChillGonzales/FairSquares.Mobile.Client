using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.ViewModels;
using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class SingleReportPurchaseViewModel : INotifyPropertyChanged
    {
        private string _marketingDescText;
        private bool _marketingDescVisible;
        private string _reportsDescText;
        private bool _reportsDescVisible;
        private string _costDescText;
        private bool _costDescVisible;
        private string _costComparisonText;
        private bool _costComparisonVisible;
        private string _purchaseButtonText;
        private bool _purchaseButtonEnabled;
        private bool _loadAnimationRunning;
        private bool _loadAnimationVisible;
        private string _legalText;
        private readonly ValidationModel _validation;
        private readonly Action<Uri> _openUri;
        private readonly INavigation _nav;
        private readonly Action<BaseNavPageType> _navigateFromMenu;
        private readonly IToastService _alertService;
        private readonly string _runtimePlatform;
        private readonly ICurrentUserService _userCache;
        private readonly IPurchasingService _purchaseService;
        private readonly IPurchasedReportService _prService;

        public SingleReportPurchaseViewModel(ValidationModel validation,
                                             Action<Uri> openUri,
                                             string runtimePlatform,
                                             Action<BaseNavPageType> navigateFromMenu,
                                             INavigation nav,
                                             IToastService alertService,
                                             IPurchasedReportService prService,
                                             IPurchasingService purchaseService,
                                             ICurrentUserService userCache)
        {
            _validation = validation;
            _openUri = openUri;
            _nav = nav;
            _navigateFromMenu = navigateFromMenu;
            _alertService = alertService;
            _runtimePlatform = runtimePlatform;
            _userCache = userCache;
            _purchaseService = purchaseService;
            _prService = prService;

            SetViewState(validation);
        }

        private async Task SetViewState(ValidationModel validation)
        {
            PurchaseButtonText = $"Purchase Report";
            LegalText = GetLegalJargon(validation);
            ReportsDescVisible = true;
            CostDescVisible = true;
            PurchaseButtonEnabled = true;
            MarketingDescVisible = true;
            MarketingDescText = $"Purchase one additional report.";
            ReportsDescText = $"Adds one available report to use to your account. No expiration, no hassle.";
            CostDescText = $"${SubscriptionUtility.GetSingleReportInfo(validation).Price}";
        }

        private async Task PurchaseButtonClicked()
        {
            LoadAnimationRunning = true;
            LoadAnimationVisible = true;
            PurchaseButtonEnabled = false;
            try
            {
                var code = SubscriptionUtility.GetSingleReportInfo(_validation).Code;
                try
                {
                    await PurchaseItem(code);
                    Device.BeginInvokeOnMainThread(async () => await _nav.PopAsync());
                    _alertService.ShortToast($"Thank you for your purchase!");
                    _navigateFromMenu(BaseNavPageType.Order);
                }
                catch (Exception ex)
                {
                    _alertService.LongToast($"Failed to purchase report. {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _alertService.LongToast($"Something went wrong when trying to purchase report. {ex.Message}");
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

        private async Task PurchaseItem(string itemCode)
        {
            if (_userCache.GetLoggedInAccount() == null)
            {
                throw new InvalidOperationException("User must be logged in to purchase a report.");
            }
#if RELEASE
            var purchase = await _purchaseService.PurchaseItem(itemCode, ItemType.InAppPurchase, "payload");
#else
            var purchase = new InAppBillingPurchase()
            {
                PurchaseToken = "PurchaseToken",
                ProductId = itemCode,
                Id = "12345"
            };
            await Task.Delay(5000);
#endif
            if (purchase == null)
                throw new InvalidOperationException($"Something went wrong when attempting to purchase. Please try again.");

            var model = new Models.PurchasedReportModel()
            {
                PurchaseId = purchase.Id,
                PurchaseToken = purchase.PurchaseToken,
                PurchasedDateTime = DateTimeOffset.Now,
                PurchaseSource = Device.RuntimePlatform == Device.Android ? Models.PurchaseSource.GooglePlay : Models.PurchaseSource.AppStore,
                UserId = _userCache.GetLoggedInAccount().UserId,
                Email = _userCache.GetLoggedInAccount().Email
            };
            _prService.AddPurchasedReport(model);
        }

        private string GetLegalJargon(ValidationModel validation)
        {
            string platformText = _runtimePlatform == Device.Android ? "Play Store" : "iTunes";
            var costDesc = $"${SubscriptionUtility.IndvReportNoSubPrice.ToString()}";
            if (validation.Subscription != null)
            {
                var info = SubscriptionUtility.GetInfoFromSubType(validation.Subscription.SubscriptionType);
                switch (validation.Subscription.SubscriptionType)
                {
                    case SubscriptionType.Premium:
                        costDesc = $"${SubscriptionUtility.IndvReportPremiumPrice.ToString()}";
                        break;
                    case SubscriptionType.Enterprise:
                        costDesc = $"${SubscriptionUtility.IndvReportEnterprisePrice.ToString()}";
                        break;
                    default:
                        costDesc = $"${SubscriptionUtility.IndvReportBasicPrice.ToString()}";
                        break;
                }
            }
            return $@"A {costDesc} purchase will be applied to your {platformText} account upon confirmation. " +
                    $@"For more information, ";
        }

        public event PropertyChangedEventHandler PropertyChanged;

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
        public string CostComparisonText
        {
            get
            {
                return _costComparisonText;
            }
            set
            {
                _costComparisonText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CostComparisonText)));
            }
        }
        public bool CostComparisonVisible
        {
            get
            {
                return _costComparisonVisible;
            }
            set
            {
                _costComparisonVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CostComparisonVisible)));
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
        public ICommand PurchaseButtonCommand => new Command(async () => await PurchaseButtonClicked());
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
        public ICommand LegalLinkCommand => new Command(() => _openUri(new Uri(Configuration.PrivacyPolicyUrl)));

    }
}
