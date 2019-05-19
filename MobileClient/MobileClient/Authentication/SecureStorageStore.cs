using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Essentials;

namespace MobileClient.Authentication
{
    public static class SecureStorageStore
    {
        public static async Task SaveAsync(Account account, string serviceId)
        {
            // We don't want to update saved user, we just want to PUT. Only 1 user should be cached at a time.
            // Serialize all the accounts to javascript
            var json = JsonConvert.SerializeObject(new List<Account>() { account });

            // Securely save the accounts for the given service
            await SecureStorage.SetAsync(serviceId, json);
        }
        public static async Task<List<Account>> FindAccountsForServiceAsync(string serviceId)
        {
            // Get the json for accounts for the service
            var json = await SecureStorage.GetAsync(serviceId).ConfigureAwait(false);

            try
            {
                // Try to return deserialized list of accounts
                return JsonConvert.DeserializeObject<List<Account>>(json);
            }
            catch { }

            // If this fails, return an empty list
            return new List<Account>();
        }
        public static async Task MigrateAllAccountsAsync(string serviceId, IEnumerable<Account> accountStoreAccounts)
        {
            var wasMigrated = await SecureStorage.GetAsync("XamarinAuthAccountStoreMigrated").ConfigureAwait(false);

            if (wasMigrated == "1")
                return;

            await SecureStorage.SetAsync("XamarinAuthAccountStoreMigrated", "1").ConfigureAwait(false);

            // Just in case, look at existing 'new' accounts
            var accounts = await FindAccountsForServiceAsync(serviceId);

            foreach (var account in accountStoreAccounts)
            {

                // Check if the new storage already has this account
                // We don't want to overwrite it if it does
                if (accounts.Any(a => a.Username == account.Username))
                    continue;

                // Add the account we are migrating
                accounts.Add(account);
            }

            // Serialize all the accounts to javascript
            var json = JsonConvert.SerializeObject(accounts);

            // Securely save the accounts for the given service
            await SecureStorage.SetAsync(serviceId, json).ConfigureAwait(false);
        }
    }
}
