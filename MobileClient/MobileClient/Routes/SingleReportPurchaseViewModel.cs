using MobileClient.Models;
using MobileClient.Services;
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

        public SingleReportPurchaseViewModel(ValidationModel validation, Action<Uri> openUri)
        {
            _validation = validation;
            _openUri = openUri;
        }

        private async Task SetViewState(ValidationModel validation)
        {

        }

        private async Task PurchaseButtonClicked()
        {
            LoadAnimationRunning = true;
            LoadAnimationVisible = true;
            PurchaseButtonEnabled = false;
            try
            {
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
                    _alertService.LongToast($"Thank you for your purchase!");
                    _navigateFromMenu(BaseNavPageType.Order);
                }
                catch (Exception ex)
                {
                    _alertService.LongToast($"Failed to purchase subscription. {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                _alertService.LongToast($"Something went wrong when trying to purchase subscription. {ex.Message}");
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

        private async Task PurchaseItem(string subCode)
        {
            if (_userCache.GetLoggedInAccount() == null)
            {
                throw new InvalidOperationException("User must be logged in to purchase a subscription.");
            }
#if RELEASE
            var sub = await _purchaseService.PurchaseItem(subCode, ItemType.Subscription, "payload");
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
