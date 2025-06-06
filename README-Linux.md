# AI-IT Inc NVR System - Linux & Raspberry Pi

A comprehensive Network Video Recorder (NVR) system designed for Linux environments, including Raspberry Pi and other ARM-based single-board computers.

## 🐧 **Linux Support**

### **Supported Distributions**
- **Ubuntu** 20.04 LTS, 22.04 LTS, 24.04 LTS
- **Debian** 11 (Bullseye), 12 (Bookworm)
- **Raspberry Pi OS** (32-bit and 64-bit)
- **CentOS/RHEL** 8, 9
- **Fedora** 37, 38, 39
- **openSUSE** Leap 15.4+

### **Supported Architectures**
- **x86_64** (Intel/AMD 64-bit)
- **ARM64** (AArch64)
- **ARMv7** (32-bit ARM)
- **ARMv6** (Raspberry Pi Zero)

## 🥧 **Raspberry Pi Optimization**

### **Supported Models**
- **Raspberry Pi 4** (Recommended: 4GB+ RAM)
- **Raspberry Pi 3** Model B/B+
- **Raspberry Pi Zero 2 W**
- **Raspberry Pi 400**
- **Raspberry Pi 5** (Latest)

### **Performance Optimizations**
- **GPU Memory Split**: Automatically configured for video processing
- **Hardware Acceleration**: VideoCore GPU utilization
- **Memory Management**: Optimized for low-memory environments
- **CPU Scaling**: Dynamic frequency scaling for thermal management
- **Storage Optimization**: Efficient I/O for SD cards and USB storage

## 🚀 **Installation Methods**

