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

        public async Task<ValidationResponse> ValidateOrderRequest(AccountModel user)
        {
            try
            {
                var sub = _subService.GetSubscription(user.UserId);
                var orders = await _orderService.GetMemberOrders(user.UserId);
                if (!SubscriptionUtilities.SubscriptionActive(sub) && orders.Any())
                {
                    return new ValidationResponse()
                    {
                        Success = false,
                        UserHasSubscription = false,
                        Message = "User does not have a subscription and has used their free report."
                    };
                }
                if (!SubscriptionUtilities.SubscriptionActive(sub) && !orders.Any())
                {
                    return new ValidationResponse()
                    {
                        Success = true,
                        UserHasSubscription = false,
                        Message = "User can use their free report."
                    };
                }
                var activeSubOrderCount = orders.Where(x => x.DateReceived >= sub.StartDateTime && x.DateReceived <= sub.EndDateTime)
                                                .Count();

                if ((sub.SubscriptionType == SubscriptionType.Basic && activeSubOrderCount > SubscriptionUtilities.BasicOrderCount) ||
                    (sub.SubscriptionType == SubscriptionType.Premium && activeSubOrderCount > SubscriptionUtilities.PremiumOrderCount))
                {
                    return new ValidationResponse()
                    {
                        Success = false,
                        UserHasSubscription = true,
                        Message = "User has used all of their orders for this subscription period."
                    };
                }
                else
                {
                    return new ValidationResponse()
                    {
                        Success = true,
                        UserHasSubscription = true
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to validate user's order.", ex);
                // Would rather default to allowing an order than not.
                return new ValidationResponse()
                {
                    Success = true
                };
            }
        }
    }

    public class ValidationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool UserHasSubscription { get; set; }
    }
}
