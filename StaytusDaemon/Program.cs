using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StaytusDaemon.Integrations;

namespace StaytusDaemon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables(prefix: "MC_DAEMON_")
                        .AddCommandLine(args);
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    var configPath = hostContext.Configuration["config"];
                    config
                        #if DEBUG
                        .AddUserSecrets<Program>(optional: true);
                        #else
                        .AddJsonFile(configPath);
                        #endif
                })
                .ConfigureLogging((hostContext, logBuilder) =>
                {
                    logBuilder.SetMinimumLevel(hostContext.Configuration.GetValue<bool>("debug")
                        ? LogLevel.Debug
                        : LogLevel.Information);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>()
                        .AddSingleton<HttpClient>()
                        .AddSingleton<StaytusClient>()
                        .AddSingleton<McApiClient>();
                });
    }
}