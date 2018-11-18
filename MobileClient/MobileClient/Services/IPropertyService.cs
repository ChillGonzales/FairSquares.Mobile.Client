using FairSquares.Measurement.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public interface IPropertyService
    {
        Task<Dictionary<string, PropertyModel>> GetProperties(List<string> orderIds);
    }
}
