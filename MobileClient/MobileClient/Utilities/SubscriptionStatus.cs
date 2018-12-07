using System;
using System.Collections.Generic;
using System.Text;
using MobileClient.Models;

namespace MobileClient.Utilities
{
    public class SubscriptionStatus : ISubscriptionStatus
    {

        public SubscriptionStatus(SubscriptionModel model)
        {
            Subscription = model;
        }

        public bool SubscriptionActive
        {
            get
            {
                return Subscription != null;
            }
        }
        public bool FreeTrialActive
        {
            get
            {
                return Subscription == null ? false : Subscription.StartDateTime.AddDays(14) >= DateTimeOffset.Now;
            }
        }
        public SubscriptionModel Subscription { get; set; }
    }
}
