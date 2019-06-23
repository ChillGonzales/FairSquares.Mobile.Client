using MobileClient.Utilities;
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
        private AccountModel _loggedIn;

        public event EventHandler<LoggedInEventArgs> OnLoggedIn;
        public event EventHandler OnLoggedOut;

        public CurrentUserService()
        {
            _loggedIn = GetLoggedInAccount();
        }

        public AccountModel GetLoggedInAccount()
        {
            if (_loggedIn != null)
                return _loggedIn;

            var token = SecureStorageStore.FindAccountsForServiceAsync(Configuration.GoogleServiceName)?.Result?.FirstOrDefault();
            var model = GetModelFromAccount(token);
            _loggedIn = model;
            return model;
        }

        public void LogIn(Account user)
        {
            _loggedIn = GetModelFromAccount(user);
            SecureStorageStore.SaveAsync(user, Configuration.GoogleServiceName).Wait();
            OnLoggedIn?.Invoke(this, new LoggedInEventArgs() { Account = _loggedIn });
        }

        public void LogOut()
        {
            var acct = SecureStorageStore.FindAccountsForServiceAsync(Configuration.GoogleServiceName)?.Result?.FirstOrDefault();
            SecureStorageStore.SaveAsync(null, Configuration.GoogleServiceName).Wait();
            _loggedIn = null;
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
            SecureStorageStore.SaveAsync(currentAccount, Configuration.GoogleServiceName).Wait();
        }
    }
    public class LoggedInEventArgs : EventArgs
    {
        public AccountModel Account { get; set; }
    }
}
