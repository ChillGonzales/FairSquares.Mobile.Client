using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrderConfirmation : ContentPage
    {
        private readonly ICommand _onAppearing;
        public OrderConfirmation(LocationModel loc)
        {
            InitializeComponent();
            var vm = new OrderConfirmationViewModel(loc,
                                                    App.Container.GetInstance<ICurrentUserService>(),
                                                    new AlertUtility((s1, s2, s3, s4) => DisplayAlert(s1, s2, s3, s4),
                                                        (s1, s2, s3) => DisplayAlert(s1, s2, s3)),
                                                    x => (App.Current.MainPage as BaseTab).NavigateToTab(x),
                                                    App.Container.GetInstance<IOrderValidationService>(),
                                                    App.Container.GetInstance<IOrderService>(),
                                                    App.Container.GetInstance<IPageFactory>(),
                                                    App.Container.GetInstance<IMessagingSubscriber>(),
                                                    App.Container.GetInstance<IToastService>(),
                                                    Device.RuntimePlatform,
                                                    new MainThreadNavigator(x => Device.BeginInvokeOnMainThread(x), this.Navigation),
                                                    App.Container.GetInstance<ICache<Models.Order>>(),
                                                    x => this.SubmitButton.StyleClass = new List<string>() { x },
                                                    App.Container.GetInstance<ILogger<OrderConfirmationViewModel>>());
            _onAppearing = vm.OnAppearingBehavior;
            BindingContext = vm;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _onAppearing.Execute(null);
        }
    }
}