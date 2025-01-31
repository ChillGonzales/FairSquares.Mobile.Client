﻿using MobileClient.Models;
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
        private readonly string _baseUri;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(HttpClient http, string baseUri, ILogger<SubscriptionService> logger)
        {
            _http = http;
            _baseUri = baseUri;
            _logger = logger;
        }

        public void AddSubscription(SubscriptionModel model)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");
                var result = _http.PostAsync(_baseUri, content).Result;
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add subscription.", ex, model);
                throw;
            }
        }

        public List<SubscriptionModel> GetSubscriptions(string userId)
        {
            try
            {
                var result = _http.GetAsync($"{_baseUri}?userId={userId}").Result;
                if (!result.IsSuccessStatusCode)
                {
                    if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return new List<SubscriptionModel>();
                    else
                        throw new Exception(result.Content.ReadAsStringAsync().Result);
                }
                return JsonConvert.DeserializeObject<List<SubscriptionModel>>(result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get subscription.", ex, userId);
                throw;
            }
        }
    }
}
