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
        public const string SUB_NAME_PREMIUM = "premium_subscription_monthly";
        public const string SUB_NAME_BASIC = "basic_subscription_monthly";
        public const string SUB_NAME_ENTERPRISE = "enterprise_subscription_monthly";
        public const int BasicOrderCount = 3;
        public const int PremiumOrderCount = 8;
        public const int EnterpriseOrderCount = 25;
        public const double BasicPrice = 24.99;
        public const double PremiumPrice = 49.99;
        public const double EnterprisePrice = 99.99;

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
                case SUB_NAME_BASIC:
                    return SubscriptionType.Basic;
                case SUB_NAME_PREMIUM:
                    return SubscriptionType.Premium;
                case SUB_NAME_ENTERPRISE:
                    return SubscriptionType.Enterprise;
                default:
                    return SubscriptionType.Basic;
            }
        }
    }
}
