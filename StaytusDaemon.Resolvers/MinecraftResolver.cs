using System.Net.Http;
using System.Threading.Tasks;
using StaytusDaemon.Plugins;

namespace StaytusDaemon.Resolvers
{
    // ReSharper disable once UnusedMember.Global
    [Resolver("Minecraft")]
    public class MinecraftResolver : IStatusResolver
    {
        private readonly HttpClient _http;

        public MinecraftResolver()
        {
            _http = new HttpClient();
        }

        public async ValueTask<IResolveResult> ResolveStatusAsync(IResolveContext context)
        {
            var mcClient = new McApiClient(_http, context.Logger);

            var isOnline = await mcClient.CheckServerStatusAsync(context.ServiceConfig["host"],
                context.ServiceConfig["port"] ?? "25565");

            return new ResolveResult
            {
                IsOnline = isOnline
            };
        }
    }
}