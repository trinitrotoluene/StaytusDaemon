using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StaytusDaemon.Integrations;
using StaytusDaemon.Plugins;

namespace StaytusDaemon
{
    public class StaytusService
    {
        private readonly ILogger _logger;
        private readonly IStatusResolver _resolver;
        private readonly IConfigurationSection _service;
        private readonly StaytusClient _staytus;
        private readonly CustomResolverStrategy _strategy;

        public StaytusService(IConfigurationSection service, IStatusResolver resolver,
            CustomResolverStrategy strategy, StaytusClient staytus, ILogger logger)
        {
            _service = service;
            _resolver = resolver;
            _strategy = strategy;
            _staytus = staytus;
            _logger = logger;
        }

        public async Task UpdateAsync(ResolveContext ctx, CancellationToken ct)
        {
            try
            {
                var currentStatus = await _staytus.GetStatusAsync(_service.Key);

                // Check for maintenance period
                if (currentStatus == ApiConstants.Permalinks.Maintenance)
                {
                    _logger.LogWarning("Marked as under maintenance. Skipping for this interval.",
                        _service.Key);

                    // No-Op
                    return;
                }

                // Try to resolve N times until success.
                IResolveResult resolveResult = default;
                int i;
                for (i = 0; i < _strategy.RetryAmount; i++)
                {
                    resolveResult = await _resolver.ResolveStatusAsync(ctx);

                    if (resolveResult.IsOnline) break;

                    // Give the service some time to get it together.
                    await Task.Delay(100, ct);
                }

                _logger.LogDebug("Attempted to resolve {0} time(s), service {1}", i + 1,
                    resolveResult?.IsOnline ?? false ? "Online" : "Offline");

                var newStatus = CalculateNextStatus(resolveResult, currentStatus);

                if (newStatus != null)
                {
                    _logger.LogDebug("Updating status {1} -> {2}", _service.Key, currentStatus, newStatus);

                    await _staytus.UpdateStatusAsync(_service.Key, newStatus);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[0] An exception was thrown while attempting to update:\r\n {1}",
                    _service.Key, ex);
            }
        }

        private string CalculateNextStatus(IResolveResult resolveResult, string currentStatus)
        {
            string nextStatus = null;

            if (resolveResult.IsOnline)
            {
                if (resolveResult.Latency > _strategy.LatencyThreshold &&
                    currentStatus != ApiConstants.Permalinks.DegradedPerformance)
                {
                    nextStatus = ApiConstants.Permalinks.DegradedPerformance;

                    _logger.LogInformation("Online with degraded performance.");
                }
                else if (currentStatus != ApiConstants.Permalinks.Operational)
                {
                    nextStatus = ApiConstants.Permalinks.Operational;

                    _logger.LogInformation("Online.");
                }
            }
            else if (currentStatus == ApiConstants.Permalinks.Operational)
            {
                nextStatus = ApiConstants.Permalinks.PartialOutage;

                _logger.LogInformation("Offline. Declaring partial outage!");
            }
            else if (currentStatus == ApiConstants.Permalinks.PartialOutage)
            {
                nextStatus = ApiConstants.Permalinks.MajorOutage;

                _logger.LogInformation("Still offline. Major outage!");
            }

            return nextStatus;
        }
    }
}