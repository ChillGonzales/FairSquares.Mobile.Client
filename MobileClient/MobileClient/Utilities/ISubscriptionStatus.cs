using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Utilities
{
    public interface ISubscriptionStatus
    {
        bool SubscriptionActive { get; }
        bool FreeTrialActive { get; }
        SubscriptionModel Subscription { get; set; }
    }
}
