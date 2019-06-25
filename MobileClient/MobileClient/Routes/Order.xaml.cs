using MobileClient.Authentication;
using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Order : ContentPage
    {
        private readonly ICommand _onAppearingBehavior;
        public Order()
        {
            InitializeComponent();
            var vm = new OrderViewModel(App.Container.GetInstance<IOrderValidationService>(),
                                                App.Container.GetInstance<ICurrentUserService>(),
                                                App.Container.GetInstance<IOrderService>(),
                                                App.Container.GetInstance<IToastService>(),
                                                App.Container.GetInstance<IPageFactory>(),
                                                new MainThreadNavigator(this.Navigation),
                                                App.Container.GetInstance<IMessagingSubscriber>(),
                                                App.Container.GetInstance<ILogger<OrderViewModel>>(),
                                                Device.RuntimePlatform,
                                                new AlertUtility((s1, s2, s3, s4) => DisplayAlert(s1, s2, s3, s4),
                                                    (s1, s2, s3) => DisplayAlert(s1, s2, s3)),
                                                x => (App.Current.MainPage as BaseTab).NavigateToTab(x),
                                                App.Container.GetInstance<ICache<Models.Order>>());
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