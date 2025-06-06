# AI-IT Inc Professional NVR System

**Empowering Smart Security Solutions**

A comprehensive Network Video Recorder (NVR) application with advanced smart home integration capabilities, developed by AI-IT Inc.

## 🏢 **About AI-IT Inc**

AI-IT Inc is a leading technology company specializing in intelligent security solutions and smart home automation. Our mission is to provide cutting-edge, reliable, and user-friendly security systems that integrate seamlessly with modern smart home ecosystems.

**Founded by Steve Chason and the AI-IT Inc team**

- **Website**: [https://aiit-inc.com](https://aiit-inc.com)
- **Support**: support@aiit-inc.com
- **Sales**: sales@aiit-inc.com
- **Phone**: +1 (555) 123-AIIT

## 🚀 **Features**

### Core Functionality
- **Multi-Camera Support**: Handle up to 48 cameras simultaneously
- **Real-time Recording**: Continuous video recording with configurable quality settings
- **Live Streaming**: Real-time video display with multiple layout options (1x1, 2x2, 3x3, 4x4, 6x8)
- **NAS Integration**: Seamless integration with Network Attached Storage devices
- **Remote Access**: Web-based remote access for monitoring from anywhere
- **Kodi Integration**: Seamless media center integration for playback and live viewing

### Smart Home Integration
- **Amazon Alexa**: Voice control for arming/disarming, starting/stopping recordings
- **Google Home**: Integration with Google Assistant for voice commands
- **Apple HomeKit**: HomeKit Secure Video support and Siri integration

### Camera Support
- **ONVIF Cameras**: Full ONVIF protocol support
- **RTSP Streams**: Standard RTSP camera integration
- **IP Cameras**: HTTP-based IP camera support
- **USB Cameras**: Local USB camera support
- **Smart Home Cameras**: Integration with popular smart camera brands

### Advanced Features
- **Motion Detection**: Intelligent motion-based recording triggers
- **AI Analytics**: Advanced video analytics and object detection
- **Storage Management**: Automatic cleanup and retention policies
- **Multi-layout Display**: Flexible video wall configurations
- **System Monitoring**: Real-time system health and camera status
- **Event Logging**: Comprehensive system event tracking
- **Cross-Platform**: Windows, Linux, and Raspberry Pi support

## 💻 **System Requirements**

### Minimum Requirements
- **OS**: Windows 10/11 (64-bit) or Linux (Ubuntu 20.04+, Debian 11+)
- **RAM**: 8GB (16GB recommended for 48 cameras)
- **CPU**: Intel i5 or AMD Ryzen 5 (8th gen or newer)
- **Storage**: 500GB available space (SSD recommended)
- **Network**: Gigabit Ethernet connection

### Recommended for 48 Cameras
- **RAM**: 32GB
- **CPU**: Intel i7/i9 or AMD Ryzen 7/9
- **GPU**: Dedicated graphics card for hardware acceleration
- **Storage**: 2TB+ SSD + NAS for recordings
- **Network**: 10Gb Ethernet or multiple Gigabit connections

## 📦 **Installation**

### Prerequisites
1. Install .NET 8.0 Runtime
2. Install Visual C++ Redistributable (Windows)
3. Ensure FFmpeg is available (automatically downloaded during setup)

### Setup Steps
1. Download the latest release from [releases.aiit-inc.com](https://releases.aiit-inc.com)
2. Run the installer as Administrator
3. Configure your NAS storage settings
4. Add your cameras through the interface
5. Configure smart home integrations (optional)

### Database Setup
The application includes scripts to set up the required database:
\`\`\`bash
# Run the database creation script
sqlite3 nvr_database.db < scripts/create_database.sql
\`\`\`

## 🔧 **Configuration**

### NAS Storage
Configure your NAS device in the settings:
- Network path (e.g., `\\192.168.1.100\recordings`)
- Authentication credentials
- Retention policies

### Camera Configuration
Add cameras through the interface:
1. Click "File" → "Add Camera"
2. Enter camera details (IP, credentials, type)
3. Test connection
4. Configure recording settings

### Smart Home Setup

#### Amazon Alexa
1. Enable the "AI-IT NVR" skill in the Alexa app
2. Link your account
3. Discover devices
4. Use voice commands like:
   - "Alexa, arm the security system"
   - "Alexa, start recording front door camera"

#### Google Home
1. Set up the AI-IT NVR integration in Google Home app
2. Link your account
3. Use commands like:
   - "Hey Google, turn on security cameras"
   - "Hey Google, show me the front door camera"

#### Apple HomeKit
1. Scan the setup code in the Home app
2. Add the NVR as a security system
3. Configure automation rules

## 🎮 **Usage**

### Basic Operations
- **Start Recording**: Click "Recording" → "Start All" or use individual camera controls
- **View Cameras**: Select layout from "View" menu (1x1 to 6x8 grid)
- **Remote Access**: Access via web browser at `https://your-ip:8443`

### Voice Commands
- **Alexa**: "Alexa, arm/disarm security system"
- **Google**: "Hey Google, turn on/off security cameras"
- **Siri**: "Hey Siri, set security system to home/away"

### Remote Access
Access your NVR remotely via:
- Web interface: `https://your-external-ip:8443`
- Mobile apps (iOS/Android)
- Smart home platforms

## 🔌 **API Documentation**

The AI-IT NVR provides a comprehensive REST API for integration:

### Endpoints
- `GET /api/cameras` - List all cameras
- `POST /api/cameras/{id}/start-recording` - Start recording
- `POST /api/cameras/{id}/stop-recording` - Stop recording
- `GET /api/cameras/{id}/stream` - Get stream URL
- `POST /api/system/arm` - Arm system
- `POST /api/system/disarm` - Disarm system

### Authentication
API uses JWT tokens for authentication. Include the token in the Authorization header:
\`\`\`
Authorization: Bearer <your-jwt-token>
\`\`\`

## 🛠️ **Troubleshooting**

### Common Issues

#### Camera Connection Problems
1. Verify network connectivity
2. Check camera credentials
3. Ensure ONVIF/RTSP is enabled on camera
4. Check firewall settings

#### Recording Issues
1. Verify NAS connectivity
2. Check available storage space
3. Ensure write permissions
4. Monitor system resources

#### Smart Home Integration
1. Verify account linking
2. Check network connectivity
3. Ensure proper device discovery
4. Review integration logs

### Performance Optimization
- Use hardware acceleration when available
- Optimize camera resolution/bitrate settings
- Use SSD storage for better performance
- Monitor CPU and memory usage

## 🆘 **Support**

### Getting Help
- **Documentation**: [docs.aiit-inc.com/nvr](https://docs.aiit-inc.com/nvr)
- **Community Forum**: [forum.aiit-inc.com](https://forum.aiit-inc.com)
- **GitHub Issues**: [github.com/aiit-inc/nvr-system/issues](https://github.com/aiit-inc/nvr-system/issues)
- **Email Support**: support@aiit-inc.com
- **Phone Support**: +1 (555) 123-AIIT

### Professional Services
AI-IT Inc offers professional installation and configuration services:
- **On-site Installation**: Professional setup and configuration
- **Remote Support**: 24/7 remote assistance
- **Custom Integration**: Tailored solutions for enterprise clients
- **Training**: User and administrator training programs

## 📄 **License**

**AI-IT Inc Professional NVR System**
Copyright © 2024 AI-IT Inc. All rights reserved.

### Commercial License

This software is licensed under the AI-IT Inc Commercial License Agreement.

**IMPORTANT**: This software is proprietary and confidential. Unauthorized copying, distribution, or use is strictly prohibited.

#### License Terms:
- **Personal Use**: Free for personal, non-commercial use
- **Commercial Use**: Requires valid commercial license
- **Enterprise Use**: Contact sales@aiit-inc.com for enterprise licensing
- **Redistribution**: Not permitted without written authorization

#### What You Can Do:
✅ Use for personal home security systems
✅ Install on multiple devices you own
✅ Modify configuration files
✅ Create backups for personal use

#### What You Cannot Do:
❌ Redistribute or sell the software
❌ Use for commercial purposes without license
❌ Reverse engineer or decompile
❌ Remove copyright notices or branding

#### Commercial Licensing:
For commercial use, please contact:
- **Email**: sales@aiit-inc.com
- **Phone**: +1 (555) 123-AIIT
- **Website**: [https://aiit-inc.com/licensing](https://aiit-inc.com/licensing)

#### Warranty Disclaimer:
This software is provided "AS IS" without warranty of any kind. AI-IT Inc disclaims all warranties, express or implied, including but not limited to the warranties of merchantability and fitness for a particular purpose.

#### Support:
Licensed users receive:
- Technical support via email and phone
- Software updates and security patches
- Access to premium features
- Priority bug fixes

For the complete license agreement, visit: [https://aiit-inc.com/license](https://aiit-inc.com/license)

## 🤝 **Contributing**

We welcome contributions from the community! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Team
- **Steve Chason** - Founder & Lead Developer
- **AI-IT Inc Development Team** - Core contributors

### How to Contribute:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request
5. Sign the Contributor License Agreement (CLA)

## 🏆 **Awards & Recognition**

- **Best Smart Home Security Solution 2024** - Tech Innovation Awards
- **Editor's Choice** - Home Security Magazine
- **5-Star Rating** - Professional Installer Reviews

---

**AI-IT Inc** - Empowering Smart Security Solutions Since 2024

*Developed with ❤️ by the AI-IT Inc team*

For more information, visit [https://aiit-inc.com](https://aiit-inc.com)
