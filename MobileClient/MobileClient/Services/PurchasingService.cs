using MobileClient.Models;
using MobileClient.Utilities;
using Plugin.InAppBilling;
using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
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
                var errorMsg = "";
                switch (purchaseEx.PurchaseError)
                {
                    case PurchaseError.AlreadyOwned:
                        errorMsg = $"Current user already owns this item.";
                        break;
                    case PurchaseError.AppStoreUnavailable:
                        errorMsg = "The App Store is unavailable. Please check your internet connection and try again.";
                        break;
                    case PurchaseError.BillingUnavailable:
                        errorMsg = "The billing service is unavailable. Please check your internet connection and try again.";
                        break;
                    case PurchaseError.DeveloperError:
                        errorMsg = "A configuration error occurred during billing. Please contact Fair Squares support.";
                        break;
                    case PurchaseError.InvalidProduct:
                        errorMsg = "This product was not found. Please contact Fair Squares support.";
                        break;
                    case PurchaseError.PaymentInvalid:
                        errorMsg = "A payment configuration error occurred during billing. Please contact Fair Squares support.";
                        break;
                    case PurchaseError.PaymentNotAllowed:
                        errorMsg = "Current user is not allowed to authorize payments.";
                        break;
                    case PurchaseError.ProductRequestFailed:
                        errorMsg = "The product request failed. Please try again.";
                        break;
                    case PurchaseError.ServiceUnavailable:
                        errorMsg = "The network connection is not available. Please check your internet connection.";
                        break;
                    case PurchaseError.UserCancelled:
                        errorMsg = "The transaction was cancelled by the user.";
                        break;
                    default:
                        errorMsg = "An error occurred while purchasing subscription. Please contact Fair Squares support.";
                        break;
                }
                throw new InvalidOperationException(errorMsg, purchaseEx);
            }
            catch (Exception ex)
            {
                // Something else has gone wrong, log it
                _logger.LogError("Failed to purchase subscription.", name, ex);
                throw new InvalidOperationException(ex.Message, ex);
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

        public async Task<IEnumerable<InAppBillingPurchase>> GetPurchases()
        {
            var billing = CrossInAppBilling.Current;
            bool connected = false;
            try
            {
                connected = await billing.ConnectAsync(ItemType.Subscription);
            }
            catch (Exception ex)
            {
                var message = $"Error occurred while try to connect to billing service.";
                _logger.LogError(message, ex);
                throw new InvalidOperationException(message);
            }
            if (!connected)
            {
                // we are offline or can't connect, don't try to purchase
                _logger.LogError($"Can't connect to billing service.");
                throw new InvalidOperationException($"Can't connect to billing service. Check connection to internet.");
            }

            try
            {
                // fetch purchases
                return await billing.GetPurchasesAsync(ItemType.Subscription) ?? Enumerable.Empty<InAppBillingPurchase>();
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                // Billing Exception handle this based on the type
                _logger.LogError("Billing error occurred.", purchaseEx);
                var errorMsg = "";
                switch (purchaseEx.PurchaseError)
                {
                    case PurchaseError.AlreadyOwned:
                        errorMsg = $"Current user already owns this item.";
                        break;
                    case PurchaseError.AppStoreUnavailable:
                        errorMsg = "The App Store is unavailable. Please check your internet connection and try again.";
                        break;
                    case PurchaseError.BillingUnavailable:
                        errorMsg = "The billing service is unavailable. Please check your internet connection and try again.";
                        break;
                    case PurchaseError.DeveloperError:
                        errorMsg = "A configuration error occurred during billing. Please contact Fair Squares support.";
                        break;
                    case PurchaseError.InvalidProduct:
                        errorMsg = "This product was not found. Please contact Fair Squares support.";
                        break;
                    case PurchaseError.PaymentInvalid:
                        errorMsg = "A payment configuration error occurred during billing. Please contact Fair Squares support.";
                        break;
                    case PurchaseError.PaymentNotAllowed:
                        errorMsg = "Current user is not allowed to authorize payments.";
                        break;
                    case PurchaseError.ProductRequestFailed:
                        errorMsg = "The product request failed. Please try again.";
                        break;
                    case PurchaseError.ServiceUnavailable:
                        errorMsg = "The network connection is not available. Please check your internet connection.";
                        break;
                    case PurchaseError.UserCancelled:
                        errorMsg = "The transaction was cancelled by the user.";
                        break;
                    default:
                        errorMsg = "An error occurred while purchasing subscription. Please contact Fair Squares support.";
                        break;
                }
                throw new InvalidOperationException(errorMsg, purchaseEx);
            }
            catch (Exception ex)
            {
                // Something else has gone wrong, log it
                _logger.LogError("Failed to get purchases.", ex);
                throw new InvalidOperationException(ex.Message, ex);
            }
            finally
            {
                await billing.DisconnectAsync();
            }
        }
    }
}
