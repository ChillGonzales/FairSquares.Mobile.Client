using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Utilities
{
    public interface ICacheRefresher
    {
        Task RefreshCaches(string userId);
        bool IsRefreshing { get; }
    }
}
