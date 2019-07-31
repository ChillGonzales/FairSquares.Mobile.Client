using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Utilities
{
    public interface ILogger<T>
    {
        void LogError(string message, Exception ex, params object[] args);
    }
}
