FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

COPY . ./

RUN dotnet restore

RUN ./build.sh

FROM mcr.microsoft.com/dotnet/core/runtime:3.0

COPY --from=build-env ./output ./

ENTRYPOINT ["dotnet", "StaytusDaemon.dll"]
