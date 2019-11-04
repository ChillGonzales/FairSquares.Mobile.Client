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
        private readonly MainThreadNavigator _nav;
        private readonly ValidationModel _validation;
        private readonly IPageFactory _pageFactory;
        private string _singleReportPrice;
        private string _subscriptionPrice;
        private string _subscriptionHeaderText;

        public PurchaseOptionsViewModel(ValidationModel validation, MainThreadNavigator nav, IPageFactory pageFactory)
        {
            _validation = validation;
            _nav = nav;
            _pageFactory = pageFactory;
            var freeTrial = new[] { ValidationState.NoSubscriptionAndTrialValid, ValidationState.FreeReportValid }.Contains(validation.State);
            SubscriptionHeaderText = freeTrial ? "FREE 30 DAY TRIAL AVAILABLE!" : "Become a Subscriber";
            SingleReportPrice = $"An additional report gives you access to submit an additional order. Price is ${SubscriptionUtility.IndvReportNoSubPrice} per report.";
            var ending = freeTrial ? " after 30-day free trial ends." : ".";
            SubscriptionPrice = $"A subscription plan gives you access to 3 reports every month for $24.99 a month{ending}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand SingleReportCommand => new Command(() => _nav.Push(_pageFactory.GetPage(PageType.SingleReportPurchase, _validation)));
        public ICommand SubscriptionCommand => new Command(() => _nav.Push(_pageFactory.GetPage(PageType.Purchase, _validation)));
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
        public string SubscriptionHeaderText
        {
            get
            {
                return _subscriptionHeaderText;
            }
            set
            {
                _subscriptionHeaderText = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubscriptionHeaderText)));
            }
        }
    }
}
