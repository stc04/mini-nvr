#!/bin/bash

# AI-IT Inc NVR Installation Script for Linux
# Supports Ubuntu, Debian, Raspberry Pi OS

set -e

echo "AI-IT Inc NVR System - Linux Installation"
echo "=========================================="

# Detect system
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$NAME
    VER=$VERSION_ID
else
    echo "Cannot detect OS version"
    exit 1
fi

echo "Detected OS: $OS $VER"

# Check if running on Raspberry Pi
if grep -q "Raspberry Pi" /proc/device-tree/model 2>/dev/null; then
    IS_RPI=true
    echo "Detected: Raspberry Pi"
else
    IS_RPI=false
fi

# Check architecture
ARCH=$(uname -m)
echo "Architecture: $ARCH"

# Update system
echo "Updating system packages..."
sudo apt-get update

# Install dependencies
echo "Installing dependencies..."
sudo apt-get install -y \
    curl \
    wget \
    unzip \
    software-properties-common \
    apt-transport-https \
    ca-certificates \
    gnupg \
    lsb-release

# Install .NET 8.0
echo "Installing .NET 8.0..."
if [ "$ARCH" = "armv7l" ] || [ "$ARCH" = "aarch64" ]; then
    # ARM architecture (Raspberry Pi)
    curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0 --runtime aspnetcore
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
    echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
    export DOTNET_ROOT=$HOME/.dotnet
    export PATH=$PATH:$HOME/.dotnet
else
    # x64 architecture
    wget https://packages.microsoft.com/config/ubuntu/$(lsb_release -rs)/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
    sudo dpkg -i packages-microsoft-prod.deb
    rm packages-microsoft-prod.deb
    sudo apt-get update
    sudo apt-get install -y aspnetcore-runtime-8.0
fi

# Install FFmpeg and video processing tools
echo "Installing video processing tools..."
sudo apt-get install -y \
    ffmpeg \
    v4l-utils \
    gstreamer1.0-tools \
    gstreamer1.0-plugins-base \
    gstreamer1.0-plugins-good \
    gstreamer1.0-plugins-bad \
    gstreamer1.0-plugins-ugly \
    libgstreamer1.0-dev \
    libgstreamer-plugins-base1.0-dev

# Raspberry Pi specific setup
if [ "$IS_RPI" = true ]; then
    echo "Setting up Raspberry Pi optimizations..."
    
    # Enable camera
    sudo raspi-config nonint do_camera 0
    
    # Increase GPU memory
    if ! grep -q "gpu_mem=" /boot/config.txt; then
        echo "gpu_mem=128" | sudo tee -a /boot/config.txt
    fi
    
    # Enable hardware acceleration
    if ! grep -q "start_x=1" /boot/config.txt; then
        echo "start_x=1" | sudo tee -a /boot/config.txt
    fi
    
    # Install Raspberry Pi specific packages
    sudo apt-get install -y \
        libraspberrypi0 \
        libraspberrypi-dev \
        libraspberrypi-bin
fi

# Install hardware acceleration support
echo "Installing hardware acceleration support..."
if [ "$ARCH" = "x86_64" ]; then
    # Intel/AMD VA-API support
    sudo apt-get install -y \
        vainfo \
        intel-media-va-driver \
        mesa-va-drivers
fi

# Create application directory
APP_DIR="/opt/aiit-nvr"
echo "Creating application directory: $APP_DIR"
sudo mkdir -p $APP_DIR
sudo chown $USER:$USER $APP_DIR

# Create data directories
sudo mkdir -p /var/lib/aiit-nvr
sudo mkdir -p /var/log/aiit-nvr
sudo chown $USER:$USER /var/lib/aiit-nvr
sudo chown $USER:$USER /var/log/aiit-nvr

# Create systemd service
echo "Creating systemd service..."
sudo tee /etc/systemd/system/aiit-nvr.service > /dev/null <<EOF
[Unit]
Description=AI-IT Inc NVR System
After=network.target

[Service]
Type=notify
User=$USER
WorkingDirectory=$APP_DIR
ExecStart=$APP_DIR/AIIT.NVR.Linux
Restart=always
RestartSec=10
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080;https://0.0.0.0:8443

[Install]
WantedBy=multi-user.target
EOF

