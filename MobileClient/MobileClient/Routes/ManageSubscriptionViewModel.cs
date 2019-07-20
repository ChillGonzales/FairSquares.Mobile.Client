using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileClient.Routes
{
    public class ManageSubscriptionViewModel : INotifyPropertyChanged
    {
        private readonly ValidationModel _model;
        private readonly string _runtimePlatform;
        private readonly Action<Uri> _openUri;
        private readonly MainThreadNavigator _nav;
        private readonly ILogger<ManageSubscriptionViewModel> _logger;
        private readonly IPageFactory _pageFactory;
        private string _subscriptionTypeLabel;
        private string _remainingOrdersLabel;
        private string _endDateLabel;
        private string _disclaimerLabel;
        private string _getMoreReportsLabel;
        private string _purchasedOrdersLabel;

        public ManageSubscriptionViewModel(ValidationModel model,
                                           string runtimePlatform,
                                           Action<Uri> openUri,
                                           MainThreadNavigator nav,
                                           ILogger<ManageSubscriptionViewModel> logger,
                                           IPageFactory pageFactory)
        {
            _model = model;
            _runtimePlatform = runtimePlatform;
            _openUri = openUri;
            _nav = nav;
            _logger = logger;
            _pageFactory = pageFactory;
            try
            {
                SubscriptionTypeLabel = "   " + _model.Subscription.SubscriptionType.ToString();
                RemainingOrdersLabel = "   " + _model.RemainingOrders.ToString();
                PurchasedOrdersLabel = "   " + _model.PurchasedReports?.Count.ToString();
                EndDateLabel = "   " + _model.Subscription.EndDateTime.ToString("dddd, dd MMMM yyyy");
                GetMoreReportsLabel = $"Purchase additional reports at a reduced price of ${SubscriptionUtility.GetSingleReportInfo(_model).Price} per report.";
                GetMoreReportsCommand = new Command(() => _nav.Push(_pageFactory.GetPage(PageType.SingleReportPurchase, _model)));
                var compName = _runtimePlatform == Device.Android ? "Google" : "Apple";
                var supportUri = _runtimePlatform == Device.Android ? "https://support.google.com/googleplay/answer/7018481" :
                                    "https://support.apple.com/en-us/HT202039#subscriptions";
                DisclaimerLabel = $"NOTE: {compName} does not allow subscriptions to be cancelled through the app. This button will open a web browser with instructions on how to cancel from your device.";
                CancelSubCommand = new Command(() => _openUri(new Uri(supportUri)));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load manage subscription page.", ex);
            }
        }

        // Bound properties
        public event PropertyChangedEventHandler PropertyChanged;
        public string SubscriptionTypeLabel
        {
            get
            {
                return _subscriptionTypeLabel;
            }
            set
            {
                _subscriptionTypeLabel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SubscriptionTypeLabel)));
            }
        }
        public string RemainingOrdersLabel
        {
            get
            {
                return _remainingOrdersLabel;
            }
            set
            {
                _remainingOrdersLabel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RemainingOrdersLabel)));
            }
        }
        public string PurchasedOrdersLabel
        {
            get
            {
                return _purchasedOrdersLabel;
            }
            set
            {
                _purchasedOrdersLabel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PurchasedOrdersLabel)));
            }
        }
        public string EndDateLabel
        {
            get
            {
                return _endDateLabel;
            }
            set
            {
                _endDateLabel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EndDateLabel)));
            }
        }
        public string DisclaimerLabel
        {
            get
            {
                return _disclaimerLabel;
            }
            set
            {
                _disclaimerLabel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisclaimerLabel)));
            }
        }
        public ICommand CancelSubCommand { get; set; }
        public string GetMoreReportsLabel
        {
            get
            {
                return _getMoreReportsLabel;
            }
            set
            {
                _getMoreReportsLabel = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GetMoreReportsLabel)));
            }
        }
        public ICommand GetMoreReportsCommand { get; private set; }
    }
}
