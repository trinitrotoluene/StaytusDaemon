using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace mc_status_daemon
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddEnvironmentVariables(prefix: "MC_DAEMON_")
                        .AddCommandLine(args)
                        .AddJsonFile("config.json", optional: true)
                        .AddUserSecrets<Program>(optional: true);
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
                        .AddSingleton<HttpClient>();
                });
    }
}