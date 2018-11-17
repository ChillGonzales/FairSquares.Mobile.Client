using FairSquares.Measurement.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.Services
{
    public interface IPropertyService
    {
        Dictionary<string, PropertyModel> GetProperties(List<string> orderIds);
    }
}
