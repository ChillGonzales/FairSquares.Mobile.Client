using MobileClient.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Utilities
{
    public interface ICacheRefresher
    {
        Task RefreshCaches(AccountModel user);
        void Invalidate();
        void Revalidate();
        Task RefreshTask { get; }
        bool Invalidated { get; }
    }
}
