using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Auth;

namespace MobileClient.Droid
{
    [Activity(Label = "GoogleAuthInterceptor", NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataSchemes = new[] { Configuration.RedirectNoPath },
    DataPaths = new[] { Configuration.RedirectPath })]
    public class GoogleAuthInterceptor : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Convert Android.Net.Url to Uri
            var uri = new Uri(Intent.Data.ToString());

            // Load redirectUrl page
            App.Container.GetInstance<OAuth2Authenticator>().OnPageLoading(uri);

            Task.Run(() =>
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
                StartActivity(intent);
            });

            this.Finish();
            return;
        }
    }
}