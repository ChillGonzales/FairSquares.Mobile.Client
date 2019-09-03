using MobileClient.Authentication;
using MobileClient.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Utility
{
    public class LoggerFactory
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ICurrentUserService _user;

        public LoggerFactory(IAnalyticsService analyticsService, ICurrentUserService user)
        {
            _analyticsService = analyticsService;
            _user = user;
        }
        public ILogger<T> Get<T>(bool isDebug = false)
        {
            if (isDebug)
            {
                return new DebugLogger<T>();
            }
            return new AnalyticsLogger<T>(_analyticsService, _user);
        }
    }
}
