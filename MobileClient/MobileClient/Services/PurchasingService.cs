using MobileClient.Models;
using MobileClient.Utilities;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public class PurchasingService : IPurchasingService
    {
        private readonly ILogger<PurchasingService> _logger;

        public PurchasingService(ILogger<PurchasingService> logger)
        {
            _logger = logger;
        }

        public async Task<InAppBillingPurchase> PurchaseSubscription(string name, string payload)
        {
            var billing = CrossInAppBilling.Current;
            try
            {
                var connected = await billing.ConnectAsync(ItemType.Subscription);
                if (!connected)
                {
                    //we are offline or can't connect, don't try to purchase
                    _logger.LogError($"Can't connect to billing service.", name, payload);
                    throw new InvalidOperationException($"Can't connect to billing service. Check connection to internet.");
                }

                //check purchases
                var purchase = await billing.PurchaseAsync(name, ItemType.Subscription, payload);

                if (purchase == null)
                {
                    // did not purchase
                    _logger.LogError("Failed to purchase subscription.", name);
                    throw new InvalidOperationException("Failed to complete purchase. Please contact support.");
                }
                return purchase;
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                //Billing Exception handle this based on the type
                _logger.LogError("Billing error occurred.", purchaseEx, name, payload);
                switch (purchaseEx.PurchaseError)
                {
                }
                throw new InvalidOperationException("Failed to complete purchase. Please contact support.");
            }
            catch (Exception ex)
            {
                // Something else has gone wrong, log it
                _logger.LogError("Failed to purchase subscription.", name, ex);
                throw new InvalidOperationException("Failed to complete purchase. Please contact support.");
            }
            finally
            {
                await billing.DisconnectAsync();
            }
        }
    }
}
