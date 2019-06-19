using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class PurchaseOptionsViewModel : INotifyPropertyChanged
    {
        private readonly INavigation _nav;
        private readonly ValidationModel _validation;
        private readonly IPageFactory _pageFactory;
        private string _singleReportPrice;
        private string _subscriptionPrice;
        private bool _isFreeTrialVisible;

        public PurchaseOptionsViewModel(ValidationModel validation, INavigation nav, IPageFactory pageFactory)
        {
            _validation = validation;
            _nav = nav;
            _pageFactory = pageFactory;
            IsFreeTrialVisible = new[] { ValidationState.NoSubscriptionAndTrialValid, ValidationState.FreeReportValid }.Contains(validation.State);
            SingleReportPrice = $"Price is ${SubscriptionUtility.IndvReportNoSubPrice} per report.";
            SubscriptionPrice = $"Plans start at {SubscriptionUtility.BasicOrderCount} reports/month for ${SubscriptionUtility.BasicPrice}.";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SingleReportCommand => new Command(async () => await _nav.PushAsync(_pageFactory.GetPage(PageType.SingleReportPurchase, _validation)));
        public ICommand SubscriptionCommand => new Command(async () => await _nav.PushAsync(_pageFactory.GetPage(PageType.Purchase, _validation)));
        public string SingleReportPrice
        {
            get
            {
                return _singleReportPrice;
            }
            set
            {
                _singleReportPrice = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SingleReportPrice)));
            }
        }
        public string SubscriptionPrice
        {
            get
            {
                return _subscriptionPrice;
            }
            set
            {
                _subscriptionPrice = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubscriptionPrice)));
            }
        }
        public bool IsFreeTrialVisible
        {
            get
            {
                return _isFreeTrialVisible;
            }
            set
            {
                _isFreeTrialVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsFreeTrialVisible)));
            }
        }
    }
}
