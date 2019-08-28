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
        private readonly IConfigurationSection _service;
        private readonly IStatusResolver _resolver;
        private readonly CustomResolverStrategy _strategy;
        private readonly StaytusClient _staytus;
        private readonly ILogger _logger;

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

                if (currentStatus == ApiConstants.Permalinks.Maintenance)
                {
                    _logger.LogWarning("[{0}] marked as under maintenance. Skipping for this interval.",
                        _service.Key);

                    return;
                }

                IResolveResult resolveResult = default;
                int i;
                for (i = 0; i < _strategy.RetryAmount; i++)
                {
                    resolveResult = await _resolver.ResolveStatusAsync(ctx);

                    if (resolveResult.IsOnline) break;
                }

                _logger.LogDebug("[{0}] Resolved {1} time(s), service {2}", _service.Key, i + 1,
                    resolveResult?.IsOnline ?? false ? "Online" : "Offline");

                await _staytus.UpdateStatusAsync(_service.Key, currentStatus, resolveResult);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[0] An exception was thrown while attempting to update:\r\n {1}",
                    _service.Key, ex);
            }
        }
    }
}