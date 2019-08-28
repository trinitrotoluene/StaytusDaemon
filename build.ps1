Write-Host "Compiling StaytusDaemon..."

dotnet build -c Release

dotnet publish -c Release

Write-Host "Copying application to $PWD\output\"

mkdir $PWD\output\

Copy-Item -Recurse -Force .\StaytusDaemon\bin\Release\netcoreapp3.0\publish\* $PWD\output\
