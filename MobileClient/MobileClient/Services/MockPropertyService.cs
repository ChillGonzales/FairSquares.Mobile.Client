using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FairSquares.Measurement.Core.Models;

namespace MobileClient.Services
{
    public class MockPropertyService : IPropertyService
    {
        public async Task<Dictionary<string, PropertyModel>> GetProperties(List<string> orderIds)
        {
            await Task.Delay(5);
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
