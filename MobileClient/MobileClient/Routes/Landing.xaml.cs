using MobileClient.Services;
using MobileClient.Utilities;
using MobileClient.Utility;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Landing : ContentPage
    {
        public Landing()
        {
            InitializeComponent();
            BindingContext = new LandingViewModel(App.Container.GetInstance<OAuth2Authenticator>(),
                                                  App.Container.GetInstance<IToastService>(),
                                                  App.Container.GetInstance<ILogger<LandingViewModel>>(),
                                                  new MainThreadNavigator(x => Device.BeginInvokeOnMainThread(x), this.Navigation));
        }
    }
}