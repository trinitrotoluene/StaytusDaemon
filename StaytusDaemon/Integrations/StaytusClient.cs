using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StaytusDaemon.Integrations
{
    public class StaytusClient
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        private readonly ILogger<StaytusClient> _logger;

        public StaytusClient(IConfiguration config, ILogger<StaytusClient> logger, HttpClient http)
        {
            _config = config;
            _logger = logger;
            _http = http;
        }

        public Task UpdateStatusAsync(string serviceName, string newStatus) =>
            _http.SendAsync(BuildStatusUpdateMessage(serviceName, newStatus));

        public async Task<string> GetStatusAsync(string serviceName)
        {
            var response = await _http.SendAsync(BuildStatusRequestMessage(_config["Staytus:BaseUrl"], serviceName));

            string currentStatus;
            using (var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync()))
            {
                currentStatus = jsonDoc.RootElement.GetProperty("data")
                    .GetProperty("status")
                    .GetProperty("permalink")
                    .GetString();
            }

            return currentStatus;
        }

        private HttpRequestMessage BuildStatusUpdateMessage(string serviceName, string permalink)
        {
            var requestUri = _config["Staytus:BaseUrl"] + ApiConstants.SetStatusEndpoint;
            _logger.LogDebug("GET -> " + requestUri);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Content = new StringContent($@"{{ ""service"": ""{serviceName}"", ""status"": ""{permalink}""}}",
                    Encoding.UTF8, "application/json"),
                RequestUri = new Uri(requestUri)
            };

            request.Headers.Add("X-Auth-Token", _config["Staytus:Token"]);
            request.Headers.Add("X-Auth-Secret", _config["Staytus:Secret"]);

            return request;
        }

        private HttpRequestMessage BuildStatusRequestMessage(string siteUrl, string serviceName)
        {
            var requestUri = siteUrl + ApiConstants.GetStatusEndpoint;
            _logger.LogDebug("GET ->  " + requestUri);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Content = new StringContent($@"{{""service"": ""{serviceName}""}}", Encoding.UTF8,
                    "application/json"),
                RequestUri = new Uri(requestUri)
            };

            request.Headers.Add("X-Auth-Token", _config["Staytus:Token"]);
            request.Headers.Add("X-Auth-Secret", _config["Staytus:Secret"]);

            return request;
        }
    }
}