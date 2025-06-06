@echo off
setlocal enabledelayedexpansion

:: AI-IT Inc NVR - Fire TV Stick Installer
:: This batch script installs Kodi and the AI-IT Inc NVR addon on a Fire TV Stick

:: Set console colors
color 0B

:: Display banner
echo =========================================================
echo   AI-IT Inc NVR - Fire TV Stick Installer
echo =========================================================
echo.

:: Create temporary directory
set TEMP_DIR=%TEMP%\AIIT-NVR-Installer
if not exist "%TEMP_DIR%" mkdir "%TEMP_DIR%"

:: Step 1: Check for ADB installation
echo Step 1: Checking for ADB installation...
where adb >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo ADB already installed!
    for /f "tokens=*" %%i in ('where adb') do set ADB_PATH=%%i
) else (
    echo ADB not found. Installing Platform Tools...
    
    :: Download ADB Platform Tools
    echo Downloading Platform Tools...
    set PLATFORM_TOOLS_URL=https://dl.google.com/android/repository/platform-tools-latest-windows.zip
    set PLATFORM_TOOLS_ZIP=%TEMP_DIR%\platform-tools.zip
    set PLATFORM_TOOLS_DIR=%TEMP_DIR%\platform-tools
    
    powershell -Command "& {Invoke-WebRequest -Uri '%PLATFORM_TOOLS_URL%' -OutFile '%PLATFORM_TOOLS_ZIP%'}"
    
    :: Extract Platform Tools
    echo Extracting Platform Tools...
    powershell -Command "& {Expand-Archive -Path '%PLATFORM_TOOLS_ZIP%' -DestinationPath '%TEMP_DIR%' -Force}"
    
    :: Set ADB path
    set ADB_PATH=%PLATFORM_TOOLS_DIR%\adb.exe
    
    :: Add to PATH temporarily
    set PATH=%PATH%;%PLATFORM_TOOLS_DIR%
    
    echo ADB installed successfully!
)

:: Step 2: Connect to Fire TV Stick
echo.
echo Step 2: Connect to Fire TV Stick
echo Please ensure your Fire TV Stick has:
echo   1. Developer Options enabled
echo   2. ADB Debugging turned ON
echo   3. Apps from Unknown Sources enabled
echo.

set /p FIRETV_IP="Enter your Fire TV Stick IP address: "

echo Connecting to Fire TV Stick at %FIRETV_IP%...
"%ADB_PATH%" connect %FIRETV_IP%:5555

:: Verify connection
"%ADB_PATH%" devices | findstr "%FIRETV_IP%" >nul
if %ERRORLEVEL% EQU 0 (
    echo Successfully connected to Fire TV Stick!
) else (
    echo Failed to connect to Fire TV Stick. Please check:
    echo   - Fire TV Stick and PC are on the same network
    echo   - Developer options and ADB debugging are enabled
    echo   - IP address is correct
    echo   - You accepted the connection prompt on your Fire TV
    goto :EOF
)

:: Step 3: Download Kodi and NVR addon
echo.
echo Step 3: Downloading required files...

:: Download Kodi APK
echo Downloading Kodi...
set KODI_URL=https://mirrors.kodi.tv/releases/android/arm/kodi-20.2-Nexus-armeabi-v7a.apk
set KODI_APK=%TEMP_DIR%\kodi.apk
powershell -Command "& {Invoke-WebRequest -Uri '%KODI_URL%' -OutFile '%KODI_APK%'}"

:: Download NVR addon
echo Downloading NVR addon...
set ADDON_URL=https://aiit-inc.com/downloads/plugin.video.aiit-nvr-1.0.0.zip
set ADDON_ZIP=%TEMP_DIR%\plugin.video.aiit-nvr.zip
powershell -Command "& {Invoke-WebRequest -Uri '%ADDON_URL%' -OutFile '%ADDON_ZIP%'}"

:: Step 4: Install Kodi on Fire TV Stick
echo.
echo Step 4: Installing Kodi on Fire TV Stick...
"%ADB_PATH%" install -r "%KODI_APK%"

:: Check if installation was successful
if %ERRORLEVEL% EQU 0 (
    echo Kodi installed successfully!
) else (
    echo Failed to install Kodi. Error code: %ERRORLEVEL%
    goto :EOF
)

:: Step 5: Transfer NVR addon to Fire TV Stick
echo.
echo Step 5: Transferring NVR addon to Fire TV Stick...
"%ADB_PATH%" push "%ADDON_ZIP%" /sdcard/Download/

:: Check if transfer was successful
if %ERRORLEVEL% EQU 0 (
    echo NVR addon transferred successfully!
) else (
    echo Failed to transfer NVR addon. Error code: %ERRORLEVEL%
    goto :EOF
)

:: Step 6: Launch Kodi
echo.
echo Step 6: Launching Kodi...
"%ADB_PATH%" shell am start -n org.xbmc.kodi/.Splash

echo.
echo Installation process completed!
echo.
echo Next steps:
echo 1. In Kodi, go to Settings ^> Add-ons
echo 2. Select 'Install from zip file'
echo 3. If prompted, enable 'Unknown sources'
echo 4. Navigate to 'External storage' ^> 'Download'
echo 5. Select 'plugin.video.aiit-nvr.zip'
echo 6. Wait for 'Add-on installed' notification
echo 7. Configure the addon with your NVR details
echo.
echo Thank you for choosing AI-IT Inc NVR!

pause
