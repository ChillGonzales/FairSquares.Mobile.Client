using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Utilities
{
    public interface ICacheRefresher
    {
        void RefreshCaches(string userId);
    }
}
