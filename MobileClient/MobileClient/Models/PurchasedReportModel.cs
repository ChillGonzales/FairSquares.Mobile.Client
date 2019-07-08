using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Models
{
    public class PurchasedReportModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public DateTimeOffset PurchasedDateTime { get; set; }
        public string PurchaseId { get; set; }
        public PurchaseSource PurchaseSource { get; set; }
        public string PurchaseToken { get; set; }
        public string Email { get; set; }
    }
}
