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

        public LandingPage()
        {
            InitializeComponent();
            _auth = App.Container.GetInstance<OAuth2Authenticator>();
            GoogleLogin.Clicked += GoogleLogin_Clicked;
        }

        private void GoogleLogin_Clicked(object sender, EventArgs e)
        {
            var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            presenter.Login(_auth);
        }
    }
}