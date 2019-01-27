using System;
using System.Collections.Generic;
using System.Text;
using MobileClient.Models;
using Plugin.InAppBilling.Abstractions;
using Xamarin.Forms;

namespace MobileClient.Utilities
{
    public static class SubscriptionUtilities
    {
        private const string _subNamePremium = "premium_subscription_monthly";
        private const string _subNameBasic = "basic_subscription_monthly";
        private const string _subNameUnlimited = "unlimited_subscription_monthly";

        public static bool SubscriptionActive(SubscriptionModel model)
        {
            return model != null && model.EndDateTime > DateTimeOffset.Now;
        }
        public static SubscriptionModel GetModelFromIAP(InAppBillingPurchase purchase, string userId, SubscriptionModel previousSub)
        {
            var start = ResolveStartDate(purchase.TransactionDateUtc, previousSub);
            // This is for the case when the last purchase was for the last active month and they have not renewed.
            if (start == null)
                return null;
            return new SubscriptionModel()
            {
                PurchaseId = purchase.Id,
                PurchaseToken = purchase.PurchaseToken,
                StartDateTime = start.Value,
                EndDateTime = start.Value.AddMonths(1),
                PurchasedDateTime = purchase.TransactionDateUtc,
                PurchaseSource = Device.RuntimePlatform == Device.Android ? PurchaseSource.GooglePlay : PurchaseSource.AppStore,
                SubscriptionType = GetTypeFromProductId(purchase.ProductId),
                UserId = userId
            };
        }
        public static DateTimeOffset? ResolveStartDate(DateTime transactionDate, SubscriptionModel previousSub)
        {
            if (previousSub == null)
                return new DateTimeOffset(transactionDate);
            if (transactionDate >= previousSub.StartDateTime)
            {
                return previousSub.StartDateTime.AddMonths(1);
            }
            return null;
        }
        public static SubscriptionType GetTypeFromProductId(string productId)
        {
            switch (productId)
            {
                case _subNameBasic:
                    return SubscriptionType.Basic;
                case _subNamePremium:
                    return SubscriptionType.Premium;
                case _subNameUnlimited:
                    return SubscriptionType.Unlimited;
                default:
                    return SubscriptionType.Basic;
            }
        }
    }
}
