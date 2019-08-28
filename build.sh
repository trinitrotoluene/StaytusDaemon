#!/bin/sh

echo "Compiling StaytusDaemon..."

dotnet build -c Release

dotnet publish -c Release

echo "Copying application to $PWDoutput/"

mkdir $PWD/output/

cp -rf ./StaytusDaemon/bin/Release/netcoreapp3.0/publish/* $PWD/output/
