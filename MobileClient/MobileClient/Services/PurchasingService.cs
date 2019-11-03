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
        private readonly IInAppBilling _billing;
        private bool _isPurchasing;

        public PurchasingService(IInAppBilling billing, ILogger<PurchasingService> logger)
        {
            _logger = logger;
            _billing = billing;
        }

        public async Task<InAppBillingPurchase> PurchaseItem(string name, ItemType iapType, string payload)
        {
            if (!SubscriptionUtility.ValidatePurchaseType(name, iapType))
                throw new InvalidOperationException($"The item that is attempting to be purchased does not have the proper IAP type.");
            try
            {
                if (_isPurchasing)
                {
                    throw new InvalidOperationException("A different purchase is already in progress. Please try again.");
                }
                _isPurchasing = true;
                bool connected = false;
                try
                {
                    connected = await _billing.ConnectAsync(iapType);
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
                    _logger.LogError($"Can't connect to billing service.", null, name, payload);
                    throw new InvalidOperationException($"Can't connect to billing service. Check connection to internet.");
                }

                InAppBillingPurchase purchase = null;
                try
                {
                    //check purchases
                    purchase = await _billing.PurchaseAsync(name, iapType, payload);
                }
                catch (InAppBillingPurchaseException purchaseEx)
                {
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
                    try
                    {
                        if (purchaseEx.PurchaseError != PurchaseError.UserCancelled)
                            _logger.LogError($"Error type: '{purchaseEx.PurchaseError.ToString()}'. Error Message displayed to user: '{errorMsg}'.",
                                purchaseEx, name, payload);
                    }
                    catch { }
                    throw new InvalidOperationException(errorMsg, purchaseEx);
                }
                catch (Exception ex)
                {
                    // Something else has gone wrong, log it
                    _logger.LogError("Failed to purchase subscription.", ex, name);
                    throw new InvalidOperationException(ex.Message, ex);
                }
                finally
                {
                    await _billing.DisconnectAsync();
                }
                if (purchase == null)
                {
                    // did not purchase
                    _logger.LogError($"Failed to purchase {name}.", null, name);
                    throw new InvalidOperationException("Failed to complete purchase. Please contact support.");
                }
                return purchase;
            }
            catch
            {
                throw;
            }
            finally
            {
                _isPurchasing = false;
            }
        }

        public async Task<IEnumerable<InAppBillingPurchase>> GetPurchases(ItemType iapType)
        {
            bool connected = false;
            try
            {
                connected = await _billing.ConnectAsync(iapType);
            }
            catch (Exception)
            {
                var message = $"Error occurred while try to connect to billing service.";
                throw new InvalidOperationException(message);
            }
            if (!connected)
            {
                throw new InvalidOperationException($"Can't connect to billing service. Check connection to internet.");
            }

            try
            {
                // fetch purchases
                return await _billing.GetPurchasesAsync(iapType) ?? Enumerable.Empty<InAppBillingPurchase>();
            }
            catch (InAppBillingPurchaseException purchaseEx)
            {
                // Billing Exception handle this based on the type
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
                throw new InvalidOperationException(ex.Message, ex);
            }
            finally
            {
                await _billing.DisconnectAsync();
            }
        }
    }
}
