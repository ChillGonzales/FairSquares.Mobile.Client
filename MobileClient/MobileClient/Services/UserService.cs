using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using MobileClient.Models;
using MobileClient.Utilities;
using Newtonsoft.Json;

namespace MobileClient.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _http;
        private readonly ILogger<UserService> _logger;

        public UserService(HttpClient http, ILogger<UserService> logger)
        {
            _http = http;
            _logger = logger;
        }

        public void AddUser(UserModel user)
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(user));
                var result = _http.PostAsync("", content).Result;
                result.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add user.", user, ex);
                throw;
            }
        }

        public UserModel GetUser(string userId)
        {
            try
            {
                var result = _http.GetAsync($"?userId={userId}").Result;
                result.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<UserModel>(result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get user.", userId, ex);
                throw;
            }
        }
    }
}
