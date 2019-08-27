FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build-env

WORKDIR /StaytusDaemon

COPY . ./

RUN dotnet restore

RUN chmod +x ./build.sh

RUN ./build.sh

FROM mcr.microsoft.com/dotnet/core/runtime:3.0

COPY --from=build-env ./output ./

ENTRYPOINT ["dotnet", "StaytusDaemon.dll"]
