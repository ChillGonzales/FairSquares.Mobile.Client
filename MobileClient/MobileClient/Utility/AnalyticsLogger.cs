using MobileClient.Authentication;
using MobileClient.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace MobileClient.Utility
{
    public class AnalyticsLogger<T> : ILogger<T>
    {
        private readonly IAnalyticsService _service;
        private readonly ICurrentUserService _userCache;

        public AnalyticsLogger(IAnalyticsService service, ICurrentUserService userCache)
        {
            _service = service;
            _userCache = userCache;
        }
        public void LogError(string message, params object[] args)
        {
            try
            {
                var dict = new Dictionary<string, string>();
                dict.Add("source_name", typeof(T).ToString());
                dict.Add("message", message);
                dict.Add("app_platform", Device.RuntimePlatform);
                for (int i = 0; i < args.Length; i++)
                {
                    dict.Add($"metadata_{i}", args[i]?.ToString());
                }
                if (_userCache.GetLoggedInAccount() != null)
                    dict.Add("user_email", _userCache.GetLoggedInAccount().Email);
                _service.LogEvent("app_error", dict);
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.ToString());
#endif
            }
        }
    }
}
