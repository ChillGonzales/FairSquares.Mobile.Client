using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileClient.Authentication;
using MobileClient.Models;
using MobileClient.Utilities;

namespace MobileClient.Services
{
    public class OrderValidationService : IOrderValidationService
    {
        private readonly ICache<Order> _orderCache;
        private readonly ICache<SubscriptionModel> _subCache;
        private readonly ICache<PurchasedReportModel> _prCache;
        private readonly ICacheRefresher _cacheRefresher;
        private readonly ILogger<OrderValidationService> _logger;

        public OrderValidationService(ICacheRefresher cacheRefresher,
                                      ICache<Order> orderCache,
                                      ICache<SubscriptionModel> subCache,
                                      ICache<PurchasedReportModel> prCache,
                                      ILogger<OrderValidationService> logger)
        {
            _cacheRefresher = cacheRefresher;
            _orderCache = orderCache;
            _subCache = subCache;
            _prCache = prCache;
            _logger = logger;
        }

        public async Task<ValidationModel> ValidateOrderRequest(AccountModel user, bool useCached = true)
        {
            try
            {
                List<SubscriptionModel> subs = null;
                List<PurchasedReportModel> purchased = null;
                IEnumerable<Models.Order> orders = null;
                SubscriptionModel lastSub = null;
                if (!useCached)
                    await _cacheRefresher.RefreshCaches(user);
                subs = _subCache.GetAll().Select(x => x.Value).OrderBy(x => x.StartDateTime).ToList();
                lastSub = subs.LastOrDefault();
                purchased = _prCache.GetAll().Select(x => x.Value).ToList();
                orders = _orderCache.GetAll().Select(x => x.Value);

                var totalRemainingOrders = subs
                    .Select(x => SubscriptionUtility.GetInfoFromSubType(x.SubscriptionType).OrderCount).Sum() + 1
                                                                                                              + purchased.Count()
                                                                                                              - orders.Count();
                var model = new ValidationModel()
                {
                    PurchasedReports = purchased,
                    RemainingOrders = totalRemainingOrders
                };

                if (!subs.Any() && !orders.Any())
                {
                    model.State = ValidationState.FreeReportValid;
                    model.Message = "User can use their free report.";
                    return model;
                }
                // Handle case of no sub but purchased reports
                if (!subs.Any() && purchased.Any())
                {
                    if (orders.Count() < (1 + purchased.Count()))
                    {
                        model.State = ValidationState.NoSubscriptionAndReportValid;
                        model.Message = "User can use their purchased report.";
                        return model;
                    }
                }
                if (!subs.Any() && orders.Any())
                {
                    // This case means they've never had a subscription before, and are eligible for a trial month.
                    model.State = ValidationState.NoSubscriptionAndTrialValid;
                    model.Message = "User has used their free report, but is eligible for a free trial period.";
                    return model;
                }
                if (subs.Any() && !SubscriptionUtility.SubscriptionActive(lastSub) && orders.Any())
                {
                    model.State = ValidationState.NoSubscriptionAndTrialAlreadyUsed;
                    model.Message = "User has used their free trial and free report.";
                    return model;
                }

                if (totalRemainingOrders <= 0)
                {
                    model.State = ValidationState.NoReportsLeftInPeriod;
                    model.RemainingOrders = 0;
                    model.Subscription = lastSub;
                    model.Message = "User has used all of their orders for this subscription period.";
                    return model;
                }
                else
                {
                    model.State = ValidationState.SubscriptionReportValid;
                    model.Subscription = lastSub;
                    model.RemainingOrders = totalRemainingOrders;
                    return model;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to validate user's order.", ex);
                throw ex;
            }
        }
    }

    public class ValidationModel
    {
        public ValidationState State { get; set; }
        public string Message { get; set; }
        public int RemainingOrders { get; set; }
        public List<PurchasedReportModel> PurchasedReports { get; set; }
        public SubscriptionModel Subscription { get; set; }
    }

    public enum ValidationState
    {
        NoSubscriptionAndTrialAlreadyUsed,
        NoSubscriptionAndTrialValid,
        NoSubscriptionAndReportValid,
        FreeReportValid,
        SubscriptionReportValid,
        NoReportsLeftInPeriod
    }
}
