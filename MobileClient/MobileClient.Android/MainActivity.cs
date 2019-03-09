using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using MobileClient.Authentication;
using Android.Content;
using Plugin.InAppBilling;
using Plugin.CurrentActivity;
using Android.Gms.Common;
using Android.Util;
using Firebase.Iid;
using Firebase.Messaging;

namespace MobileClient.Droid
{
    [Activity(Label = "Fair Squares", Icon = "@mipmap/ic_launcher", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.Window.RequestFeature(WindowFeatures.ActionBar);
                base.SetTheme(Resource.Style.MainTheme);
                if (Intent.Extras != null)
                {
                    foreach (var key in Intent.Extras.KeySet())
                    {
                        var value = Intent.Extras.GetString(key);
                        Log.Debug("DEBUG", $"Key: '{key}' Value: '{value}'");
                    }
                }
                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;

                base.OnCreate(savedInstanceState);

                CreateNotificationChannel();
                IsPlayServicesAvailable();
                global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
                global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, savedInstanceState);
                global::Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;
                CrossCurrentActivity.Current.Init(this, savedInstanceState);
                Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;
                LoadApplication(new App());
                var x = typeof(Xamarin.Forms.Themes.LightThemeResources);
                x = typeof(Xamarin.Forms.Themes.Android.UnderlineEffect);
            }
            catch (Exception ex)
            {
                Log.Error("Error", "Error occurred in OnCreate." + ex.ToString());
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            //InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification 
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID, "FCM Notifications", NotificationImportance.Default)
            {
                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        public bool IsPlayServicesAvailable()
        {
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    Log.Debug("push", GoogleApiAvailability.Instance.GetErrorString(resultCode));
                }
                else
                {
                    Log.Debug("push", "This device is not supported");
                    Finish();
                }

                return false;
            }

            Log.Debug("push", "Google Play Services is available.");
            return true;
        }
    }
}