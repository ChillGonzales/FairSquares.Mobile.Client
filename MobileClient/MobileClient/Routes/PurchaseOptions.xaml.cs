using MobileClient.Services;
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
    public partial class PurchaseOptions : ContentPage
    {
        public PurchaseOptions(ValidationModel validation)
        {
            InitializeComponent();
            BindingContext = new PurchaseOptionsViewModel(validation, new MainThreadNavigator(x => Device.BeginInvokeOnMainThread(x), this.Navigation), App.Container.GetInstance<IPageFactory>());
        }
    }
}