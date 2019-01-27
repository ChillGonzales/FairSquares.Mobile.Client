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
                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;
                    else
                        throw new Exception(result.Content.ReadAsStringAsync().Result);
                }
                return JsonConvert.DeserializeObject<SubscriptionModel>(result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get subscription.", userId, ex);
                throw;
            }
        }
    }
}
