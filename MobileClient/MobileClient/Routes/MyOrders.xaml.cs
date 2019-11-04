using FairSquares.Measurement.Core.Models;
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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyOrders : ContentPage
    {
        private readonly ICommand _onAppearingBehavior;
        public MyOrders()
        {
            InitializeComponent();
            var vm = new MyOrdersViewModel(App.Container.GetInstance<IOrderService>(),
                                           App.Container.GetInstance<ICache<Models.Order>>(),
                                           App.Container.GetInstance<ICache<PropertyModel>>(),
                                           App.Container.GetInstance<ICache<ImageModel>>(),
                                           App.Container.GetInstance<ILogger<MyOrdersViewModel>>(),
                                           App.Container.GetInstance<ICacheRefresher>(),
                                           App.Container.GetInstance<IOrderValidationService>(),
                                           App.Container.GetInstance<IPageFactory>(),
                                           App.Container.GetInstance<ICurrentUserService>(),
                                           App.Container.GetInstance<LaunchedFromPushModel>(),
                                           new MainThreadNavigator(x => Device.BeginInvokeOnMainThread(x), this.Navigation),
                                           App.Container.GetInstance<IMessagingCenter>(),
                                           x => Device.BeginInvokeOnMainThread(x),
                                           x => (Application.Current.MainPage as BaseTab).NavigateToTab(x));
            _onAppearingBehavior = vm.OnAppearingBehavior;
            BindingContext = vm;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _onAppearingBehavior.Execute(null);
        }
    }
}