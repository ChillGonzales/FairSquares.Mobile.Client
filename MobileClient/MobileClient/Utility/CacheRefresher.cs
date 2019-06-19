using MobileClient.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Utilities
{
    public class CacheRefresher : ICacheRefresher
    {
        private readonly ILogger<CacheRefresher> _logger;
        private readonly Func<AccountModel, Task> _refreshFunc;
        public bool Invalidated { get; private set; }

        public CacheRefresher(ILogger<CacheRefresher> logger, Func<AccountModel, Task> refreshFunc)
        {
            _logger = logger;
            _refreshFunc = refreshFunc;
        }

        public async Task RefreshCaches(AccountModel user)
        {
            try
            {
                if (user == null)
                    return;
                await _refreshFunc(user);
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
