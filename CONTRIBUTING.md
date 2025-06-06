# Contributing to AI-IT Inc Professional NVR System

Thank you for your interest in contributing to the AI-IT Inc Professional NVR System! We welcome contributions from the community and are grateful for your support.

## 🏢 **About AI-IT Inc**

AI-IT Inc is committed to building the best smart security solutions. Founded by Steve Chason, our team is dedicated to creating innovative, reliable, and user-friendly security systems.

## 🤝 **How to Contribute**

### 1. **Code of Conduct**

By participating in this project, you agree to abide by our Code of Conduct:
- Be respectful and inclusive
- Focus on constructive feedback
- Help create a welcoming environment for all contributors
- Follow professional communication standards

### 2. **Types of Contributions**

We welcome various types of contributions:

#### 🐛 **Bug Reports**
- Use the GitHub issue tracker
- Provide detailed reproduction steps
- Include system information and logs
- Use the bug report template

#### 💡 **Feature Requests**
- Check existing issues first
- Provide clear use cases
- Explain the business value
- Consider implementation complexity

#### 📝 **Documentation**
- Improve existing documentation
- Add examples and tutorials
- Fix typos and formatting
- Translate documentation

#### 💻 **Code Contributions**
- Bug fixes
- New features
- Performance improvements
- Code refactoring

### 3. **Development Process**

#### **Getting Started**
1. Fork the repository
2. Clone your fork locally
3. Create a new branch for your feature/fix
4. Set up the development environment

#### **Development Environment Setup**
\`\`\`bash
# Clone the repository
git clone https://github.com/aiit-inc/nvr-system.git
cd nvr-system

# Install dependencies
dotnet restore

# Run setup scripts
python scripts/setup_ffmpeg.py
sqlite3 nvr_database.db < scripts/create_database.sql

# Build the project
dotnet build
\`\`\`

#### **Making Changes**
1. Create a feature branch: `git checkout -b feature/your-feature-name`
2. Make your changes
3. Write or update tests
4. Ensure all tests pass
5. Update documentation if needed
6. Commit your changes with clear messages

#### **Commit Message Guidelines**
Use clear, descriptive commit messages:
\`\`\`
feat: add motion detection for IP cameras
fix: resolve memory leak in video streaming
docs: update installation instructions
refactor: improve camera manager performance
\`\`\`

#### **Pull Request Process**
1. Push your branch to your fork
2. Create a pull request against the main branch
3. Fill out the pull request template
4. Ensure all checks pass
5. Respond to review feedback

### 4. **Contributor License Agreement (CLA)**

Before we can accept your contributions, you must sign our Contributor License Agreement (CLA). This ensures that:
- AI-IT Inc can continue to license the software
- Your contributions are properly attributed
- The project remains legally compliant

The CLA process is automated through our GitHub integration.

### 5. **Development Guidelines**

#### **Code Style**
- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and concise

#### **Testing**
- Write unit tests for new features
- Ensure existing tests continue to pass
- Test on multiple platforms when possible
- Include integration tests for complex features

#### **Performance**
- Consider memory usage, especially for Raspberry Pi
- Optimize for multi-camera scenarios
- Profile performance-critical code
- Document performance characteristics

#### **Security**
- Follow secure coding practices
- Validate all inputs
- Use parameterized queries
- Implement proper authentication

### 6. **Review Process**

#### **Code Review Criteria**
- Functionality: Does it work as intended?
- Code Quality: Is it well-written and maintainable?
- Performance: Does it meet performance requirements?
- Security: Are there any security concerns?
- Documentation: Is it properly documented?

#### **Review Timeline**
- Initial review: Within 3-5 business days
- Follow-up reviews: Within 2 business days
- Final approval: Within 1 business day

### 7. **Recognition**

Contributors will be recognized in:
- CONTRIBUTORS.md file
- Release notes
- Project documentation
- AI-IT Inc website (for significant contributions)

### 8. **Support for Contributors**

#### **Getting Help**
- **Discord**: [discord.gg/aiit-inc](https://discord.gg/aiit-inc)
- **Email**: developers@aiit-inc.com
- **Documentation**: [docs.aiit-inc.com/contributing](https://docs.aiit-inc.com/contributing)

#### **Mentorship Program**
New contributors can request mentorship from experienced team members:
- Code review guidance
- Architecture discussions
- Best practices training
- Career development advice

### 9. **Licensing**

By contributing to this project, you agree that your contributions will be licensed under the same license as the project (AI-IT Inc Commercial License).

### 10. **Special Recognition**

#### **Core Contributors**
Contributors who make significant, ongoing contributions may be invited to join the core team with additional privileges:
- Direct commit access
- Participation in roadmap planning
- Recognition as AI-IT Inc team member

#### **Bounty Program**
AI-IT Inc offers bounties for certain high-priority issues:
- Security vulnerabilities: $500-$2000
- Critical bug fixes: $100-$500
- Major feature implementations: $200-$1000

### 11. **Contact Information**

#### **Development Team**
- **Steve Chason** - Founder & Lead Developer (steve@aiit-inc.com)
- **Development Team** - developers@aiit-inc.com

#### **Business Inquiries**
- **Sales**: sales@aiit-inc.com
- **Partnerships**: partnerships@aiit-inc.com
- **Legal**: legal@aiit-inc.com

---

## 🚀 **Getting Started Checklist**

- [ ] Read this contributing guide
- [ ] Sign the Contributor License Agreement
- [ ] Set up development environment
- [ ] Join our Discord community
- [ ] Find a good first issue
- [ ] Make your first contribution!

Thank you for contributing to AI-IT Inc Professional NVR System!

---

**AI-IT Inc** - Empowering Smart Security Solutions
*Building the future of intelligent security systems*

For more information, visit [https://aiit-inc.com](https://aiit-inc.com)
\`\`\`

Now let's update the splash screen branding:

```typescriptreact file="AIIT.NVR/SplashScreen.xaml.cs"
[v0-no-op-code-block-prefix]using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AIIT.NVR.Core.Services;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using System.Linq;

