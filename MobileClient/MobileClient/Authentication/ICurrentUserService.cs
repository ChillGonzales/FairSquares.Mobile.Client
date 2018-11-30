using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Auth;

namespace MobileClient.Authentication
{
    public interface ICurrentUserService
    {
        void LogIn(Account user);
        Account GetLoggedInAccount();
        void LogOut();
    }
}
