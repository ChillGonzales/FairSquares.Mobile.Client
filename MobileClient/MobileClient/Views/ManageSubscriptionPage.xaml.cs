using MobileClient.Services;
using MobileClient.ViewModels;
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
    public partial class ManageSubscriptionPage : ContentPage
    {
        private readonly ManageSubscriptionViewModel _model;
        private readonly IPurchasingService _purchaseService;

        public ManageSubscriptionPage(ManageSubscriptionViewModel model)
        {
            InitializeComponent();
            _purchaseService = App.Container.GetInstance<IPurchasingService>();
            _model = model;
            SubscriptionTypeLabel.Text += _model.SubscriptionType.ToString();
            RemainingOrdersLabel.Text += _model.RemainingOrders.ToString();
            EndDateLabel.Text += _model.EndDateTime.ToString("dddd, dd MMMM yyyy");
            var compName = Device.RuntimePlatform == Device.Android ? "Google" : "Apple";
            var supportUri = Device.RuntimePlatform == Device.Android ? "https://support.google.com/googleplay/answer/7018481" :
                                "https://support.apple.com/en-us/HT202039#subscriptions";
            DisclaimerLabel.Text = $"NOTE: {compName} does not allow subscriptions to be cancelled through the app. This button will open a web browser with instructions on how to cancel from your device.";
            CancelSubButton.Clicked += (s, e) => Device.OpenUri(new Uri(supportUri));
        }
    }
}