namespace AIIT.NVR
{
    public partial class SplashScreen : Window
    {
        private NetworkScanService _networkScanService;
        private DispatcherTimer _scanTimer;
        private int _scanProgress = 0;
        private readonly string[] _scanMessages = new string[]
        {
            "Discovering devices on network...",
            "Checking for IP cameras...",
            "Scanning for smart home devices...",
            "Detecting NAS storage...",
            "Looking for Kodi installations...",
            "Checking Plex and Emby servers...",
            "Testing network bandwidth...",
            "Analyzing system resources...",
            "Finalizing compatibility report..."
        };

        public SplashScreen()
        {
            InitializeComponent();
            _networkScanService = new NetworkScanService();
            
            // Initialize scan timer
            _scanTimer = new DispatcherTimer();
            _scanTimer.Interval = TimeSpan.FromSeconds(1.5);
            _scanTimer.Tick += ScanTimer_Tick;
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            // Show loading UI
            ResultsGrid.Visibility = Visibility.Collapsed;
            LoadingGrid.Visibility = Visibility.Visible;
            ScanButton.IsEnabled = false;
            
            // Start scan timer for UI updates
            _scanProgress = 0;
            ScanStatusText.Text = _scanMessages[_scanProgress];
            _scanTimer.Start();
            
            // Start actual network scan in background
            Task.Run(async () =>
            {
                await _networkScanService.ScanNetworkAsync();
                
                // Update UI on completion
                Dispatcher.Invoke(() =>
                {
                    _scanTimer.Stop();
                    ShowScanResults();
                });
            });
        }

        private void ScanTimer_Tick(object sender, EventArgs e)
        {
            _scanProgress++;
            
            if (_scanProgress < _scanMessages.Length)
            {
                ScanStatusText.Text = _scanMessages[_scanProgress];
            }
            else
            {
                // If real scan is still running, loop back to first message
                _scanProgress = 0;
                ScanStatusText.Text = _scanMessages[_scanProgress];
            }
        }

        private void ShowScanResults()
        {
            // Get scan results
            var results = _networkScanService.GetScanResults();
            
            // Show results UI
            LoadingGrid.Visibility = Visibility.Collapsed;
            
            // Create results display
            var resultsPanel = new StackPanel();
            
            // Add header
            var header = new TextBlock
            {
                Text = "🎉 AI-IT Inc Network Scan Complete!",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            resultsPanel.Children.Add(header);
            
            // Add discovered devices
            if (results.Any())
            {
                var devicesHeader = new TextBlock
                {
                    Text = "Discovered Compatible Devices:",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(0, 0, 0, 10)
                };
                resultsPanel.Children.Add(devicesHeader);
                
                foreach (var device in results)
                {
                    var devicePanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(0, 0, 0, 5)
                    };
                    
                    var icon = new TextBlock
                    {
                        Text = GetDeviceIcon(device.DeviceType),
                        FontSize = 16,
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    
                    var deviceInfo = new TextBlock
                    {
                        Text = $"{device.DeviceName} ({device.IpAddress})",
                        Foreground = new SolidColorBrush(Colors.LightGray),
                        FontSize = 12
                    };
                    
                    devicePanel.Children.Add(icon);
                    devicePanel.Children.Add(deviceInfo);
                    resultsPanel.Children.Add(devicePanel);
                }
                
                // Check for Kodi specifically
                var kodiDevices = results.Where(r => r.DeviceName?.Contains("Kodi", StringComparison.OrdinalIgnoreCase) == true);
                if (kodiDevices.Any())
                {
                    var kodiNotice = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 15, 0, 0)
                    };
                    
                    var kodiText = new TextBlock
                    {
                        Text = $"🎉 Great! We found {kodiDevices.Count()} Kodi installation(s). Integration will be automatically configured!",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = new SolidColorBrush(Colors.White),
                        FontWeight = FontWeights.Bold,
                        FontSize = 12
                    };
                    
                    kodiNotice.Child = kodiText;
                    resultsPanel.Children.Add(kodiNotice);
                }
            }
            else
            {
                var noDevices = new TextBlock
                {
                    Text = "No compatible devices found on this network segment.\nYou can still proceed with manual configuration.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.Orange),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                resultsPanel.Children.Add(noDevices);
            }
            
            // Add recommendation
            var recommendation = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 46)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 20, 0, 0)
            };
            
            var recText = new TextBlock
            {
                Text = "✨ Ready to proceed with AI-IT Inc NVR! Click 'Launch NVR' to start with optimized settings based on your network scan.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.LightGray),
                FontSize = 12
            };
            
            recommendation.Child = recText;
            resultsPanel.Children.Add(recommendation);
            
            // Update the results grid
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = resultsPanel
            };
            
            ResultsGrid.Children.Clear();
            ResultsGrid.Children.Add(scrollViewer);
            ResultsGrid.Visibility = Visibility.Visible;
            ScanButton.IsEnabled = true;
            ScanButton.Content = "🔄 SCAN AGAIN";
        }

        private string GetDeviceIcon(DeviceType deviceType)
        {
            return deviceType switch
            {
                DeviceType.IPCamera => "📹",
                DeviceType.NASDevice => "💾",
                DeviceType.SmartHomeDevice => "🏠",
                DeviceType.OnvifCamera => "📷",
                DeviceType.RTSPCamera => "🎥",
                _ => "🔧"
            };
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LaunchButton_Click(object sender, RoutedEventArgs e)
        {
            // Launch main application
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
