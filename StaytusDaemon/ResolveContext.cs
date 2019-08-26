using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StaytusDaemon.Plugins;

namespace StaytusDaemon
{
    public class ResolveContext : IResolveContext
    {
        public string Host { get; set; }
        
        public ushort Port { get; set; }
        
        public IConfigurationSection ServiceConfig { get; set; }
        
        public ILogger Logger { get; set; }
    }
}