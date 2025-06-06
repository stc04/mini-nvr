# 🔥 PC to Fire TV Stick Installation Guide

This guide provides step-by-step instructions for installing the AI-IT Inc NVR Kodi addon from your Windows PC to your Fire TV Stick.

## 📋 Prerequisites

- Windows PC
- Fire TV Stick (any model)
- Both devices on the same network
- USB cable (optional, for faster transfer)

## 🛠️ Step 1: Prepare Your Fire TV Stick

### Enable Developer Options

1. On your Fire TV, navigate to **Settings**
2. Select **My Fire TV** (or **Device** on older models)
3. Select **About**
4. Scroll to **Network**
5. Note down your Fire TV's **IP Address** (e.g., 192.168.1.100)
6. Go back to **About**
7. Highlight **Build** and click it **7 times** until you see "You are now a developer"
8. Go back to **My Fire TV**
9. Select **Developer Options**
10. Enable **ADB Debugging**
11. Enable **Apps from Unknown Sources**

![Fire TV Developer Settings](https://aiit-inc.com/images/firetv-dev-settings.jpg)

## 🖥️ Step 2: Set Up ADB on Your PC

### Install ADB Platform Tools

1. Download [ADB Platform Tools](https://developer.android.com/studio/releases/platform-tools)
2. Extract the ZIP file to a folder (e.g., `C:\adb`)
3. Open Command Prompt as Administrator
4. Navigate to the ADB folder:
   \`\`\`
   cd C:\adb
   \`\`\`

### Connect to Fire TV Stick

1. In Command Prompt, enter:
   \`\`\`
   adb connect [FIRE_TV_IP_ADDRESS]:5555
   \`\`\`
   Replace `[FIRE_TV_IP_ADDRESS]` with your Fire TV's IP address
   
2. On your Fire TV, you'll see a connection request - select **Allow**

3. Verify connection by typing:
   \`\`\`
   adb devices
   \`\`\`
   You should see your Fire TV listed as connected

![ADB Connection](https://aiit-inc.com/images/adb-connection.jpg)

## 📥 Step 3: Download Required Files

### Download Kodi

1. Download [Kodi for Android](https://kodi.tv/download/android) (ARM version)
   - Latest stable version recommended: Kodi 20.2 "Nexus"
   - Direct link: [kodi-20.2-Nexus-armeabi-v7a.apk](https://mirrors.kodi.tv/releases/android/arm/kodi-20.2-Nexus-armeabi-v7a.apk)

### Download AI-IT Inc NVR Addon

1. Download the [AI-IT Inc NVR Addon ZIP](https://aiit-inc.com/downloads/plugin.video.aiit-nvr-1.0.0.zip)

## 📲 Step 4: Install Kodi on Fire TV Stick

### Using ADB

1. In Command Prompt, install Kodi:
   \`\`\`
   adb install path\to\kodi-20.2-Nexus-armeabi-v7a.apk
   \`\`\`
   Replace `path\to\` with the actual path to your downloaded APK

2. Wait for "Success" message

![ADB Install Success](https://aiit-inc.com/images/adb-install-success.jpg)

## 📦 Step 5: Transfer NVR Addon to Fire TV

### Using ADB Push

1. Push the addon ZIP to Fire TV:
   \`\`\`
   adb push path\to\plugin.video.aiit-nvr-1.0.0.zip /sdcard/Download/
   \`\`\`
   Replace `path\to\` with the actual path to your downloaded ZIP

2. Wait for transfer to complete

## 🚀 Step 6: Install NVR Addon in Kodi

### Launch Kodi

1. Start Kodi on your Fire TV using:
   \`\`\`
   adb shell am start -n org.xbmc.kodi/.Splash
   \`\`\`
   Or launch it from your Fire TV Apps menu

### Install Addon from ZIP

1. In Kodi, navigate to **Settings** (gear icon)
2. Select **Add-ons**
3. Select **Install from zip file**
4. If prompted about unknown sources, select **Settings** and enable **Unknown sources**
5. Go back and select **Install from zip file** again
6. Navigate to **External storage** > **Download**
7. Select **plugin.video.aiit-nvr-1.0.0.zip**
8. Wait for "Add-on installed" notification

![Kodi Addon Installation](https://aiit-inc.com/images/kodi-addon-install.jpg)

## ⚙️ Step 7: Configure NVR Addon

### Access Addon Settings

1. Go to **Add-ons** > **Video add-ons**
2. Find and select **AI-IT Inc NVR**
3. Press the **Menu** button on your remote
4. Select **Settings**

### Configure Connection

1. Enter your NVR system's IP address
2. Enter port (default: 8080)
3. Enter username and password
4. Configure stream quality based on your Fire TV model:
   - Fire TV Stick (Basic): Low/Medium (720p)
   - Fire TV Stick 4K: Medium/High (1080p)
   - Fire TV Stick 4K Max: High/Ultra (4K)

![NVR Addon Settings](https://aiit-inc.com/images/nvr-addon-settings.jpg)

## 🎮 Step 8: Optimize for Fire TV

### Remote Control Setup

1. In Kodi, go to **Settings** > **System** > **Input**
2. Configure remote control for optimal navigation

### Performance Optimization

1. In Kodi, go to **Settings** > **Player** > **Videos**
2. Adjust settings based on your Fire TV model:
   - **Allow hardware acceleration**: Enabled
   - **Processing method**: Auto-detect
   - **Adjust display refresh rate**: On start/stop

## 🔍 Troubleshooting

### Connection Issues

- **Problem**: ADB cannot connect to Fire TV
  - **Solution**: Verify both devices are on same network
  - **Solution**: Restart Fire TV and try again
  - **Solution**: Try USB connection instead of Wi-Fi

### Installation Failures

- **Problem**: "App not installed" error
  - **Solution**: Free up storage space on Fire TV
  - **Solution**: Download correct APK version (ARM)
  - **Solution**: Try installing via Downloader app instead

### Addon Not Working

- **Problem**: Cannot connect to NVR
  - **Solution**: Verify NVR IP address and port
  - **Solution**: Check username and password
  - **Solution**: Ensure NVR is running and accessible

### Performance Issues

- **Problem**: Stuttering or buffering
  - **Solution**: Lower stream quality
  - **Solution**: Increase buffer size in advanced settings
  - **Solution**: Use 5GHz Wi-Fi or Ethernet adapter

## 📞 Support

If you encounter any issues not covered in this guide:

- **Email**: support@aiit-inc.com
- **Website**: https://aiit-inc.com/support
- **Phone**: 1-800-AIIT-SUP

---

© 2025 AI-IT Inc. All rights reserved.
\`\`\`

```powershell file="scripts/pc_to_firestick.ps1"
<#
.SYNOPSIS
    Installs AI-IT Inc NVR Kodi addon from PC to Fire TV Stick
.DESCRIPTION
    This script automates the process of installing Kodi and the AI-IT Inc NVR addon
    on a Fire TV Stick from a Windows PC using ADB.
.NOTES
    File Name      : pc_to_firestick.ps1
    Author         : AI-IT Inc
    Prerequisite   : PowerShell 5.1 or later
    Copyright      : (c) 2025 AI-IT Inc
#>

# Script configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

# Banner
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  AI-IT Inc NVR - Fire TV Stick Installer" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Create temporary directory
$tempDir = Join-Path $env:TEMP "AIIT-NVR-Installer"
if (!(Test-Path $tempDir)) {
    New-Item -ItemType Directory -Path $tempDir | Out-Null
}

# Function to download file with progress
function Download-File {
    param (
        [string]$Url,
        [string]$OutputPath
    )
    
    Write-Host "Downloading $Url..." -ForegroundColor Yellow
    
    try {
        $webClient = New-Object System.Net.WebClient
        $webClient.DownloadFile($Url, $OutputPath)
        Write-Host "Download complete!" -ForegroundColor Green
    }
    catch {
        Write-Host "Download failed: $_" -ForegroundColor Red
        exit 1
    }
}

# Function to check if a command exists
function Test-Command {
    param (
        [string]$Command
    )
    
    $exists = $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
    return $exists
}

# Step 1: Check for ADB and install if needed
Write-Host "Step 1: Checking for ADB installation..." -ForegroundColor Cyan

$adbPath = ""
if (Test-Command "adb") {
    $adbPath = (Get-Command "adb").Source
    Write-Host "ADB already installed at: $adbPath" -ForegroundColor Green
}
else {
    Write-Host "ADB not found. Installing Platform Tools..." -ForegroundColor Yellow
    
    # Download ADB Platform Tools
    $platformToolsUrl = "https://dl.google.com/android/repository/platform-tools-latest-windows.zip"
    $platformToolsZip = Join-Path $tempDir "platform-tools.zip"
    $platformToolsDir = Join-Path $tempDir "platform-tools"
    
    Download-File -Url $platformToolsUrl -OutputPath $platformToolsZip
    
    # Extract Platform Tools
    Write-Host "Extracting Platform Tools..." -ForegroundColor Yellow
    Expand-Archive -Path $platformToolsZip -DestinationPath $tempDir -Force
    
    # Set ADB path
    $adbPath = Join-Path $platformToolsDir "adb.exe"
    
    # Add to PATH temporarily
    $env:Path += ";$platformToolsDir"
    
    Write-Host "ADB installed successfully!" -ForegroundColor Green
}

# Step 2: Connect to Fire TV Stick
Write-Host "`nStep 2: Connect to Fire TV Stick" -ForegroundColor Cyan
Write-Host "Please ensure your Fire TV Stick has:" -ForegroundColor Yellow
Write-Host "  1. Developer Options enabled" -ForegroundColor Yellow
Write-Host "  2. ADB Debugging turned ON" -ForegroundColor Yellow
Write-Host "  3. Apps from Unknown Sources enabled" -ForegroundColor Yellow
Write-Host ""

$firetvIp = Read-Host "Enter your Fire TV Stick IP address"

Write-Host "Connecting to Fire TV Stick at $firetvIp..." -ForegroundColor Yellow
& $adbPath connect "$($firetvIp):5555"

# Verify connection
$devices = & $adbPath devices
if ($devices -match $firetvIp) {
    Write-Host "Successfully connected to Fire TV Stick!" -ForegroundColor Green
}
else {
    Write-Host "Failed to connect to Fire TV Stick. Please check:" -ForegroundColor Red
    Write-Host "  - Fire TV Stick and PC are on the same network" -ForegroundColor Red
    Write-Host "  - Developer options and ADB debugging are enabled" -ForegroundColor Red
    Write-Host "  - IP address is correct" -ForegroundColor Red
    Write-Host "  - You accepted the connection prompt on your Fire TV" -ForegroundColor Red
    exit 1
}

# Step 3: Download Kodi and NVR addon
Write-Host "`nStep 3: Downloading required files..." -ForegroundColor Cyan

# Download Kodi APK
$kodiUrl = "https://mirrors.kodi.tv/releases/android/arm/kodi-20.2-Nexus-armeabi-v7a.apk"
$kodiApk = Join-Path $tempDir "kodi.apk"
Download-File -Url $kodiUrl -OutputPath $kodiApk

# Download NVR addon
$addonUrl = "https://aiit-inc.com/downloads/plugin.video.aiit-nvr-1.0.0.zip"
$addonZip = Join-Path $tempDir "plugin.video.aiit-nvr.zip"
Download-File -Url $addonUrl -OutputPath $addonZip

# Step 4: Install Kodi on Fire TV Stick
Write-Host "`nStep 4: Installing Kodi on Fire TV Stick..." -ForegroundColor Cyan
& $adbPath install -r $kodiApk

# Check if installation was successful
if ($LASTEXITCODE -eq 0) {
    Write-Host "Kodi installed successfully!" -ForegroundColor Green
}
else {
    Write-Host "Failed to install Kodi. Error code: $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

# Step 5: Transfer NVR addon to Fire TV Stick
Write-Host "`nStep 5: Transferring NVR addon to Fire TV Stick..." -ForegroundColor Cyan
& $adbPath push $addonZip /sdcard/Download/

# Check if transfer was successful
if ($LASTEXITCODE -eq 0) {
    Write-Host "NVR addon transferred successfully!" -ForegroundColor Green
}
else {
    Write-Host "Failed to transfer NVR addon. Error code: $LASTEXITCODE" -ForegroundColor Red
    exit 1
}

# Step 6: Launch Kodi
Write-Host "`nStep 6: Launching Kodi..." -ForegroundColor Cyan
& $adbPath shell am start -n org.xbmc.kodi/.Splash

Write-Host "`nInstallation process completed!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. In Kodi, go to Settings > Add-ons" -ForegroundColor White
Write-Host "2. Select 'Install from zip file'" -ForegroundColor White
Write-Host "3. If prompted, enable 'Unknown sources'" -ForegroundColor White
Write-Host "4. Navigate to 'External storage' > 'Download'" -ForegroundColor White
Write-Host "5. Select 'plugin.video.aiit-nvr.zip'" -ForegroundColor White
Write-Host "6. Wait for 'Add-on installed' notification" -ForegroundColor White
Write-Host "7. Configure the addon with your NVR details" -ForegroundColor White

Write-Host "`nThank you for choosing AI-IT Inc NVR!" -ForegroundColor Cyan
