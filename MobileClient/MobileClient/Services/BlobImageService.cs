using MobileClient.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MobileClient.Services
{
    public class BlobImageService : IImageService
    {
        private readonly ILogger<BlobImageService> _logger;
        private readonly string _baseUrl;
        private readonly HttpClient _http;

        public BlobImageService(string baseUrl, ILogger<BlobImageService> logger)
        {
            _baseUrl = baseUrl;
            _logger = logger;
            _http = new HttpClient();
        }
        public Dictionary<string, byte[]> GetImages(List<string> orderIds)
        {
            if (orderIds == null || !orderIds.Any())
                throw new ArgumentNullException(nameof(orderIds));
            var tasks = orderIds.Select(x => new { Order = x, Response = _http.GetAsync($"{_baseUrl}/{x}/top.png") });
            Task.WaitAll(tasks.Select(x => x.Response).ToArray());
            return tasks.ToDictionary(x => x.Order, x =>
            {
                if (!x.Response.Result.IsSuccessStatusCode)
                {
                    _logger.LogError($"Image request failed for order id '{x.Order}' with code '{x.Response.Result.StatusCode}");
                    return new byte[0];
                }
                return x.Response.Result.Content.ReadAsByteArrayAsync().Result;
            });
        }
    }
}
