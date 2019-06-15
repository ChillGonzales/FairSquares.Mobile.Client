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
        private readonly IOrderService _orderService;
        private readonly ISubscriptionService _subService;
        private readonly IPurchasedReportService _prService;
        private readonly ICache<Order> _orderCache;
        private readonly ICache<SubscriptionModel> _subCache;
        private readonly ICache<PurchasedReportModel> _prCache;
        private readonly ILogger<OrderValidationService> _logger;

        public OrderValidationService(IOrderService orderService,
                                      ISubscriptionService subService,
                                      IPurchasedReportService prService,
                                      ICache<Order> orderCache,
                                      ICache<SubscriptionModel> subCache,
                                      ICache<PurchasedReportModel> prCache,
                                      ILogger<OrderValidationService> logger)
        {
            _orderService = orderService;
            _subService = subService;
            _prService = prService;
            _orderCache = orderCache;
            _subCache = subCache;
            _prCache = prCache;
            _logger = logger;
        }

        public async Task<ValidationModel> ValidateOrderRequest(AccountModel user, bool useCached = true)
        {
            try
            {
                IOrderedEnumerable<SubscriptionModel> subs = null;
                List<PurchasedReportModel> purchased = null;
                IEnumerable<Models.Order> orders = null;
                SubscriptionModel lastSub = null;
                if (useCached)
                {
                    subs = _subCache.GetAll().Select(x => x.Value).OrderBy(x => x.StartDateTime);
                    lastSub = subs.LastOrDefault();
                    purchased = _prCache.GetAll().Select(x => x.Value).ToList();
                    orders = _orderCache.GetAll().Select(x => x.Value);
                }
                else
                {
                    subs = _subService.GetSubscriptions(user.UserId).OrderBy(x => x.StartDateTime);
                    lastSub = subs.LastOrDefault();
                    purchased = _prService.GetPurchasedReports(user.UserId);
                    orders = await _orderService.GetMemberOrders(user.UserId);
                    try
                    {
                        _subCache.Put(subs.ToDictionary(x => x.PurchaseId, x => x));
                        _prCache.Put(purchased.ToDictionary(x => x.PurchaseId, x => x));
                        _orderCache.Put(orders.ToDictionary(x => x.OrderId, x => x));
                    }
                    catch { }
                }

                if (!SubscriptionUtility.SubscriptionActive(lastSub) && !orders.Any())
                {
                    return new ValidationModel()
                    {
                        State = ValidationState.FreeReportValid,
                        Message = "User can use their free report.",
                        RemainingOrders = purchased.Count() + 1
                    };
                }
                // Handle case of no sub but purchased reports
                if (!SubscriptionUtility.SubscriptionActive(lastSub) && purchased.Any())
                {
                    if (orders.Count() < (1 + purchased.Count()))
                    {
                        return new ValidationModel()
                        {
                            State = ValidationState.NoSubscriptionAndReportValid,
                            RemainingOrders = (1 + purchased.Count()) - orders.Count(),
                            Message = "User can use their purchased report."
                        };
                    }
                }
                if (!SubscriptionUtility.SubscriptionActive(lastSub) && orders.Any())
                {
                    // This case means they've never had a subscription before, and are eligible for a trial month.
                    if (lastSub == null)
                    {
                        return new ValidationModel()
                        {
                            State = ValidationState.NoSubscriptionAndTrialValid,
                            Message = "User has used their free report, but is eligible for a free trial period."
                        };
                    }
                    return new ValidationModel()
                    {
                        State = ValidationState.NoSubscriptionAndTrialAlreadyUsed,
                        Message = "User does not have a subscription and has used their free report and trial period."
                    };
                }

                // Add 1 to the calculation to take into account free report
                var totalRemainingOrders = subs
                    .Select(x => SubscriptionUtility.GetInfoFromSubType(x.SubscriptionType).OrderCount).Sum() + 1
                                                                                                              + purchased.Count()
                                                                                                              - orders.Count();

                if (totalRemainingOrders <= 0)
                {
                    return new ValidationModel()
                    {
                        State = ValidationState.NoReportsLeftInPeriod,
                        RemainingOrders = 0,
                        Subscription = lastSub,
                        Message = "User has used all of their orders for this subscription period."
                    };
                }
                else
                {
                    return new ValidationModel()
                    {
                        State = ValidationState.SubscriptionReportValid,
                        Subscription = lastSub,
                        RemainingOrders = totalRemainingOrders,
                    };
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
