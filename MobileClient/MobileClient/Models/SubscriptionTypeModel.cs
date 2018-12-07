using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Models
{
    public class SubscriptionTypeModel
    {
        public string SubscriptionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double PricePerMonth { get; set; }
        public double PricePerYear { get; set; }
    }
}
