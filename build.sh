#!/bin/sh

echo "Compiling StaytusDaemon..."

dotnet build -c Release ./StaytusDaemon.Resolvers/StaytusDaemon.Resolvers.csproj

dotnet publish -c Release

echo "Copying application to $PWD/output/"

mkdir $PWD/output/

cp -rf ./StaytusDaemon/bin/Release/netcoreapp3.0/publish/* $PWD/output/
