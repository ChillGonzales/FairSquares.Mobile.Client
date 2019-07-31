using MobileClient.Authentication;
using MobileClient.Utilities;
using Newtonsoft.Json;
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
        public void LogError(string message, Exception ex, params object[] args)
        {
            try
            {
                var dict = new Dictionary<string, string>();
                dict.Add("source_name", typeof(T).ToString());
                dict.Add("message", message);
                dict.Add("app_platform", Device.RuntimePlatform);
                dict.Add("exception", ex?.Message);
                for (int i = 0; i < args.Length; i++)
                {
                    dict.Add($"metadata_{i}", JsonConvert.SerializeObject(args[i]));
                }
                if (_userCache.GetLoggedInAccount() != null)
                    dict.Add("user_email", _userCache.GetLoggedInAccount().Email);
                _service.LogEvent("app_error", dict);
#if DEBUG
                Debug.WriteLine(message);
                Debug.WriteLine(JsonConvert.SerializeObject(dict));
#endif
            }
            catch (Exception e)
            {
#if DEBUG
                Debug.WriteLine(e.ToString());
#endif
            }
        }
    }
}