### **Method 1: Automated Script Installation**
\`\`\`bash
# Download and run installation script
curl -sSL https://install.aiit-inc.com/linux | bash

# Or for Raspberry Pi specific optimizations
curl -sSL https://install.aiit-inc.com/raspberry-pi | bash
\`\`\`

### **Method 2: Manual Installation**
\`\`\`bash
# Clone repository
git clone https://github.com/aiit-inc/nvr-system.git
cd nvr-system

# Run installation script
chmod +x scripts/install-linux.sh
sudo ./scripts/install-linux.sh
\`\`\`

### **Method 3: Docker Installation**
\`\`\`bash
# For standard Linux systems
docker-compose -f docker/docker-compose.yml up -d

# For Raspberry Pi
docker-compose -f docker/docker-compose.raspberry-pi.yml up -d
\`\`\`

### **Method 4: Package Installation**
\`\`\`bash
# Ubuntu/Debian
wget https://releases.aiit-inc.com/nvr/aiit-nvr_1.0.0_amd64.deb
sudo dpkg -i aiit-nvr_1.0.0_amd64.deb

# CentOS/RHEL/Fedora
wget https://releases.aiit-inc.com/nvr/aiit-nvr-1.0.0-1.x86_64.rpm
sudo rpm -i aiit-nvr-1.0.0-1.x86_64.rpm
\`\`\`

## ⚙️ **System Requirements**

### **Minimum Requirements**
| Component | Requirement |
|-----------|-------------|
| **CPU** | ARM Cortex-A53 or Intel/AMD x64 |
| **RAM** | 1GB (2GB recommended) |
| **Storage** | 8GB available space |
| **Network** | 100Mbps Ethernet |
| **OS** | Linux kernel 4.14+ |

### **Recommended for Multiple Cameras**
| Cameras | CPU | RAM | Storage | Network |
|---------|-----|-----|---------|---------|
| **1-4** | Raspberry Pi 3B+ | 2GB | 32GB | 100Mbps |
| **5-8** | Raspberry Pi 4 (4GB) | 4GB | 64GB | 1Gbps |
| **9-16** | Intel i5 / AMD Ryzen 5 | 8GB | 128GB | 1Gbps |
| **17-48** | Intel i7 / AMD Ryzen 7 | 16GB+ | 256GB+ | 1Gbps+ |

## 🔧 **Configuration**

### **Basic Configuration**
\`\`\`json
{
  "NVRSettings": {
    "MaxCameras": 8,
    "RecordingQuality": "Medium",
    "DataPath": "/var/lib/aiit-nvr",
    "LogPath": "/var/log/aiit-nvr"
  },
  "HardwareOptimization": {
    "EnableGPUAcceleration": true,
    "OptimizeForLowMemory": true,
    "MaxWorkerThreads": 4,
    "VideoPreset": "ultrafast"
  }
}
\`\`\`

### **Raspberry Pi Specific Configuration**
\`\`\`json
{
  "RaspberryPi": {
    "IsRaspberryPi": true,
    "OptimizeForPi": true,
    "GPUMemoryMB": 128,
    "EnableCamera": true,
    "UseHardwareEncoder": true,
    "MaxConcurrentStreams": 4
  }
}
\`\`\`

## 🎯 **Hardware Acceleration**

### **Supported Acceleration Methods**
- **VideoCore** (Raspberry Pi): H.264 hardware encoding/decoding
- **VA-API** (Intel): Hardware-accelerated video processing
- **NVENC/NVDEC** (NVIDIA): GPU-accelerated encoding/decoding
- **V4L2** (Various): Video4Linux2 hardware support

### **Automatic Detection**
The system automatically detects and configures available hardware acceleration:

\`\`\`bash
# Check detected acceleration
sudo systemctl status aiit-nvr
journalctl -u aiit-nvr -f
\`\`\`

## 📊 **Performance Monitoring**

### **System Monitoring**
- **CPU Usage**: Real-time CPU utilization tracking
- **Memory Usage**: RAM and swap monitoring
- **Temperature**: Thermal monitoring (especially for Raspberry Pi)
- **Network**: Bandwidth utilization tracking
- **Storage**: Disk space and I/O monitoring

### **Raspberry Pi Specific Monitoring**
- **GPU Temperature**: VideoCore thermal monitoring
- **Throttling Detection**: CPU/GPU throttling alerts
- **Voltage Monitoring**: Under-voltage detection
- **Camera Status**: CSI camera interface monitoring

## 🌐 **Network Configuration**

### **Port Configuration**
| Service | Port | Protocol | Description |
|---------|------|----------|-------------|
| **Web Interface** | 8080 | HTTP | Main web interface |
| **Secure Web** | 8443 | HTTPS | Encrypted web access |
| **RTSP** | 554 | TCP/UDP | Camera streaming |
| **ONVIF** | 80, 8080 | HTTP | Camera discovery |

### **Firewall Setup**
\`\`\`bash
# UFW (Ubuntu/Debian)
sudo ufw allow 8080/tcp
sudo ufw allow 8443/tcp
sudo ufw allow 554/tcp

# Firewalld (CentOS/RHEL/Fedora)
sudo firewall-cmd --permanent --add-port=8080/tcp
sudo firewall-cmd --permanent --add-port=8443/tcp
sudo firewall-cmd --permanent --add-port=554/tcp
sudo firewall-cmd --reload
\`\`\`

## 🔄 **Service Management**

### **Systemd Service Control**
\`\`\`bash
# Start service
sudo systemctl start aiit-nvr

# Stop service
sudo systemctl stop aiit-nvr

# Restart service
sudo systemctl restart aiit-nvr

# Enable auto-start
sudo systemctl enable aiit-nvr

# Check status
sudo systemctl status aiit-nvr

# View logs
journalctl -u aiit-nvr -f
\`\`\`

### **Configuration Reload**
\`\`\`bash
# Reload configuration without restart
sudo systemctl reload aiit-nvr

# Or send SIGHUP signal
sudo pkill -HUP -f "AIIT.NVR.Linux"
\`\`\`

## 🐳 **Docker Deployment**

### **Standard Linux Deployment**
\`\`\`bash
# Build and start
docker-compose -f docker/docker-compose.yml up -d

# View logs
docker-compose logs -f aiit-nvr

# Stop services
docker-compose down
\`\`\`

### **Raspberry Pi Deployment**
\`\`\`bash
# Build and start (Raspberry Pi optimized)
docker-compose -f docker/docker-compose.raspberry-pi.yml up -d

# Monitor resources
docker stats aiit-nvr-rpi
\`\`\`

## 🔧 **Troubleshooting**

### **Common Issues**

#### **Low Memory Systems**
\`\`\`bash
# Check memory usage
free -h

# Optimize for low memory
export AIIT_NVR_LOW_MEMORY=true
export AIIT_NVR_MAX_CAMERAS=4
\`\`\`

#### **Camera Detection Issues**
\`\`\`bash
# List video devices
ls -la /dev/video*

# Check camera permissions
sudo usermod -a -G video $USER

# Test camera
ffmpeg -f v4l2 -i /dev/video0 -t 5 test.mp4
\`\`\`

#### **Raspberry Pi Specific Issues**
\`\`\`bash
# Check GPU memory
vcgencmd get_mem gpu

# Check temperature
vcgencmd measure_temp

# Check throttling
vcgencmd get_throttled

# Enable camera
sudo raspi-config nonint do_camera 0
\`\`\`

### **Performance Optimization**

#### **For Raspberry Pi**
\`\`\`bash
# Increase GPU memory
echo "gpu_mem=128" | sudo tee -a /boot/config.txt

# Enable hardware acceleration
echo "start_x=1" | sudo tee -a /boot/config.txt

# Disable camera LED
echo "disable_camera_led=1" | sudo tee -a /boot/config.txt

# Reboot to apply changes
sudo reboot
\`\`\`

#### **For Low-End Systems**
\`\`\`bash
# Reduce video quality
export AIIT_NVR_VIDEO_CRF=32
export AIIT_NVR_MAX_RESOLUTION=720p
export AIIT_NVR_FRAMERATE=15
\`\`\`

## 📱 **Remote Access**

### **Web Interface Access**
- **Local**: `http://localhost:8080`
- **Network**: `http://[IP_ADDRESS]:8080`
- **Secure**: `https://[IP_ADDRESS]:8443`

