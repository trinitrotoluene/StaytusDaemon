using Microsoft.Extensions.Configuration;

namespace StaytusDaemon
{
    public class DefaultResolverStrategy
    {
        private readonly IConfigurationSection _defaults;

        public int RetryAmount => _defaults.GetValue<int>("Retry", 1);

        public int LatencyThreshold => _defaults.GetValue<int>("LatencyThreshold", 500);

        public int FailureThreshold => _defaults.GetValue<int>("FailureThreshold", 1);

        public int SuccessThreshold => _defaults.GetValue<int>("SuccessThreshold", 1);

        public int Interval => _defaults.GetValue<int>("Interval", 300) * 1000;

        public DefaultResolverStrategy(IConfigurationSection defaults)
        {
            _defaults = defaults;
        }
    }
}