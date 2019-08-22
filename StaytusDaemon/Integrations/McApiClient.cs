using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StaytusDaemon.Integrations
{
    public class McApiClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly ILogger<McApiClient> _logger;

        public McApiClient(HttpClient http, IConfiguration config, ILogger<McApiClient> logger)
        {
            _http = http;
            _config = config;
            _logger = logger;
        }
        
        public async Task<bool> CheckServerStatusAsync(string ip, string port)
        {
            var response = await _http.SendAsync(BuildMcApiRequestMessage(ip, port));

            bool isOnline;
            using (var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync()))
            {
                isOnline = jsonDoc.RootElement.GetProperty("online")
                    .GetBoolean();
            }
            
            return isOnline;
        }
        
        private HttpRequestMessage BuildMcApiRequestMessage(string ipAddress, string port)
        {
            var requestUri = string.Format(ApiConstants.McApi, ipAddress, port);
            _logger.LogDebug("GET -> " + requestUri);
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(requestUri)
            };
            return request;
        }
    }
}