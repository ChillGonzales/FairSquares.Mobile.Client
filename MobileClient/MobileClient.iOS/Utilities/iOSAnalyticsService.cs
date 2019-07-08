using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Firebase.Analytics;
using Foundation;
using MobileClient.iOS.Utilities;
using MobileClient.Utility;
using UIKit;

// ref: https://stackoverflow.com/questions/48774378/firebase-analytics-in-xamarin-forms/50764452#50764452
[assembly: Xamarin.Forms.Dependency(typeof(iOSAnalyticsService))]
namespace MobileClient.iOS.Utilities
{
    public class iOSAnalyticsService : IAnalyticsService
    {
        public void LogEvent(string eventId)
        {
            LogEvent(eventId, (IDictionary<string, string>)null);
        }
       
        public void LogEvent(string eventId, string paramName, string value)
        {
            LogEvent(eventId, new Dictionary<string, string>
            {
                { paramName, value }
            });
        }

        public void LogEvent(string eventId, IDictionary<string, string> parameters)
        {
            // utility method to fix eventId, you can skip it if you are sure to always pass valid eventIds
            eventId = FixEventId(eventId);
            if (parameters == null)
            {
                Analytics.LogEvent(eventId, parameters: null);
                return;
            }

            var keys = new List<NSString>();
            var values = new List<NSString>();
            foreach (var item in parameters)
            {
                keys.Add(new NSString(item.Key));
                values.Add(new NSString(item.Value));
            }

            var parametersDictionary =
                NSDictionary<NSString, NSObject>.FromObjectsAndKeys(values.ToArray(), keys.ToArray(), keys.Count);
            Analytics.LogEvent(eventId, parametersDictionary);
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