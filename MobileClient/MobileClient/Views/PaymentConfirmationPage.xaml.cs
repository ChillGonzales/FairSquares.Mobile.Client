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
    public partial class PaymentConfirmationPage : ContentPage
    {
        private readonly string _platformName;
        private readonly string _storeName;
        private readonly Action _onComplete;

        public PaymentConfirmationPage(Action onComplete)
        {
            InitializeComponent();
            ConfirmButton.Clicked += OnConfirmButtonClicked;
            _platformName = Device.RuntimePlatform == Device.Android ? "Google" : "iTunes";
            _storeName = Device.RuntimePlatform == Device.Android ? "Play Store" : "App Store";
            Row1.Text = $"- Payment will be charged to your {_platformName} account at confirmation of purchase (except for the trial period).";
            Row2.Text = $"- Your subscription will automatically renew unless auto-renew is turned off at least 24-hours before the end of the current subscription period.";
            Row3.Text = $"- Your account will be charged for renewal within 24-hours prior to the end of the current subscription period. Automatic renewals will cost the same price you were originally charged for the subscription.";
            Row4.Text = $"- You can manage your subscriptions and turn off auto-renewal by going to your Account Settings on the {_storeName} after purchase.";
            Row5.Text = $"- Read our terms of service and privacy policy for more information.";
            _onComplete = onComplete;
        }

        private async void OnConfirmButtonClicked(object sender, EventArgs e)
        {
            _onComplete();
            await this.Navigation.PopAsync();
        }
    }
}