using Microsoft.Extensions.Configuration;

namespace StaytusDaemon
{
    public class DefaultResolverStrategy
    {
        private readonly IConfigurationSection _defaults;

        public DefaultResolverStrategy(IConfigurationSection defaults)
        {
            _defaults = defaults;
        }

        public int RetryAmount => _defaults.GetValue("Retry", 1);

        public int LatencyThreshold => _defaults.GetValue("LatencyThreshold", 500);

        public int Interval => _defaults.GetValue("Interval", 300) * 1000;
    }
}