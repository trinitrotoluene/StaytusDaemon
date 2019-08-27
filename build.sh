#!/bin/sh

echo "Compiling StaytusDaemon..."

dotnet publish -c Release

echo "Creating plugin directory"

mkdir -p ./StaytusDaemon/bin/Release/netcoreapp3.0/publish/Plugins

echo "Copying default resolvers"

cp -f ./StaytusDaemon.Resolvers/bin/Release/netcoreapp3.0/publish/StaytusDaemon.Resolvers.dll ./StaytusDaemon/bin/Release/netcoreapp3.0/publish/Plugins/StaytusDaemon.Resolvers.dll

mkdir -p output

echo "Copying application to $PWDoutput/"

cp -rf ./StaytusDaemon/bin/Release/netcoreapp3.0/publish/* $PWD/output/
