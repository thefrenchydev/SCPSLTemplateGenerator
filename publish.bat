@echo off
set /p VERSION="Enter version (e.g., 1.0.1): "
set /p API_KEY="Enter NuGet API Key: "

dotnet pack -c Release /p:Version=%VERSION%
dotnet nuget push .\nupkg\SCPSLTemplateGenerator.%VERSION%.nupkg --api-key %API_KEY% --source https://api.nuget.org/v3/index.json

echo Published version %VERSION%!
pause