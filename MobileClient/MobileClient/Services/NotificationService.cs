using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using MobileClient.Models;
using Newtonsoft.Json;

namespace MobileClient.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly string _baseUri;

        public NotificationService(HttpClient http, string baseUri, string apiKey)
        {
            _http = http;
            _apiKey = apiKey;
            _baseUri = baseUri;
        }

        public void Notify(NotificationRequest request)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var result = _http.PostAsync($"{_baseUri}/notify?accessKey={_apiKey}", content).Result;
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            { }
        }
    }
}
