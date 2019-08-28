using Microsoft.Extensions.Configuration;

namespace StaytusDaemon
{
    public class CustomResolverStrategy
    {
        private readonly DefaultResolverStrategy _defaultStrategy;

        private readonly IConfigurationSection _service;

        public CustomResolverStrategy(DefaultResolverStrategy defaultStrategy, IConfigurationSection service)
        {
            _defaultStrategy = defaultStrategy;
            _service = service;
        }

        public int RetryAmount
        {
            get
            {
                var raw = _service["Retry"];
                if (string.IsNullOrEmpty(raw)) return _defaultStrategy.RetryAmount;
                return int.Parse(raw);
            }
        }

        public int LatencyThreshold
        {
            get
            {
                var raw = _service["LatencyThreshold"];
                if (string.IsNullOrEmpty(raw)) return _defaultStrategy.LatencyThreshold;
                return int.Parse(raw);
            }
        }

        public int Interval
        {
            get
            {
                var raw = _service["Interval"];
                if (string.IsNullOrEmpty(raw)) return _defaultStrategy.Interval;
                return int.Parse(raw) * 1000;
            }
        }
    }
}