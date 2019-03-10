using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Firebase.CloudMessaging;
using Foundation;
using MobileClient.iOS.Utilities;
using MobileClient.Utilities;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(MessagingSubscriber))]
namespace MobileClient.iOS.Utilities
{
    public class MessagingSubscriber : IMessagingSubscriber
    {
        public void Subscribe(List<string> topics)
        {
            if (topics == null || !topics.Any())
                return;
            Console.WriteLine("FCM Token: " + Messaging.SharedInstance.FcmToken);
            foreach (var topic in topics)
                Messaging.SharedInstance.Subscribe($"{topic}");
        }

        public void Unsubscribe(List<string> topics)
        {
            if (topics == null || !topics.Any())
                return;
            foreach (var topic in topics)
                Messaging.SharedInstance.Unsubscribe($"{topic}");
        }
    }
}