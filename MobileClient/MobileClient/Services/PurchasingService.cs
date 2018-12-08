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
            using (var billing = CrossInAppBilling.Current)
            {
                bool connected = false;
                try
                {
                    connected = await billing.ConnectAsync(ItemType.Subscription);
                }
                catch (Exception ex)
                {
                    var message = $"Error occurred while try to connect to billing service.";
                    _logger.LogError(message, ex, name, payload);
                    throw new InvalidOperationException(message);
                }
                if (!connected)
                {
                    // we are offline or can't connect, don't try to purchase
                    _logger.LogError($"Can't connect to billing service.", name, payload);
                    throw new InvalidOperationException($"Can't connect to billing service. Check connection to internet.");
                }

                InAppBillingPurchase purchase = null;
                try
                {
                    //check purchases
                    purchase = await billing.PurchaseAsync(name, ItemType.Subscription, payload);
                }
                catch (InAppBillingPurchaseException purchaseEx)
                {
                    // Billing Exception handle this based on the type
                    _logger.LogError("Billing error occurred.", purchaseEx, name, payload);
                    switch (purchaseEx.PurchaseError)
                    {
                    }
                    throw new InvalidOperationException($"Failed to complete purchase with code '{purchaseEx.PurchaseError}'.");
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
                if (purchase == null)
                {
                    // did not purchase
                    _logger.LogError("Failed to purchase subscription.", name);
                    throw new InvalidOperationException("Failed to complete purchase. Please contact support.");
                }
                return purchase;
            }
        }
    }
}
