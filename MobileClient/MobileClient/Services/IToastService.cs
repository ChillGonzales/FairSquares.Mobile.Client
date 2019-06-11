using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Services
{
    public interface IToastService
    {
        void LongToast(string message);
        void ShortToast(string message);
    }
}
