#!/bin/sh

echo "Compiling StaytusDaemon..."

dotnet publish -c Release

echo "Copying application to $PWDoutput/"

cp -rf ./StaytusDaemon/bin/Release/netcoreapp3.0/publish/* $PWD/output/
