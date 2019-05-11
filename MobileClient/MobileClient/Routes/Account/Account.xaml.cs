using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using MobileClient.Utilities;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes.Account
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Account : ContentPage
    {
        public Account()
        {
            InitializeComponent();
            BindingContext = new AccountViewModel(App.Container.GetInstance<ICurrentUserService>(),
                                                  App.Container.GetInstance<ICache<SubscriptionModel>>(),
                                                  App.Container.GetInstance<IOrderValidationService>(),
                                                  this.Navigation,
                                                  App.Container.GetInstance<ILogger<AccountViewModel>>());
        }
    }
}