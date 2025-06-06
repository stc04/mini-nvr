using System;
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
                Text = "🎉 Network Scan Complete!",
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
                Text = "✨ Ready to proceed! Click 'Launch NVR' to start with optimized settings based on your network scan.",
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
