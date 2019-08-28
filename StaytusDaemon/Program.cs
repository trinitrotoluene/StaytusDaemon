using System.IO;
using System.Net.Http;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StaytusDaemon.Integrations;
using StaytusDaemon.Reflection;

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
                    var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);
                    var settingsLocation = Path.Combine(assemblyLocation, "Settings.ini");
                    config.AddEnvironmentVariables("MC_DAEMON_")
                        .AddCommandLine(args)
#if DEBUG
                        .AddUserSecrets<Program>(optional: true);
#else
                        .AddIniFile(settingsLocation);
#endif
                })
                .ConfigureLogging((hostContext, logBuilder) => { })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>()
                        .AddSingleton<HttpClient>()
                        .AddSingleton<StaytusClient>()
                        .AddSingleton<PluginManager>();
                });
    }
}