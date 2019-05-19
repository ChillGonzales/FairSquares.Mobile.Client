using MobileClient.Authentication;
using MobileClient.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LandingPage : ContentPage
    {
        private readonly OAuth2Authenticator _auth;
        private readonly ICurrentUserService _userCache;
        private readonly IAlertService _alertService;

        public LandingPage()
        {
            InitializeComponent();
            _auth = App.Container.GetInstance<OAuth2Authenticator>();
            _userCache = App.Container.GetInstance<ICurrentUserService>();
            _alertService = DependencyService.Get<IAlertService>();
            GoogleLogin.Clicked += GoogleLogin_Clicked;
            _auth.Completed += Auth_Completed;
            _auth.Error += Auth_Error;
        }

        private void Auth_Completed(object sender, AuthenticatorCompletedEventArgs e)
        {
            this.Navigation.PopAsync();
        }
        private void Auth_Error(object sender, AuthenticatorErrorEventArgs e)
        {
            _alertService.LongAlert($"Oops! Looks like there was an issue. Please try again. Error: '{e.Message}'");
            LoadingAnimation.IsVisible = false;
            LoadingAnimation.IsRunning = false;
            LoginLayout.IsVisible = true;
        }
        private void GoogleLogin_Clicked(object sender, EventArgs e)
        {
            LoadingAnimation.IsVisible = true;
            LoadingAnimation.IsRunning = true;
            LoginLayout.IsVisible = false;
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            presenter.Login(_auth);
        }
    }
}