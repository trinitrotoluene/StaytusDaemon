using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StaytusDaemon.Integrations;
using StaytusDaemon.Plugins;
using StaytusDaemon.Reflection;

namespace StaytusDaemon
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly ILoggerFactory _loggerFactory;

        private readonly StaytusClient _staytus;

        private readonly PluginManager _pluginManager;

        public Worker(ILogger<Worker> logger, StaytusClient staytus, ILoggerFactory loggerFactory, PluginManager pluginManager)
        {
            _logger = logger;
            _staytus = staytus;
            _loggerFactory = loggerFactory;
            _pluginManager = pluginManager;
        }

        private void LoadPlugins()
        {
            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);
            
            var projectLocation = Path.Combine(assemblyLocation, "Plugins");

            foreach (var file in Directory.EnumerateFiles(projectLocation, "*.dll"))
            {
                var pluginAssembly = Assembly.LoadFile(file);
                
                _pluginManager.AddResolversFrom(pluginAssembly);
                
                _logger.LogDebug("Loaded resolvers from {0}", file);
            }
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daemon started, loading plugins");

            LoadPlugins();
            
            _logger.LogInformation("Discovering service units");

            var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);
            var servicesLocation = Path.Combine(assemblyLocation, "Services");
            
            var configurationBuilder = new ConfigurationBuilder();
            foreach (var file in Directory.GetFiles(servicesLocation, "*.ini"))
            {
                _logger.LogInformation("Service file {0} found. Registering.", file);
                configurationBuilder.AddIniFile(file);
            }

            var svcConfig = configurationBuilder.Build();

            var services = svcConfig.GetChildren();

            foreach (var service in services)
            {
                var serviceType = service["Type"];
                
                _logger.LogInformation("Service '{0}' has type {1}, searching for resolver.", service.Key, serviceType);

                var resolver = _pluginManager.GetResolver(serviceType);
                
                _logger.LogInformation("Resolver found, service {0} spawned.", service.Key);
                
                _ = DispatchServiceUpdaterAsync(service, resolver, stoppingToken);
            }

            await Task.Delay(-1, stoppingToken);
        }

        private async Task DispatchServiceUpdaterAsync(IConfigurationSection service, IStatusResolver resolver, 
            CancellationToken ct)
        {
            var interval = service.GetValue("Interval", 300) * 1000;
            var context = new ResolveContext()
            {
                Host = service["Host"],
                Logger = _loggerFactory.CreateLogger(service.Key),
                Port = service.GetValue<ushort>("Port", 80),
                ServiceConfig = service
            };
            
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var currentStatus = await _staytus.GetStatusAsync(service.Key);
                    if (currentStatus != ApiConstants.Permalinks.Maintenance)
                    {
                        var resolveResult = await resolver.ResolveStatusAsync(context);

                        await _staytus.UpdateStatusAsync(service.Key, currentStatus, resolveResult);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("An exception was thrown while attempting to update '{0}':\r\n {1}", 
                        service["name"], ex);
                }
                
                await Task.Delay(interval, ct);
            }
        }
    }
}