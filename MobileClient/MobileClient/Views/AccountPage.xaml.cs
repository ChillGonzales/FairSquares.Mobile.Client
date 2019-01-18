using MobileClient.Authentication;
using MobileClient.Services;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly ICurrentUserService _userCache;
        private readonly AccountModel _user;

        public AccountPage()
        {
            InitializeComponent();
            _userCache = App.Container.GetInstance<ICurrentUserService>();
            _user = _userCache.GetLoggedInAccount();
            _userCache.OnLoggedIn += (s, e) => SetUIToAccount(e.Account);
            SetUIToAccount(_user);
            LogoutButton.Clicked += LogoutButton_Clicked;
            FeedbackButton.Clicked += FeedbackButton_Clicked;
            //SaveButton.Clicked += SaveButton_Clicked;
        }

        private async void FeedbackButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FeedbackPage());
        }

        //private void SaveButton_Clicked(object sender, EventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(FirstName.Text) || string.IsNullOrWhiteSpace(LastName.Text))
        //        return;
        //    // TODO: Implement user update
        //}

        private async void LogoutButton_Clicked(object sender, EventArgs e)
        {
            if (_userCache.GetLoggedInAccount() == null)
            {
                await Navigation.PushAsync(new LandingPage());
                return;
            }
            _userCache.LogOut();
            SetUIToAccount(null);
            await Navigation.PushAsync(new LandingPage());
        }

        private async void ToolbarItem_Activated(object sender, EventArgs e)
        {
            await this.Navigation.PushAsync(new InstructionPage());
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