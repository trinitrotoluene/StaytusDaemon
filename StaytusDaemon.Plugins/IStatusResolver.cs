using System.Threading.Tasks;

namespace StaytusDaemon.Plugins
{
    public interface IStatusResolver
    {
        ValueTask<IResolveResult> ResolveStatusAsync(IResolveContext context);
    }
}