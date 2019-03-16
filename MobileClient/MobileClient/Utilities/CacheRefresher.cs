using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Utilities
{
    public class CacheRefresher : ICacheRefresher
    {
        private readonly ILogger<CacheRefresher> _logger;
        private readonly Func<string, Task> _refreshFunc;
        public bool Invalidated { get; private set; }

        public CacheRefresher(ILogger<CacheRefresher> logger, Func<string, Task> refreshFunc)
        {
            _logger = logger;
            _refreshFunc = refreshFunc;
        }

        public async Task RefreshCaches(string userId)
        {
            try
            {
                await _refreshFunc(userId);
                Invalidated = false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to refresh caches.{ex.ToString()}");
            }
        }

        public void Invalidate()
        {
            Invalidated = true;
        }

        public void Revalidate()
        {
            Invalidated = false;
        }
    }
}
