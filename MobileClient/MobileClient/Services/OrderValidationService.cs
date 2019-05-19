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
                var sub = _subService.GetSubscriptions(user.UserId).OrderBy(x => x.StartDateTime).LastOrDefault();
                var orders = await _orderService.GetMemberOrders(user.UserId);

                if (!SubscriptionUtility.SubscriptionActive(sub) && orders.Any())
                {
                    // This case means they've never had a subscription before, and are eligible for a trial month.
                    if (sub == null)
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
                if (!SubscriptionUtility.SubscriptionActive(sub) && !orders.Any())
                {
                    return new ValidationModel()
                    {
                        State = ValidationState.FreeReportValid,
                        Message = "User can use their free report."
                    };
                }
                var activeSubOrderCount = orders.Where(x => x.DateReceived >= sub.StartDateTime && x.DateReceived <= sub.EndDateTime)
                                                .Count();

                if ((sub.SubscriptionType == SubscriptionType.Basic && activeSubOrderCount >= SubscriptionUtility.BasicOrderCount) ||
                    (sub.SubscriptionType == SubscriptionType.Premium && activeSubOrderCount >= SubscriptionUtility.PremiumOrderCount) ||
                    (sub.SubscriptionType == SubscriptionType.Enterprise && activeSubOrderCount >= SubscriptionUtility.EnterpriseOrderCount))
                {
                    return new ValidationModel()
                    {
                        State = ValidationState.NoReportsLeftInPeriod,
                        RemainingOrders = 0,
                        Subscription = sub,
                        Message = "User has used all of their orders for this subscription period."
                    };
                }
                else
                {
                    var remainingCount = sub.SubscriptionType == SubscriptionType.Basic ? SubscriptionUtility.BasicOrderCount - activeSubOrderCount :
                                        (sub.SubscriptionType == SubscriptionType.Premium ? SubscriptionUtility.PremiumOrderCount - activeSubOrderCount :
                                        SubscriptionUtility.EnterpriseOrderCount - activeSubOrderCount);
                    return new ValidationModel()
                    {
                        State = ValidationState.SubscriptionReportValid,
                        Subscription = sub,
                        RemainingOrders = remainingCount,
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
