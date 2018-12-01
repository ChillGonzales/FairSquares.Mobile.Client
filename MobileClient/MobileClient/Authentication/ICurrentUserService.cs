using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Auth;

namespace MobileClient.Authentication
{
    public interface ICurrentUserService
    {
        event EventHandler<LoggedInEventArgs> OnLoggedIn;
        void LogIn(Account user);
        AccountModel GetLoggedInAccount();
        void LogOut();
    }
}
