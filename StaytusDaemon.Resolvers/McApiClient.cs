using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StaytusDaemon.Resolvers
{
    public class McApiClient
    {
        private readonly HttpClient _http;
        private readonly ILogger _logger;
        
        private const string MC_API = "https://mcapi.us/server/status?ip={0}&port={1}";

        public McApiClient(HttpClient http, ILogger logger)
        {
            _http = http;
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
            var requestUri = string.Format(MC_API, ipAddress, port);
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