using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Iid;
using Firebase.Messaging;

namespace MobileClient.Droid.PushNotifications
{
    [Service(Name = "com.FairSquares.MobileClient.FirebaseMessagingService")]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class FirebaseRegistrationService : FirebaseMessagingService
    {
        const string TAG = "FirebaseRegistrationService";

        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            Log.Debug(TAG, $"Token '{token}'");
        }
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            foreach (var i in message.Data)
            {
                Log.Debug(TAG, $"Key '{i.Key}' Value '{i.Value}'");
            }
            Log.Debug(TAG, message.From);
        }
    }
}