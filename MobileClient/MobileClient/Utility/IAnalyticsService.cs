using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Utility
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventId);
        void LogEvent(string eventId, string paramName, string value);
        void LogEvent(string eventId, IDictionary<string, string> parameters);
    }
}
