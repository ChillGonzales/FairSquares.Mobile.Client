using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Auth;

namespace MobileClient.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly AccountStore _store;
        private Account _loggedIn;

        public CurrentUserService(AccountStore store)
        {
            _store = store;
            _loggedIn = GetLoggedInAccount();
        }

        public Account GetLoggedInAccount()
        {
            if (_loggedIn != null)
                return _loggedIn;
            return _store.FindAccountsForService(Configuration.AppName).FirstOrDefault();
        }

        public void LogIn(Account user)
        {
            _loggedIn = user;
            _store.Save(user, Configuration.AppName);
        }

        public void LogOut()
        {
            _loggedIn = null;
        }
    }
}
