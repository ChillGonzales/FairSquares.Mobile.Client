using MobileClient.Authentication;
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
    public partial class AccountPage : ContentPage
    {
        private readonly ICurrentUserService _userService;
        private readonly AccountModel _user;

        public AccountPage()
        {
            InitializeComponent();
            _userService = App.Container.GetInstance<ICurrentUserService>();
            _user = _userService.GetLoggedInAccount();
            _userService.OnLoggedIn += (s, e) => SetUIToAccount(e.Account);
            SetUIToAccount(_user);
            LogoutButton.Clicked += LogoutButton_Clicked;
        }

        private async void LogoutButton_Clicked(object sender, EventArgs e)
        {
            if (_userService.GetLoggedInAccount() == null)
            {
                await Navigation.PushModalAsync(new LandingPage());
                await Navigation.PopAsync();
                return;
            }
            _userService.LogOut();
            SetUIToAccount(null);
            await Navigation.PushModalAsync(new LandingPage());
        }

        private void SetUIToAccount(AccountModel account)
        {
            if (account == null)
            {
                EmailLabel.Text = "Please Log In To Continue";
                LogoutButton.Text = "Log In";
            }
            else
            {
                EmailLabel.Text = account.Email;
                LogoutButton.Text = "Log Out";
            }
        }
    }
}