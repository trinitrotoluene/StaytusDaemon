# StaytusDaemon
[Staytus](https://github.com/adamcooke/staytus) is a free, open-source solution for publishing service status. 

It exposes an API to allow services to automatically be updated during periods of outage, and this program does exactly that. Define your services, their location, and which source you want to use to verify their availability (WIP), and watch your site be updated live!

## Coming soon
- Auto-create issues at either partial or major outage
- Auto-resolve issues
- More availability sources/plug-in model

# Get started

- Clone the repository

```bash
git pull https://github.com/trinitrotoluene/StaytusDaemon.git
cd StaytusDaemon
```
- Edit the configuration file `StaytusDaemon/config.json` to fit your requirements. It will be automatically copied to the build's output directory.

- Add your service unit definitions in the `"services"` array of the configuration file.
```
{
  "name": "my-service", // The shortname of your Staytus service
  "interval": 300, // Polling interval, in seconds
  "type": "Ping", // The type of the service. See below for more info
  "host": "127.0.0.1" // The host of the service to use for addressing
}
```

## Sources

- ICMP (Ping)
  - Requires
    - Host
- MCAPI (Minecraft)
  - Requires
    - Host
    - Port `"port": "25565"`

Select the type of a service in your definition, they will be automatically run when the daemon is started.
```
"type": "Ping"
```
- Compile and run the app
```bash
dotnet publish -c Release
dotnet StaytusDaemon/bin/Release/netcoreapp3.0/publish/StaytusDaemon.dll
```

**If you appear to encounter errors, set the `debug` flag in `config.json` to `true` and check the console output.**
