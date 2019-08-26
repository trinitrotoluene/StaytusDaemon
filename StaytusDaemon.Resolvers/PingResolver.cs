using System.Net.NetworkInformation;
using System.Threading.Tasks;
using StaytusDaemon.Plugins;

namespace StaytusDaemon.Resolvers
{
    // ReSharper disable once UnusedMember.Global
    [Resolver("Ping")]
    public class PingResolver : IStatusResolver
    {
        public async ValueTask<IResolveResult> ResolveStatusAsync(IResolveContext context)
        {
            var ping = new Ping();

            var pingResult = await ping.SendPingAsync(context.Host);

            return new ResolveResult()
            {
                IsOnline = pingResult.Status == IPStatus.Success,
                Latency = (int) pingResult.RoundtripTime
            };
        }
    }
}