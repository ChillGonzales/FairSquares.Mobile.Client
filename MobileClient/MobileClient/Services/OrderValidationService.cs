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
        private readonly ILogger<OrderValidationService> _logger;

        public OrderValidationService(IOrderService orderService, ISubscriptionService subService, ILogger<OrderValidationService> logger)
        {
            _orderService = orderService;
            _subService = subService;
            _logger = logger;
        }

        public async Task<ValidationModel> ValidateOrderRequest(AccountModel user)
        {
            try
            {
                var subs = _subService.GetSubscriptions(user.UserId).OrderBy(x => x.StartDateTime);
                var lastSub = subs.LastOrDefault();
                var orders = await _orderService.GetMemberOrders(user.UserId);

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
                if (!SubscriptionUtility.SubscriptionActive(lastSub) && !orders.Any())
                {
                    return new ValidationModel()
                    {
                        State = ValidationState.FreeReportValid,
                        Message = "User can use their free report."
                    };
                }

                // Add 1 to the calculation to take into account free report
                var totalRemainingOrders = subs
                    .Select(x => SubscriptionUtility.GetInfoFromSubType(x.SubscriptionType).OrderCount).Sum() - orders.Count() + 1;

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
        FreeReportValid,
        SubscriptionReportValid,
        NoReportsLeftInPeriod
    }
}
