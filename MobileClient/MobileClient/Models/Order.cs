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
        public PlatformType? PlatformType { get; set; }
        public StatusModel Status { get; set; }
        public PositionModel AddressPosition { get; set; }
    }
    public class StatusModel
    {
        public Status Status { get; set; }
        public string Message { get; set; }
    }
    public enum Status
    {
        Pending = 0,
        ActionRequired = 1,
        Completed = 2
    }
    public class PositionModel
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
    public enum PlatformType
    {
        Android = 0,
        iOS = 1
    }
    public enum RoofOption
    {
        PrimaryOnly = 0,
        RoofDetachedGarage = 1,
        RoofShedBarn = 2
    }
    public class OrderEqualityComparer : EqualityComparer<Order>
    {
        public override bool Equals(Order x, Order y)
        {
            return (x.OrderId == y.OrderId
                && x.Fulfilled == y.Fulfilled
                && x.MemberId == y.MemberId);
        }

        public override int GetHashCode(Order obj)
        {
            string hCode = obj.OrderId + obj.Fulfilled.ToString() + obj.MemberId;
            return hCode.GetHashCode();
        }
    }
}
