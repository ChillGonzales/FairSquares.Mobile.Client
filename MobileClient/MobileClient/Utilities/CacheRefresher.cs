﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Utilities
{
    public class CacheRefresher : ICacheRefresher
    {
        private readonly ILogger<CacheRefresher> _logger;
        private readonly Func<string, Task> _refreshFunc;
        public bool IsRefreshing { get; private set; }

        public CacheRefresher(ILogger<CacheRefresher> logger, Func<string, Task> refreshFunc)
        {
            _logger = logger;
            _refreshFunc = refreshFunc;
        }
        public void RefreshCaches(string userId)
        {
            try
            {
                Task.Run(async () =>
                {
                    IsRefreshing = true;
                    await _refreshFunc(userId);
                    IsRefreshing = false;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to refresh caches.{ex.ToString()}");
            }
        }
    }
}