# Enable and start service
sudo systemctl daemon-reload
sudo systemctl enable aiit-nvr

# Create configuration file
echo "Creating configuration file..."
tee $APP_DIR/appsettings.json > /dev/null <<EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "NVRSettings": {
    "MaxCameras": 48,
    "RecordingQuality": "Medium",
    "StreamingPort": 8080,
    "RemoteAccessPort": 8443,
    "EnableSSL": false,
    "WebViewerEnabled": true,
    "DataPath": "/var/lib/aiit-nvr",
    "LogPath": "/var/log/aiit-nvr"
  },
  "HardwareOptimization": {
    "EnableGPUAcceleration": true,
    "OptimizeForLowMemory": $([ "$IS_RPI" = true ] && echo "true" || echo "false"),
    "MaxWorkerThreads": $(nproc),
    "VideoPreset": "ultrafast",
    "VideoCRF": 28
  },
  "RaspberryPi": {
    "IsRaspberryPi": $IS_RPI,
    "OptimizeForPi": $IS_RPI,
    "GPUMemoryMB": 128,
    "EnableCamera": true
  }
}
EOF

# Set up firewall rules
echo "Configuring firewall..."
if command -v ufw >/dev/null 2>&1; then
    sudo ufw allow 8080/tcp comment "AI-IT NVR HTTP"
    sudo ufw allow 8443/tcp comment "AI-IT NVR HTTPS"
    sudo ufw allow 554/tcp comment "RTSP"
fi

# Create startup script
echo "Creating startup script..."
tee $APP_DIR/start-nvr.sh > /dev/null <<'EOF'
#!/bin/bash

# AI-IT Inc NVR Startup Script

echo "Starting AI-IT Inc NVR System..."

# Check system resources
TOTAL_MEM=$(free -m | awk 'NR==2{printf "%.0f", $2}')
echo "Total Memory: ${TOTAL_MEM}MB"

if [ $TOTAL_MEM -lt 1024 ]; then
    echo "Warning: Low memory system detected. Some features may be limited."
    export AIIT_NVR_LOW_MEMORY=true
fi

# Check for GPU acceleration
if command -v vcgencmd >/dev/null 2>&1; then
    echo "Raspberry Pi VideoCore detected"
    export AIIT_NVR_GPU_TYPE=VideoCore
elif command -v nvidia-smi >/dev/null 2>&1; then
    echo "NVIDIA GPU detected"
    export AIIT_NVR_GPU_TYPE=NVIDIA
elif command -v vainfo >/dev/null 2>&1; then
    echo "VA-API support detected"
    export AIIT_NVR_GPU_TYPE=VAAPI
else
    echo "No GPU acceleration detected, using CPU"
    export AIIT_NVR_GPU_TYPE=CPU
fi

# Start the application
exec dotnet AIIT.NVR.Linux.dll
EOF

chmod +x $APP_DIR/start-nvr.sh

# Create web interface shortcut
echo "Creating web interface access..."
tee ~/Desktop/aiit-nvr-web.desktop > /dev/null <<EOF
[Desktop Entry]
Version=1.0
Type=Application
Name=AI-IT NVR Web Interface
Comment=Access AI-IT Inc NVR System
Exec=xdg-open http://localhost:8080
Icon=applications-multimedia
Terminal=false
Categories=AudioVideo;Video;
EOF

chmod +x ~/Desktop/aiit-nvr-web.desktop

echo ""
echo "Installation completed successfully!"
echo ""
echo "Next steps:"
echo "1. Copy the AI-IT NVR application files to: $APP_DIR"
echo "2. Start the service: sudo systemctl start aiit-nvr"
echo "3. Check status: sudo systemctl status aiit-nvr"
echo "4. Access web interface: http://localhost:8080"
echo ""
echo "Configuration file: $APP_DIR/appsettings.json"
echo "Log files: /var/log/aiit-nvr/"
echo ""

if [ "$IS_RPI" = true ]; then
    echo "Raspberry Pi specific notes:"
    echo "- GPU memory has been set to 128MB"
    echo "- Camera interface has been enabled"
    echo "- A reboot is recommended to apply all changes"
    echo ""
fi

echo "For support, visit: https://aiit-inc.com/nvr/support"
