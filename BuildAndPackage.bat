@echo off
echo ========================================
echo ItemGiver - Build and Package Script
echo ========================================
echo.

REM Build the DLL
echo [1/3] Building ItemGiver.dll...
cd /d "%~dp0"
dotnet build --configuration Release
if errorlevel 1 (
    echo ERROR: Build failed!
    pause
    exit /b 1
)
echo Build successful!
echo.

REM Copy DLL to package folder
echo [2/3] Copying DLL to ThunderstorePackage...
copy /Y "bin\Release\net6.0\ItemGiver.dll" "ThunderstorePackage\ItemGiver.dll"
echo DLL copied!
echo.

REM Install to game folder
echo [3/3] Installing to game BepInEx plugins folder...
copy /Y "bin\Release\net6.0\ItemGiver.dll" "D:\SteamLibrary\steamapps\common\Megabonk\BepInEx\plugins\ItemGiver.dll"
echo Installed!
echo.

echo ========================================
echo DONE!
echo ========================================
echo.
echo Next steps for Thunderstore:
echo 1. Create icon.png (256x256) in ThunderstorePackage folder
echo 2. Zip the contents of ThunderstorePackage folder
echo 3. Upload to Thunderstore
echo.
echo See PACKAGING_INSTRUCTIONS.txt for details
echo.
pause
