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
    public partial class SingleReportPurchase : ContentPage
    {
        public SingleReportPurchase(ValidationModel validation)
        {
            InitializeComponent();

            BindingContext = new SingleReportPurchaseViewModel(validation,
                                                               x => Device.OpenUri(x),
                                                               Device.RuntimePlatform,
                                                               x => (App.Current.MainPage as BaseTab).NavigateToTab(x),
                                                               new MainThreadNavigator(this.Navigation),
                                                               App.Container.GetInstance<IToastService>(),
                                                               App.Container.GetInstance<IPurchasedReportService>(),
                                                               App.Container.GetInstance<IPurchasingService>(),
                                                               App.Container.GetInstance<ICurrentUserService>(),
                                                               App.Container.GetInstance<ICache<PurchasedReportModel>>(),
                                                               App.Container.GetInstance<ILogger<SingleReportPurchaseViewModel>>());
        }
    }
}