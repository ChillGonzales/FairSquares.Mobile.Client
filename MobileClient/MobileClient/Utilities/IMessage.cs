using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Utilities
{
    public interface IAlertService
    {
        void LongAlert(string message);
        void ShortAlert(string message);
    }
}
