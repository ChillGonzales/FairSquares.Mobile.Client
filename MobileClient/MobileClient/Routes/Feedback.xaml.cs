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
                                                   new AlertUtility((s1, s2, s3, s4) => DisplayAlert(s1, s2, s3, s4),
                                                    (s1, s2, s3) => DisplayAlert(s1, s2, s3)),
                                                   App.Container.GetInstance<IPageFactory>(),
                                                   App.Container.GetInstance<ILogger<FeedbackViewModel>>(),
                                                   new MainThreadNavigator(x => Device.BeginInvokeOnMainThread(x), this.Navigation));
        }
    }
}