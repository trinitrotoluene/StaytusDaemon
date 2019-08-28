using Microsoft.Extensions.Configuration;

namespace StaytusDaemon
{
    public class CustomResolverStrategy
    {
        private readonly DefaultResolverStrategy _defaultStrategy;

        private readonly IConfigurationSection _service;

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

        public int FailureThreshold
        {
            get
            {
                var raw = _service["FailureThreshold"];
                if (string.IsNullOrEmpty(raw)) return _defaultStrategy.FailureThreshold;
                return int.Parse(raw);
            }
        }

        public int SuccessThreshold
        {
            get
            {
                var raw = _service["SuccessThreshold"];
                if (string.IsNullOrEmpty(raw)) return _defaultStrategy.SuccessThreshold;
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

        public CustomResolverStrategy(DefaultResolverStrategy defaultStrategy, IConfigurationSection service)
        {
            _defaultStrategy = defaultStrategy;
            _service = service;
        }
    }
}