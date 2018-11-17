using System;
using System.Collections.Generic;
using System.Text;
using FairSquares.Measurement.Core.Models;

namespace MobileClient.Services
{
    public class MockPropertyService : IPropertyService
    {
        public Dictionary<string, PropertyModel> GetProperties(List<string> orderIds)
        {
            var dict = new Dictionary<string, PropertyModel>();
            foreach (var id in orderIds)
            {
                dict.Add(id, new PropertyModel()
                {
                    Address = "TestAddr",
                    PropertyId = "Property" + id,
                    OrderId = id,
                    ReportType = ReportType.Basic,
                    Roofs = new List<RoofModel>()
                    {
                        { new RoofModel() { TotalArea = 1000, TotalSquares = 10, PredominantPitchRise = 8} }
                    }
                });
            }
            return dict;
        }
    }
}
