using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Services;
using Plugin.InAppBilling.Abstractions;
using Xamarin.Forms;

namespace MobileClient.Utilities
{
    public static class SubscriptionUtility
    {
        public const string SUB_NAME_PREMIUM = "premium_subscription_monthly";
        public const string SUB_NAME_BASIC = "basic_subscription_monthly";
        public const string SUB_NAME_ENTERPRISE = "enterprise_subscription_monthly";
        public const string INDV_REPORT_NO_SUB = "no_sub_one_report";
        public const string INDV_REPORT_BASIC = "basic_sub_one_report";
        public const string INDV_REPORT_PREMIUM = "premium_sub_one_report";
        public const string INDV_REPORT_ENTERPRISE = "enterprise_sub_one_report";
        public const double IndvReportNoSubPrice = 9.99;
        public const double IndvReportBasicPrice = 7.99;
        public const double IndvReportPremiumPrice = 5.99;
        public const double IndvReportEnterprisePrice = 3.99;
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
        public static SubscriptionModel GetModelFromIAP(InAppBillingPurchase purchase, AccountModel user, SubscriptionModel previousSub)
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
                UserId = user.UserId,
                Email = user.Email
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
        public static SubscriptionInfo GetInfoFromSubType(SubscriptionType type)
        {
            switch (type)
            {
                case SubscriptionType.Basic:
                    return new SubscriptionInfo()
                    {
                        OrderCount = BasicOrderCount,
                        Price = BasicPrice,
                        SubscriptionCode = SUB_NAME_BASIC
                    };
                case SubscriptionType.Premium:
                    return new SubscriptionInfo()
                    {
                        OrderCount = PremiumOrderCount,
                        Price = PremiumPrice,
                        SubscriptionCode = SUB_NAME_PREMIUM
                    };
                case SubscriptionType.Enterprise:
                    return new SubscriptionInfo()
                    {
                        OrderCount = EnterpriseOrderCount,
                        Price = EnterprisePrice,
                        SubscriptionCode = SUB_NAME_ENTERPRISE
                    };
                default:
                    return new SubscriptionInfo()
                    {
                        OrderCount = BasicOrderCount,
                        Price = BasicPrice,
                        SubscriptionCode = SUB_NAME_BASIC
                    };
            }
        }
        public static SingleReportInfo GetSingleReportInfo(ValidationModel validation)
        {
            if (validation.Subscription == null)
                return new SingleReportInfo()
                {
                    Code = INDV_REPORT_NO_SUB,
                    Price = IndvReportNoSubPrice
                };
            switch (validation.Subscription.SubscriptionType)
            {
                case SubscriptionType.Premium:
                    return new SingleReportInfo()
                    {
                        Code = INDV_REPORT_PREMIUM,
                        Price = IndvReportPremiumPrice
                    };
                case SubscriptionType.Enterprise:
                    return new SingleReportInfo()
                    {
                        Code = INDV_REPORT_ENTERPRISE,
                        Price = IndvReportEnterprisePrice
                    };
                default:
                    return new SingleReportInfo()
                    {
                        Code = INDV_REPORT_BASIC,
                        Price = IndvReportBasicPrice
                    };
            }
        }
        public static bool ValidatePurchaseType(string name, ItemType iapType)
        {
            if (new[] { SUB_NAME_BASIC, SUB_NAME_PREMIUM, SUB_NAME_ENTERPRISE }.Contains(name) && iapType == ItemType.Subscription)
                return true;
            if (new[] { INDV_REPORT_NO_SUB, INDV_REPORT_BASIC, INDV_REPORT_PREMIUM, INDV_REPORT_ENTERPRISE }.Contains(name) && iapType == ItemType.InAppPurchase)
                return true;

            return false;
        }
    }

    public class SingleReportInfo
    {
        public double Price { get; set; }
        public string Code { get; set; }
    }
    public class SubscriptionInfo
    {
        public int OrderCount { get; set; }
        public double Price { get; set; }
        public string SubscriptionCode { get; set; }
    }
}
