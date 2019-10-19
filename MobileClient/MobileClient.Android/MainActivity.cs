using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using MobileClient.Models;
using Plugin.CurrentActivity;
using Plugin.InAppBilling;
using System;

namespace MobileClient.Droid
{
    [Activity(Label = "Fair Squares", Icon = "@mipmap/ic_launcher", Theme = "@style/splashscreen", Exported = true, LaunchMode = LaunchMode.SingleTop, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        internal static readonly string CHANNEL_ID = "push_notification_channel";
        internal static readonly int NOTIFICATION_ID = 1000;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.Window.RequestFeature(WindowFeatures.ActionBar);
                base.SetTheme(Resource.Style.MainTheme);
                string orderId = null;
                if (Intent.Extras != null)
                {
                    foreach (var key in Intent.Extras.KeySet())
                    {
                        var value = Intent.Extras.GetString(key);
                        Log.Debug("Hi", $"Key: '{key}' Value: '{value}'");
                        if (key == "orderId")
                            orderId = value;
                    }

                }
                TabLayoutResource = Resource.Layout.Tabbar;
                ToolbarResource = Resource.Layout.Toolbar;
                base.OnCreate(savedInstanceState);
                IsPlayServicesAvailable();
                CreateNotificationChannel();
                global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
                Xamarin.Essentials.Platform.Init(this, savedInstanceState);
                global::Xamarin.Auth.Presenters.XamarinAndroid.AuthenticationConfiguration.Init(this, savedInstanceState);
                global::Xamarin.Auth.CustomTabsConfiguration.CustomTabsClosingMessage = null;
                CrossCurrentActivity.Current.Init(this, savedInstanceState);
                Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;
                Fabric.Fabric.With(this, new Crashlytics.Crashlytics());
                Crashlytics.Crashlytics.HandleManagedExceptions();
                LoadApplication(new App());
                App.PushModel.OrderId = orderId;
            }
            catch (Exception ex)
            {
                Log.Error("Error", "Error occurred in OnCreate." + ex.ToString());
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            InAppBillingImplementation.HandleActivityResult(requestCode, resultCode, data);
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

            var channel = new NotificationChannel(CHANNEL_ID, "Order Complete Notifications", NotificationImportance.Default)
            {
                Description = "Order Completion notifications show in this channel."
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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