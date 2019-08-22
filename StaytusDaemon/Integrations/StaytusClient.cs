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
        private readonly ILogger<StaytusClient> _logger;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        
        public StaytusClient(IConfiguration config, ILogger<StaytusClient> logger, HttpClient http)
        {
            _config = config;
            _logger = logger;
            _http = http;
        }

        public async Task UpdateStatusAsync(string serviceName, string currentStatus, bool isReportedOnline)
        {
            string nextStatus = null;
            if (isReportedOnline)
            {
                if (currentStatus != ApiConstants.Permalinks.Operational)
                {
                    nextStatus = ApiConstants.Permalinks.Operational;
                    
                    _logger.LogInformation("Service {0} is back online", serviceName);
                }
            }
            else if (currentStatus == ApiConstants.Permalinks.Operational)
            {
                nextStatus = ApiConstants.Permalinks.PartialOutage;
                
                _logger.LogInformation("Service {0} is offline. Updating to partial-outage", serviceName);
            }
            else if (currentStatus != ApiConstants.Permalinks.MajorOutage)
            {
                nextStatus = ApiConstants.Permalinks.MajorOutage;
                
                _logger.LogInformation("Service {0} is still offline. Updating to major-outage");
            }

            if (nextStatus != null)
            {
                _logger.LogDebug("SERVICE -> {0} [{1} -> {2}]", serviceName, currentStatus, nextStatus);
                
                await _http.SendAsync(BuildStatusUpdateMessage(serviceName, nextStatus));
            }
        }
        
        public async Task<string> GetStatusAsync(string serviceName)
        {
            var response = await _http.SendAsync(BuildStatusRequestMessage(_config["staytus:base_url"], serviceName));

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
            var requestUri = _config["staytus:base_url"] + ApiConstants.SetStatusEndpoint;
            _logger.LogDebug("GET -> " + requestUri);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Content = new StringContent($@"{{ ""service"": ""{serviceName}"", ""status"": ""{permalink}""}}",
                    Encoding.UTF8, "application/json"),
                RequestUri = new Uri(requestUri)
            };
            
            request.Headers.Add("X-Auth-Token", _config["staytus:token"]);
            request.Headers.Add("X-Auth-Secret", _config["staytus:secret"]);

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

            request.Headers.Add("X-Auth-Token", _config["staytus:token"]);
            request.Headers.Add("X-Auth-Secret", _config["staytus:secret"]);
            
            return request;
        }
    }
}