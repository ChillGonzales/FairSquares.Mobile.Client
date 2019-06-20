using MobileClient.Authentication;
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
    public partial class Feedback : ContentPage
    {
        public Feedback()
        {
            InitializeComponent();
            BindingContext = new FeedbackViewModel(App.Container.GetInstance<INotificationService>(),
                                                   App.Container.GetInstance<ICurrentUserService>(),
                                                   new MainThreadNavigator(this.Navigation));
        }
    }
}