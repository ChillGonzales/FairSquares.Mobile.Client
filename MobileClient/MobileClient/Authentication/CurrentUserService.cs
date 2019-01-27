using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using Xamarin.Auth;

namespace MobileClient.Authentication
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly AccountStore _store;
        private AccountModel _loggedIn;
        public event EventHandler<LoggedInEventArgs> OnLoggedIn;
        public event EventHandler OnLoggedOut;

        public CurrentUserService(AccountStore store)
        {
            _store = store;
            _loggedIn = GetLoggedInAccount();
        }

        public AccountModel GetLoggedInAccount()
        {
            if (_loggedIn != null)
                return _loggedIn;

            var token = _store.FindAccountsForService(Configuration.AppName).FirstOrDefault();
            var model = GetModelFromAccount(token);
            _loggedIn = model;
            return model;
        }

        public void LogIn(Account user)
        {
            _loggedIn = GetModelFromAccount(user);
            _store.Save(user, Configuration.AppName);
            OnLoggedIn?.Invoke(this, new LoggedInEventArgs() { Account = _loggedIn });
        }

        public void LogOut()
        {
            try
            {
                var acct = _store.FindAccountsForService(Configuration.AppName).FirstOrDefault();
                _store.Delete(acct, Configuration.AppName);
                _loggedIn = null;
            } 
            catch { }
            OnLoggedOut?.Invoke(this, new EventArgs());
        }

        private AccountModel GetModelFromAccount(Account token)
        {
            if (token == null)
                return null;
            var handler = new JwtSecurityTokenHandler();
            var decoded = handler.ReadToken(token.Properties["id_token"]) as JwtSecurityToken;
            return new AccountModel()
            {
                Token = token,
                Email = decoded.Claims.FirstOrDefault(x => x.Type == "email")?.Value,
                UserId = decoded.Claims.FirstOrDefault(x => x.Type == "sub")?.Value
            };
        }

        private void RefreshAccessToken(Account currentAccount)
        {
            var http = new HttpClient();
            var content = new
            {
                client_id = Configuration.ClientId,
                refresh_token = currentAccount.Properties["refresh_token"],
                grant_type = "refresh_token"
            };
            var response = http.PostAsync($"https://www.googleapis.com/oauth2/v4/token", new StringContent(JsonConvert.SerializeObject(content))).Result;
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content.ReadAsStringAsync().Result);
            currentAccount.Properties["access_token"] = dict["access_token"];
            _store.Save(currentAccount, Configuration.AppName);
        }
    }
    public class LoggedInEventArgs : EventArgs
    {
        public AccountModel Account { get; set; }
    }
}
