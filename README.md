# StaytusDaemon
[Staytus](https://github.com/adamcooke/staytus) is a free, open-source solution for publishing service status. 

StaytusDaemon integrates with the HTTP API exposed by Staytus to update your services, live.

## Getting started

### Docker

It is recommended that if your environment supports it, that you build the StaytusDaemon image from the included Dockerfile.

```bash
docker build -t trinitrotoluene/staytusdaemon .
```

Once the build completes, you'll be able to start a StaytusDaemon container with `docker run` whenever you want!

In order to configure the daemon, you'll need to mount your own `Settings.ini` and `Services` folder. See below (and the example files in the repository) for more information.

```bash
touch Settings.ini
mkdir Services
touch Services/MyService.ini

# Configure as shown in later sections and adjust paths as appropriate.

docker run -d \
    -v /home/$USER/Settings.ini:/StaytusDaemon/Settings.ini:ro \
    -v /home/$USER/Services:/StaytusDaemon/Services:ro \
    trinitrotoluene/staytusdaemon
```

### Compile & run

If all you want to do is compile, configure and run the daemon, clone the repository and run the build script.

The script will copy the compiled application to `output/`, at which point you can jump to the "Configure" section of this page.

```bash
git clone https://github.com/trinitrotoluene/StaytusDaemon.git
cd StaytusDaemon

chmod +x build.sh
./build.sh
```

## Configure

In order to communicate with Staytus, you'll need an API token and secret, which you can generate in the Settings -> API Tokens section of your Staytus page.

First, fill out the `Settings.ini` file with your authentication details and the URL of your Staytus page. 

```bash
cd output
vim Settings.ini
```

```ini
[Staytus]
Token=aaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa
Secret=ABCDEFGHIJKLMNOPQRStuvwxYZ
BaseUrl=https://status.example.org

[Defaults]
; How many times a service resolution should fail before being considered offline
Retry=1
; At what latency (if reported) should a service be marked as "Degraded Service"
LatencyThreshold=500
```

Once you have correctly configured the daemon's core features, it's time to set up a service. Service units are defined in `Services\Services.ini` by default, however they can be split/defined over several files if you prefer.

```bash
cd Services
vim Services.ini
```

```ini
; The section header should be the "short-name" set for your service in Staytus.
[host]
; The name of the resolver that should be used for this service. See below for a list of those available by default.
Type=Ping
; The host or IP of your service. Varies by resolver.
Host=example.org
; The interval that the daemon should wait before re-attempting to resolve the status of a service.
Interval=600

; Defaults set in Settings.ini can be overriden in here.
Retry=3
LatencyThreshold=300

; Example of a resolver using the optional "Port" tag and a different service type.
[mc]
Type=Minecraft
Host=mc.example.org
Post=25565
Interval=300

LatencyThreshold=200
```

## Resolvers

StaytusDaemon is designed with flexibility in mind, so it can easily be adapted to the needs of the many types of services it may be reporting the status of.

In order to accomplish this, a plug-in system is used. Simply reference the `StaytusDaemon.Plugins/StaytusDaemon.Plugins.csproj` project in a class library and implement a resolver by extending the `IStatusResolver` interface. Resolvers must also be decorated with the `ResolverAttribute` to define their names used when selecting `Type` in a service definition.

To register your resolver, simply copy the output assembly (and any dependencies added EXCEPT those included by the Plugins package) to the Plugins folder. When the daemon runs it will load and crawl your assembly for resolver types to register.

When the names of two resolvers clash, the most recently added one will remain registered.

Note also that resolvers are not loaded transiently, and thus any state in your resolver should be carefully managed.

See below an example of a "Ping" resolver.
```c#
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
```

### Default resolvers
- `"Ping"`
- `"Minecraft"`
