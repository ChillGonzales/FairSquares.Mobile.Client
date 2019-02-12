using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Services
{
    public interface ISubscriptionService
    {
        List<SubscriptionModel> GetSubscriptions(string userId);
        void AddSubscription(SubscriptionModel model);
    }
}