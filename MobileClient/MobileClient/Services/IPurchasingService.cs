using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public interface IPurchasingService
    {
        InAppBillingPurchase PurchaseSubscription(string name, string payload);
        IEnumerable<InAppBillingPurchase> GetPurchases();
    }
}