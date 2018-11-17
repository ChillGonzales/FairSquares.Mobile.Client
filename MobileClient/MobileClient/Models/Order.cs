using FairSquares.Measurement.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Models
{
    public class Order
    {
        public string OrderId { get; set; }
        public string MemberId { get; set; }
        public string MemberEmail { get; set; }
        public string StreetAddress { get; set; }
        public ReportType ReportType { get; set; }
        public string Comments { get; set; }
        public RoofOption RoofOption { get; set; }
        public bool Fulfilled { get; set; }
        public DateTimeOffset? DateReceived { get; set; }
        public DateTimeOffset? DateFulfilled { get; set; }
        public int? Price { get; set; }
        public string ChargeId { get; set; }
        public string CouponCodeId { get; set; }
    }
    public enum RoofOption
    {
        PrimaryOnly = 0,
        RoofDetachedGarage = 1,
        RoofShedBarn = 2
    }
}
