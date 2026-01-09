@echo off
echo [INFO] Copying dependencies from LABAPI_REFERENCES...
if not defined LABAPI_REFERENCES (
    echo [ERROR] LABAPI_REFERENCES environment variable is not set!
    exit /b 1
)

if not exist "dependencies" mkdir dependencies

echo [INFO] Syncing templates from src folder...
powershell -ExecutionPolicy Bypass -File syncTemplates.ps1
if errorlevel 1 (
    echo [ERROR] Template sync failed!
    exit /b 1
)

echo [INFO] Building project...
dotnet build -c Release
echo [INFO] Packing project...
dotnet pack -c Release
echo [INFO] Uninstalling old version...
dotnet tool uninstall --global SCPSLTemplateGenerator
echo [INFO] Installing new version...
dotnet tool install --global --add-source .\nupkg\ SCPSLTemplateGenerator
echo [SUCCESS] Package updated successfully!