using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Iid;
using Firebase.Messaging;

namespace MobileClient.Droid.PushNotifications
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class FirebaseRegistrationService : FirebaseInstanceIdService
    {
        const string TAG = "FirebaseRegistrationService";

        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Log.Debug(TAG, "Refreshed token: " + refreshedToken);
            // TODO: Subscribe to topics?
        }
    }

    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        // ref: https://docs.microsoft.com/en-us/xamarin/android/app-fundamentals/notifications/local-notifications-walkthrough
        // ref: https://stackoverflow.com/questions/37711082/how-to-handle-notification-when-app-in-background-in-firebase
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            if (!message.From.Contains(App.TopicPrefix))
                return;
            message.Data.TryGetValue("title", out var title);
            message.Data.TryGetValue("body", out var body);
            message.Data.TryGetValue("orderId", out var orderId);
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
                return;
            Log.Debug("Push", title);
            Log.Debug("Push", body);
            Log.Debug("Push", orderId);
            var extras = new Bundle();
            extras.PutString(nameof(orderId), orderId);
            var intent = new Intent(this, typeof(MainActivity));
            intent.PutExtras(extras);

            // Construct a back stack for cross-task navigation:
            var stackBuilder = Android.App.TaskStackBuilder.Create(this);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            stackBuilder.AddNextIntent(intent);

            // Create the PendingIntent with the back stack:
            var pendingIntent = stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);
            // Build the notification:
            var builder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                          .SetAutoCancel(true) // Dismiss the notification from the notification area when the user clicks on it
                          .SetContentIntent(pendingIntent)
                          .SetSmallIcon(Resource.Mipmap.ic_launcher) // This is the icon to display
                          .SetContentTitle(title)
                          .SetContentText(body);

            // Finally, publish the notification:
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NOTIFICATION_ID, builder.Build());
        }
    }
}