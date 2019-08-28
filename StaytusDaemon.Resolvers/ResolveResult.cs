using StaytusDaemon.Plugins;

namespace StaytusDaemon.Resolvers
{
    public class ResolveResult : IResolveResult
    {
        public bool IsOnline { get; internal set; }

        public int Latency { get; internal set; }
    }
}