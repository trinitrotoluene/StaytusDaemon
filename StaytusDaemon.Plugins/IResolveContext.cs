using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StaytusDaemon.Plugins
{
    public interface IResolveContext
    {
        string Host { get; set; }
        
        ushort Port { get; set; }
        
        IConfigurationSection ServiceConfig { get; set; }
        
        ILogger Logger { get; set; }
    }
}