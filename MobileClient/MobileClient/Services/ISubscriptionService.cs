using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Services
{
    public interface ISubscriptionService
    {
        SubscriptionModel GetSubscription(string userId);
        void AddSubscription(SubscriptionModel model);
    }
}