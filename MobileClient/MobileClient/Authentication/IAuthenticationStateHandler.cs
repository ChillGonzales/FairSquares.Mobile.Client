using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Authentication
{
    public interface IAuthenticationStateHandler
    {
        void HandleLogin();
        void HandleLogout();
    }
}
