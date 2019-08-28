namespace StaytusDaemon.Plugins
{
    public interface IResolveResult
    {
        bool IsOnline { get; }

        int Latency { get; }
    }
}