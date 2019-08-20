using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace mc_status_daemon
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly HttpClient _http;

        private readonly IConfiguration _config;

        public Worker(ILogger<Worker> logger, HttpClient http, IConfiguration config)
        {
            _logger = logger;
            _http = http;
            _config = config;
            
            _http.DefaultRequestHeaders.Add("X-Auth-Token", _config["staytus:token"]);
            _http.DefaultRequestHeaders.Add("X-Auth-Secret", _config["staytus:secret"]);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daemon started");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollServerHostAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to update host status: {ex}");
                }

                try
                {
                    await PollMcServerAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to update server status: {ex}");
                }

                await Task.Delay(_config.GetValue<int>("interval"), stoppingToken);
            }
        }

        private async Task UpdateServerStatusAsync(string serviceName, string serviceStatus, bool isOnline)
        {
            _logger.LogInformation($"Service status for {serviceName} updated, service is {serviceStatus}");
            if (isOnline)
            {
                _logger.LogDebug("Server is reported online");
                if (serviceStatus != ApiConstants.Permalinks.Operational)
                {
                    _logger.LogInformation("Server detected as online, updating service");
                    await _http.SendAsync(BuildStatusUpdateMessage(serviceName,
                        ApiConstants.Permalinks.Operational));
                }
            }
            // If we were operational, move to partial outage
            else if (serviceStatus == ApiConstants.Permalinks.Operational)
            {
                _logger.LogInformation("Server offline. Moving to partial outage");
                await _http.SendAsync(BuildStatusUpdateMessage(serviceName,
                    ApiConstants.Permalinks.PartialOutage));
            }
            // If we were partial outage, move to major outage
            else if (serviceStatus != ApiConstants.Permalinks.MajorOutage)
            {
                _logger.LogInformation("Server still offline. Moving to major outage");
                await _http.SendAsync(BuildStatusUpdateMessage(serviceName,
                    ApiConstants.Permalinks.MajorOutage));
            }
        }
        
        private async Task PollServerHostAsync()
        {
            var serviceStatus = await CheckServiceStatusAsync(_config["host:service_name"]);
            var ping = new Ping();

            var pingResult = await ping.SendPingAsync(_config["mcapi:server_ip"], 10_000);

            await UpdateServerStatusAsync(_config["host:service_name"], serviceStatus,
                pingResult.Status == IPStatus.Success);
        }
        
        private async Task<bool> CheckServerStatusAsync()
        {
            var response = await _http.SendAsync(BuildMcApiRequestMessage(_config["mcapi:server_ip"],
                _config["mcapi:server_port"]));

            bool isOnline;
            using (var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync()))
            {
                isOnline = jsonDoc.RootElement.GetProperty("online")
                    .GetBoolean();
            }
            
            return isOnline;
        }

        private async Task<string> CheckServiceStatusAsync(string serviceName)
        {
            var response = await _http.SendAsync(BuildStatusRequestMessage(serviceName));

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
        
        private async Task PollMcServerAsync()
        {
            _logger.LogInformation("Polling minecraft service");

            string currentStatus = await CheckServiceStatusAsync(_config["staytus:service_name"]);

            if (currentStatus == ApiConstants.Permalinks.Maintenance) return;

            _logger.LogInformation($"Current service status reported as {currentStatus}. Validating with mcapi");

            bool isOnline = await CheckServerStatusAsync();

            await UpdateServerStatusAsync(_config["staytus:service_name"], currentStatus, isOnline);
        }

        private HttpRequestMessage BuildMcApiRequestMessage(string ipAddress, string port)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(string.Format(ApiConstants.McApi, ipAddress, port))
            };
            return request;
        }
        
        private HttpRequestMessage BuildStatusUpdateMessage(string serviceName, string permalink)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Content = new StringContent($@"{{ ""service"": ""{serviceName}"", ""status"": ""{permalink}""}}",
                    Encoding.UTF8, "application/json"),
                RequestUri = new Uri(_config["staytus:base_url"] + ApiConstants.SetStatusEndpoint)
            };

            return request;
        }

        private HttpRequestMessage BuildStatusRequestMessage(string serviceName)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                Content = new StringContent($@"{{""service"": ""{serviceName}""}}", Encoding.UTF8,
                    "application/json"),
                RequestUri = new Uri(_config["staytus:base_url"] + ApiConstants.GetStatusEndpoint)
            };

            return request;
        }
    }
}