using System;
using System.Collections.Generic;
using System.Text;
using MobileClient.Models;

namespace MobileClient.Utilities
{
    public static class SubscriptionUtilities
    {
        public static bool SubscriptionActive(SubscriptionModel model)
        {
            return model != null;
        }
    }
}
