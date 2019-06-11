using MobileClient.Services;
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
        private string _subscriptionTypeLabel;
        private string _remainingOrdersLabel;
        private string _endDateLabel;
        private string _disclaimerLabel;

        public ManageSubscriptionViewModel(ValidationModel model, string runtimePlatform, Action<Uri> openUri)
        {
            _model = model;
            _runtimePlatform = runtimePlatform;
            _openUri = openUri;
            SubscriptionTypeLabel = "   " + _model.Subscription.SubscriptionType.ToString();
            RemainingOrdersLabel = "   " + _model.RemainingOrders.ToString();
            EndDateLabel = "   " + _model.Subscription.EndDateTime.ToString("dddd, dd MMMM yyyy");
            var compName = _runtimePlatform == Device.Android ? "Google" : "Apple";
            var supportUri = _runtimePlatform == Device.Android ? "https://support.google.com/googleplay/answer/7018481" :
                                "https://support.apple.com/en-us/HT202039#subscriptions";
            DisclaimerLabel = $"NOTE: {compName} does not allow subscriptions to be cancelled through the app. This button will open a web browser with instructions on how to cancel from your device.";
            CancelSubCommand = new Command(() => _openUri(new Uri(supportUri)));
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
    }
}
