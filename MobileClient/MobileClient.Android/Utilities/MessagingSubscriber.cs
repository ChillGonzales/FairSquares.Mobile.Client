using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MobileClient.Utilities;
using MobileClient.Droid.Utilities;
using Firebase.Messaging;

[assembly: Xamarin.Forms.Dependency(typeof(MessagingSubscriber))]
namespace MobileClient.Droid.Utilities
{
    public class MessagingSubscriber : IMessagingSubscriber
    {
        public void Subscribe(List<string> topics)
        {
            foreach (var topic in topics)
                FirebaseMessaging.Instance.SubscribeToTopic(topic);
        }
        public void Unsubscribe(List<string> topics)
        {
            foreach (var topic in topics)
                FirebaseMessaging.Instance.UnsubscribeFromTopic(topic);
        }
    }
}