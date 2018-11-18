﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MobileClient.Models;
using Newtonsoft.Json;

namespace MobileClient.Services
{
    public class AzureOrderService : IOrderService
    {
        private readonly string _baseEndpoint;
        private readonly string _apiKey;
        private HttpClient _http; 

        public AzureOrderService(string baseEndpoint, string apiKey)
        {
            _baseEndpoint = baseEndpoint;
            _apiKey = apiKey;
            _http = new HttpClient();
        }

        public async Task<string> AddOrder(Order order)
        {
            var result = await _http.PostAsync($"{_baseEndpoint}?accessKey={_apiKey}",
                new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json"));
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("HTTP request to add order failed with error code: " + result.StatusCode);
            }
            else
            {
                return result.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task<IEnumerable<Order>> GetMemberOrders(string memberId)
        {
            var result = await _http.GetAsync($"{_baseEndpoint}/member?memberId={memberId}&accessKey={_apiKey}");
            if (!result.IsSuccessStatusCode)
            {
                throw new Exception("HTTP request to get member's orders failed with error code: " + result.StatusCode);
            }
            else
            {
                return JsonConvert.DeserializeObject<IEnumerable<Order>>(result.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
