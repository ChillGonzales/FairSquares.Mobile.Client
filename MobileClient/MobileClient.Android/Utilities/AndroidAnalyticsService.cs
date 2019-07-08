using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Analytics;
using MobileClient.Droid.Utilities;
using MobileClient.Utility;
using Plugin.CurrentActivity;

// ref: https://stackoverflow.com/questions/48774378/firebase-analytics-in-xamarin-forms/50764452#50764452
[assembly: Xamarin.Forms.Dependency(typeof(AndroidAnalyticsService))]
namespace MobileClient.Droid.Utilities
{
    public class AndroidAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventId)
        {
            LogEvent(eventId, null);
        }

        public void LogEvent(string eventId, string paramName, string value)
        {
            LogEvent(eventId, new Dictionary<string, string>
            {
                {paramName, value}
            });
        }

        public void LogEvent(string eventId, IDictionary<string, string> parameters)
        {
            // utility method to fix eventId, you can skip it if you are sure to always pass valid eventIds
            eventId = FixEventId(eventId);
            var fireBaseAnalytics = FirebaseAnalytics.GetInstance(CrossCurrentActivity.Current.AppContext);
            if (parameters == null)
            {
                fireBaseAnalytics.LogEvent(eventId, null);
                return;
            }
            var bundle = new Bundle();
            foreach (var item in parameters)
            {
                bundle.PutString(item.Key, item.Value);
            }
            fireBaseAnalytics.LogEvent(eventId, bundle);
        }
        // utility method to fix eventId, you can skip it if you are sure to always pass valid eventIds
        private string FixEventId(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
                return "unknown";

            //remove unwanted characters
            eventId = Regex.Replace(eventId, @"[^a-zA-Z0-9_]+", "_", RegexOptions.Compiled);

            //trim to 40 if needed
            return eventId.Substring(0, Math.Min(40, eventId.Length));
        }
    }
}