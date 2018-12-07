using MobileClient.Models;
using MobileClient.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace MobileClient.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly HttpClient _http;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(HttpClient http, ILogger<SubscriptionService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public void AddSubscription(SubscriptionModel model)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model));
                var result = _http.PostAsync("", content).Result;
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add subscription.", ex, model);
                throw;
            }
        }

        public SubscriptionModel GetSubscription(string userId)
        {
            try
            {
                var result = _http.GetAsync($"?userId={userId}").Result;
                result.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<SubscriptionModel>(result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get subscription.", userId, ex);
                throw;
            }
        }

        public List<SubscriptionTypeModel> GetSubscriptionTypes()
        {
            try
            {
                var result = _http.GetAsync("types").Result;
                result.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<List<SubscriptionTypeModel>>(result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get subscription types.", ex);
                throw;
            }
        }
    }
}
