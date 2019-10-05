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

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Routes
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Purchase : ContentPage
    {
        public Purchase(ValidationModel model)
        {
            InitializeComponent();
            BindingContext = new PurchaseViewModel(App.Container.GetInstance<IToastService>(),
                                                   App.Container.GetInstance<IPurchasingService>(),
                                                   App.Container.GetInstance<ICache<SubscriptionModel>>(),
                                                   App.Container.GetInstance<ISubscriptionService>(),
                                                   App.Container.GetInstance<ICurrentUserService>(),
                                                   App.Container.GetInstance<MainThreadNavigator>(),
                                                   model,
                                                   Device.RuntimePlatform,
                                                   page => (Application.Current.MainPage as BaseTab).NavigateToTab(page),
                                                   new AlertUtility((s1, s2, s3, s4) => DisplayAlert(s1, s2, s3, s4),
                                                   (s1, s2, s3) => DisplayAlert(s1, s2, s3)),
                                                   uri => Device.OpenUri(uri),
                                                   registry);
            registry.RegisterPage(this);
        }
    }
}