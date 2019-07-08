using Plugin.InAppBilling.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public interface IPurchasingService
    {
        Task<InAppBillingPurchase> PurchaseItem(string name, ItemType iapType, string payload);
        Task<IEnumerable<InAppBillingPurchase>> GetPurchases(ItemType iapType);
    }
}