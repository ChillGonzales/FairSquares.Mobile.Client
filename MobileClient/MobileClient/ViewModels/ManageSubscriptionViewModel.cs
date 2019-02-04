using MobileClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.ViewModels
{
    public class ManageSubscriptionViewModel
    {
        public SubscriptionType SubscriptionType { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public int RemainingOrders { get; set; }
    }
}
