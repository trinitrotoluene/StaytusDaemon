Write-Host "Compiling StaytusDaemon..."

dotnet publish -c Release

Write-Host "Creating plugin directory"

mkdir -Force .\StaytusDaemon\bin\Release\netcoreapp3.0\publish\Plugins

Write-Host "Copying default resolvers"

Copy-Item -Force .\StaytusDaemon.Resolvers\bin\Release\netcoreapp3.0\publish\StaytusDaemon.Resolvers.dll .\StaytusDaemon\bin\Release\netcoreapp3.0\publish\Plugins\StaytusDaemon.Resolvers.dll

mkdir -Force output

Write-Host "Copying application to $PWD\output\"

Copy-Item -Recurse -Force .\StaytusDaemon\bin\Release\netcoreapp3.0\publish\* $PWD\output\