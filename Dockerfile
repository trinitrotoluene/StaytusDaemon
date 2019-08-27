FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

WORKDIR /StaytusDaemon

COPY . ./

RUN chmod +x ./build.sh

RUN ./build.sh

FROM mcr.microsoft.com/dotnet/core/runtime:3.0

WORKDIR /StaytusDaemon

COPY --from=build-env ./StaytusDaemon/output ./

ENTRYPOINT ["dotnet", "StaytusDaemon.dll"]
