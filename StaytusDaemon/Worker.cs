using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StaytusDaemon.Integrations;

namespace StaytusDaemon
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly IConfiguration _config;

        private readonly StaytusClient _staytus;
        private readonly McApiClient _mcApi;

        public Worker(ILogger<Worker> logger, IConfiguration config, StaytusClient staytus, McApiClient mcApi)
        {
            _logger = logger;
            _config = config;
            _staytus = staytus;
            _mcApi = mcApi;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StaytusDaemon running");

            var services = _config.GetSection("services");

            foreach (var service in services.GetChildren())
            {
                _logger.LogInformation("Staytus service: {0} found, enabling", service["name"]);

                _ = DispatchServiceUpdaterAsync(service, stoppingToken);
            }

            await Task.Delay(-1, stoppingToken);
        }

        private async Task DispatchServiceUpdaterAsync(IConfigurationSection serviceConfig, CancellationToken ct)
        {
            var interval = serviceConfig.GetValue<int>("interval") * 1000;
            var type = serviceConfig.GetValue<ServiceType>("type");

            _logger.LogInformation("Enabled service of type {0} with interval {1}ms", type, interval);

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var currentStatus = await _staytus.GetStatusAsync(serviceConfig["name"]);
                    if (currentStatus != ApiConstants.Permalinks.Maintenance)
                    {
                        switch (type)
                        {
                            case ServiceType.Ping:
                                await PingServerAsync(currentStatus, serviceConfig);
                                break;
                            case ServiceType.Minecraft:
                                await PingMcServerAsync(currentStatus, serviceConfig);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("An exception was thrown while attempting to update service '{0}' {1}", 
                        serviceConfig["name"], ex);
                }
                
                await Task.Delay(interval, ct);
            }
        }

        private async Task PingServerAsync(string currentStatus, IConfigurationSection serviceConfig)
        {
            var ping = new Ping();

            var pingResult = await ping.SendPingAsync(serviceConfig["host"], 10_000);

            await _staytus.UpdateStatusAsync(serviceConfig["name"], currentStatus,
                pingResult.Status == IPStatus.Success);
        }

        private async Task PingMcServerAsync(string currentStatus, IConfigurationSection serviceConfig)
        {
            bool isOnline = await _mcApi.CheckServerStatusAsync(serviceConfig["host"], serviceConfig["port"]);

            await _staytus.UpdateStatusAsync(serviceConfig["name"], currentStatus, isOnline);
        }
    }
}