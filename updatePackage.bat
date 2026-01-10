@echo off
echo [INFO] Copying dependencies from SL_REFERENCES...
if not defined SL_REFERENCES (
    echo [ERROR] SL_REFERENCES environment variable is not set!
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
set "TOOL_ID=SCPSLTemplateGenerator"
echo [INFO] Uninstalling old version (if present)...
dotnet tool list --global | findstr /I " %TOOL_ID% " >nul
if %errorlevel%==0 (
    dotnet tool uninstall --global %TOOL_ID%
) else (
    echo [INFO] Tool not currently installed; skipping uninstall.
)
echo [INFO] Installing new version...
dotnet tool install --global --add-source .\nupkg\ %TOOL_ID%
echo [SUCCESS] Package updated successfully!