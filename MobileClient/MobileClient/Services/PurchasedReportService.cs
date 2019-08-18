using MobileClient.Models;
using MobileClient.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace MobileClient.Services
{
    public class PurchasedReportService : IPurchasedReportService
    {
        private readonly HttpClient _http;
        private readonly string _baseUri;
        private readonly ILogger<PurchasedReportService> _logger;

        public PurchasedReportService(HttpClient http, string baseUri, ILogger<PurchasedReportService> logger)
        {
            _http = http;
            _baseUri = baseUri;
            _logger = logger;
        }

        public void AddPurchasedReport(PurchasedReportModel model)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var result = _http.PostAsync(_baseUri, content).Result;
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add purchased report.", ex, model);
                throw;
            }
        }

        public List<PurchasedReportModel> GetPurchasedReports(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return new List<PurchasedReportModel>();
                var result = _http.GetAsync($"{_baseUri}?userId={userId}").Result;
                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return new List<PurchasedReportModel>();
                    else
                        throw new Exception($"HTTP call failed with status code '{result.StatusCode}' and content '{result.Content.ReadAsStringAsync().Result}'.");
                }
                return JsonConvert.DeserializeObject<List<PurchasedReportModel>>(result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get purchased reports. {ex.Message}", ex, userId);
                throw;
            }
        }
    }
}
