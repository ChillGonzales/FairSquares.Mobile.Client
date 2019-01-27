using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FairSquares.Measurement.Core.Models;
using MobileClient.Utilities;
using Newtonsoft.Json;

namespace MobileClient.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly string _baseUrl;
        private readonly HttpClient _http;
        private readonly ILogger<PropertyService> _logger;

        public PropertyService(string baseUrl, ILogger<PropertyService> logger)
        {
            _baseUrl = baseUrl;
            _logger = logger;
            _http = new HttpClient();
        }

        public async Task<Dictionary<string, PropertyModel>> GetProperties(List<string> orderIds)
        {
            try
            {
                var response = await _http.PostAsync(_baseUrl, new StringContent(JsonConvert.SerializeObject(orderIds), Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"HTTP call returned an error code of '{response.StatusCode}' and message '{response.Content.ReadAsStringAsync().Result}'.");
                var content = JsonConvert.DeserializeObject<List<PropertyModel>>(await response.Content.ReadAsStringAsync() ?? "[]");
                return content.ToDictionary(x => x.OrderId, x => x);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get properties. {ex.ToString()}");
                throw;
            }

        }
    }
}
