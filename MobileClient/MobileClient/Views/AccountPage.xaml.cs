using MobileClient.Authentication;
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
            PayButton.Clicked += PayButton_Clicked;
        }

        private async void PayButton_Clicked(object sender, EventArgs e)
        {
            var billing = CrossInAppBilling.Current;
            try
            {
                var connected = await billing.ConnectAsync(ItemType.Subscription);
                if (!connected)
                {
                    //we are offline or can't connect, don't try to purchase
                    return;
                }

                //check purchases
                var purchase = await billing.PurchaseAsync("premium_subscription_monthly", ItemType.Subscription, "payload");

                //possibility that a null came through.
                if (purchase == null)
                {
                    //did not purchase
                    Debug.WriteLine("Did not purchase");
                }
                else
                {
                    //purchased!
                    Debug.WriteLine("Purchased!");
                }
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                //Billing Exception handle this based on the type
                Debug.WriteLine("Error: " + purchaseEx);
            }
            catch (Exception ex)
            {
                //Something else has gone wrong, log it
                Debug.WriteLine("Issue connecting: " + ex);
            }
            finally
            {
                await billing.DisconnectAsync();
            }
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