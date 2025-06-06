#!/usr/bin/env python3
"""
Create Kodi addon ZIP package for AI-IT Inc NVR
"""

import os
import zipfile
import shutil
from pathlib import Path

def create_addon_zip():
    """Create the Kodi addon ZIP file"""
    
    # Define paths
    addon_dir = Path("kodi-addon")
    output_dir = Path("releases")
    zip_filename = "plugin.video.aiit-nvr-1.0.0.zip"
    zip_path = output_dir / zip_filename
    
    # Create output directory
    output_dir.mkdir(exist_ok=True)
    
    # Remove existing ZIP if it exists
    if zip_path.exists():
        zip_path.unlink()
    
    print(f"Creating Kodi addon ZIP: {zip_filename}")
    
    # Create ZIP file
    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        
        # Add all files from addon directory
        for root, dirs, files in os.walk(addon_dir):
            for file in files:
                file_path = Path(root) / file
                # Calculate relative path for ZIP
                arcname = file_path.relative_to(addon_dir.parent)
                zipf.write(file_path, arcname)
                print(f"  Added: {arcname}")
    
    print(f"\n✅ Kodi addon ZIP created successfully!")
    print(f"📁 Location: {zip_path.absolute()}")
    print(f"📊 Size: {zip_path.stat().st_size / 1024:.1f} KB")
    
    # Create installation instructions
    instructions_file = output_dir / "KODI_INSTALLATION_INSTRUCTIONS.txt"
    with open(instructions_file, 'w') as f:
        f.write("""
AI-IT Inc NVR Kodi Addon Installation Instructions
================================================

INSTALLATION METHODS:

Method 1: Install from ZIP file (Recommended)
--------------------------------------------
1. Copy 'plugin.video.aiit-nvr-1.0.0.zip' to your device
2. Open Kodi
3. Go to Settings > Add-ons
4. Select "Install from zip file"
5. Navigate to the ZIP file location
6. Select 'plugin.video.aiit-nvr-1.0.0.zip'
7. Wait for installation confirmation
8. Configure the addon in Settings

Method 2: Manual Installation
----------------------------
1. Extract 'plugin.video.aiit-nvr-1.0.0.zip'
2. Copy the 'plugin.video.aiit-nvr' folder to:
   - Windows: %APPDATA%\\Kodi\\addons\\
   - Linux: ~/.kodi/addons/
   - macOS: ~/Library/Application Support/Kodi/addons/
   - Android: /storage/emulated/0/Android/data/org.xbmc.kodi/files/.kodi/addons/
3. Restart Kodi
4. Enable the addon in Settings > Add-ons > Video add-ons

CONFIGURATION:
1. Go to Add-ons > Video add-ons > AI-IT Inc NVR
2. Right-click and select "Settings"
3. Configure your NVR connection details:
   - NVR Host: IP address of your NVR system
   - NVR Port: Port number (default: 8080)
   - Username: Your NVR login username
   - Password: Your NVR login password
4. Adjust streaming and interface settings as needed
5. Test the connection by accessing the addon

USAGE:
- Access from Add-ons > Video add-ons > AI-IT Inc NVR
- Or add to favorites for quick access
- Use remote control or keyboard for navigation

SUPPORT:
- Email: support@aiit-inc.com
- Website: https://aiit-inc.com/support
- Forum: https://forum.aiit-inc.com/kodi

AI-IT Inc - Professional Security Solutions
""")
    
    print(f"📋 Installation instructions: {instructions_file}")
    
    return zip_path

if __name__ == "__main__":
    create_addon_zip()