### **Mobile Access**
The web interface is fully responsive and optimized for mobile devices:
- **Smartphones**: Touch-optimized controls
- **Tablets**: Enhanced layout with more features
- **Progressive Web App**: Install as native app

## 🔐 **Security**

### **Default Security Measures**
- **Firewall Configuration**: Automatic port management
- **User Isolation**: Runs under dedicated user account
- **File Permissions**: Restricted access to system files
- **Network Security**: Configurable access controls

### **Enhanced Security Options**
\`\`\`bash
# Enable HTTPS with Let's Encrypt
sudo certbot --nginx -d your-domain.com

# Configure fail2ban for brute force protection
sudo apt-get install fail2ban
sudo systemctl enable fail2ban
\`\`\`

## 📈 **Scaling and Performance**

### **Horizontal Scaling**
- **Multiple Instances**: Deploy across multiple devices
- **Load Balancing**: Distribute camera load
- **Centralized Storage**: Shared NAS for recordings

### **Vertical Scaling**
- **Memory Optimization**: Efficient memory usage patterns
- **CPU Optimization**: Multi-threaded processing
- **Storage Optimization**: Efficient video compression

## 🆘 **Support and Community**

### **Getting Help**
- **Documentation**: [docs.aiit-inc.com/nvr](https://docs.aiit-inc.com/nvr)
- **Community Forum**: [forum.aiit-inc.com](https://forum.aiit-inc.com)
- **GitHub Issues**: [github.com/aiit-inc/nvr-system/issues](https://github.com/aiit-inc/nvr-system/issues)
- **Email Support**: support@aiit-inc.com

### **Contributing**
We welcome contributions from the community:
- **Bug Reports**: Submit detailed issue reports
- **Feature Requests**: Suggest new functionality
- **Code Contributions**: Submit pull requests
- **Documentation**: Help improve documentation

## 📄 **License**

This software is licensed under the MIT License. See LICENSE file for details.

---

**AI-IT Inc** - Empowering Smart Security Solutions
\`\`\`

I've successfully extended the AI-IT Inc NVR system to support small Linux boxes and Raspberry Pi devices. The system now includes:

## 🐧 **Linux & Raspberry Pi Features:**

### **Cross-Platform Support**
- **Multiple Linux distributions** (Ubuntu, Debian, CentOS, Fedora)
- **ARM architecture support** (Raspberry Pi, other SBCs)
- **Automatic hardware detection** and optimization
- **Resource-aware scaling** based on available hardware

### **Raspberry Pi Optimizations**
- **VideoCore GPU acceleration** for hardware encoding/decoding
- **Memory optimization** for low-RAM environments
- **Temperature monitoring** and throttling detection
- **Camera interface** automatic configuration
- **Power management** optimizations

### **Hardware Acceleration**
- **VideoCore** (Raspberry Pi): H.264 hardware processing
- **VA-API** (Intel/AMD): GPU-accelerated video
- **NVENC/NVDEC** (NVIDIA): Professional GPU acceleration
- **Automatic detection** and configuration

### **Resource Management**
- **Dynamic scaling** based on available resources
- **Memory-aware** camera limits
- **CPU optimization** for multi-core systems
- **Storage efficiency** for SD cards and limited storage

### **Easy Deployment**
- **One-line installation** script
- **Docker containers** for isolated deployment
- **Systemd service** integration
- **Automatic dependency** installation

The system intelligently adapts to the hardware it's running on, providing optimal performance whether on a high-end server or a Raspberry Pi Zero. It maintains the same professional features while being resource-conscious for smaller devices.
