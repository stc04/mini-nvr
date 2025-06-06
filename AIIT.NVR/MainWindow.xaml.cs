using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AIIT.NVR.Core.Models;
using AIIT.NVR.Core.Services;
using AIIT.NVR.Windows;

namespace AIIT.NVR
{
    public partial class MainWindow : Window
    {
        private CameraManager _cameraManager;
        private VideoRecordingService _recordingService;
        private StreamingService _streamingService;
        private NASStorage _nasStorage;
        private ObservableCollection<CameraViewModel> _cameraViewModels;
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize services
            _nasStorage = new NASStorage();
            _recordingService = new VideoRecordingService(_nasStorage);
            _streamingService = new StreamingService();
            _cameraManager = new CameraManager(_recordingService, _streamingService);
            
            // Initialize camera view models
            _cameraViewModels = new ObservableCollection<CameraViewModel>();
            CameraListView.ItemsSource = _cameraViewModels;
            
            // Set up timer for status updates
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateStatus;
            timer.Start();
            
            // Load configuration and cameras
            LoadConfiguration();
            LoadCameras();
            
            // Set initial layout
            SetVideoGridLayout(4); // 2x2 grid
        }
        
        private void LoadConfiguration()
        {
            // Load application configuration
            // This would typically load from a config file or database
            
            // For demo purposes, set up a default NAS storage
            _nasStorage.Name = "Default NAS";
            _nasStorage.NetworkPath = "\\\\192.168.1.100\\recordings";
            _nasStorage.IsConnected = true;
            _nasStorage.FreeSpace = 1024 * 1024 * 1024 * 500; // 500 GB
            _nasStorage.TotalSpace = 1024 * 1024 * 1024 * 1000; // 1 TB
        }
        
        private void LoadCameras()
        {
            // Load saved cameras
            // This would typically load from a config file or database
            
            // For demo purposes, add some sample cameras
            AddSampleCameras();
            
            // Update camera list view
            UpdateCameraList();
        }
        
        private void AddSampleCameras()
        {
            // Add sample cameras for demonstration
            var camera1 = new Camera
            {
                Id = 1,
                Name = "Front Door",
                IpAddress = "192.168.1.101",
                Port = 554,
                Username = "admin",
                Password = "admin123",
                Type = CameraType.OnvifCamera,
                Status = CameraStatus.Online,
                StreamUrl = "rtsp://admin:admin123@192.168.1.101:554/stream"
            };
            
            var camera2 = new Camera
            {
                Id = 2,
                Name = "Backyard",
                IpAddress = "192.168.1.102",
                Port = 554,
                Username = "admin",
                Password = "admin123",
                Type = CameraType.RTSPCamera,
                Status = CameraStatus.Online,
                StreamUrl = "rtsp://admin:admin123@192.168.1.102:554/stream"
            };
            
            var camera3 = new Camera
            {
                Id = 3,
                Name = "Living Room",
                IpAddress = "192.168.1.103",
                Port = 80,
                Username = "admin",
                Password = "admin123",
                Type = CameraType.IPCamera,
                Status = CameraStatus.Online,
                StreamUrl = "http://192.168.1.103/video.cgi"
            };
            
            var camera4 = new Camera
            {
                Id = 4,
                Name = "Garage",
                IpAddress = "192.168.1.104",
                Port = 554,
                Username = "admin",
                Password = "admin123",
                Type = CameraType.SmartHomeCamera,
                Status = CameraStatus.Offline,
                StreamUrl = "rtsp://admin:admin123@192.168.1.104:554/stream"
            };
            
            _cameraManager.AddCameraAsync(camera1);
            _cameraManager.AddCameraAsync(camera2);
            _cameraManager.AddCameraAsync(camera3);
            _cameraManager.AddCameraAsync(camera4);
        }
        
        private void UpdateCameraList()
        {
            _cameraViewModels.Clear();
            
            foreach (var camera in _cameraManager.Cameras)
            {
                _cameraViewModels.Add(new CameraViewModel(camera));
            }
        }
        
        private void UpdateStatus(object sender, EventArgs e)
        {
            // Update status bar information
            int onlineCount = 0;
            int recordingCount = 0;
            
            foreach (var camera in _cameraManager.Cameras)
            {
                if (camera.Status == CameraStatus.Online)
                {
                    onlineCount++;
                }
                
                if (camera.IsRecording)
                {
                    recordingCount++;
                }
            }
            
            CameraCountText.Text = $"{onlineCount}/{_cameraManager.Cameras.Count}";
            RecordingCountText.Text = recordingCount.ToString();
            
            if (_nasStorage.IsConnected)
            {
                double freeGB = _nasStorage.FreeSpace / (1024.0 * 1024 * 1024);
                double totalGB = _nasStorage.TotalSpace / (1024.0 * 1024 * 1024);
                StorageText.Text = $"{freeGB:F1} GB free of {totalGB:F1} GB";
            }
            else
            {
                StorageText.Text = "Not connected";
            }
        }
        
        private void SetVideoGridLayout(int maxCameras)
        {
            VideoGrid.Children.Clear();
            
            switch (maxCameras)
            {
                case 1:
                    VideoGrid.Columns = 1;
                    VideoGrid.Rows = 1;
                    break;
                case 4:
                    VideoGrid.Columns = 2;
                    VideoGrid.Rows = 2;
                    break;
                case 9:
                    VideoGrid.Columns = 3;
                    VideoGrid.Rows = 3;
                    break;
                case 16:
                    VideoGrid.Columns = 4;
                    VideoGrid.Rows = 4;
                    break;
                case 48:
                    VideoGrid.Columns = 8;
                    VideoGrid.Rows = 6;
                    break;
                default:
                    VideoGrid.Columns = 2;
                    VideoGrid.Rows = 2;
                    break;
            }
            
            // Add video controls for active cameras
            var activeCameras = _cameraManager.Cameras;
            int count = 0;
            
            foreach (var camera in activeCameras)
            {
                if (count >= maxCameras)
                {
                    break;
                }
                
                var videoControl = new VideoDisplayControl(camera, _streamingService);
                VideoGrid.Children.Add(videoControl);
                count++;
            }
            
            // Fill remaining slots with empty placeholders
            for (int i = count; i < maxCameras; i++)
            {
                var placeholder = new EmptyVideoPlaceholder();
                VideoGrid.Children.Add(placeholder);
            }
        }
        
        #region Event Handlers
        
        private void AddCamera_Click(object sender, RoutedEventArgs e)
        {
            var addCameraWindow = new AddCameraWindow();
            if (addCameraWindow.ShowDialog() == true)
            {
                var camera = addCameraWindow.Camera;
                _cameraManager.AddCameraAsync(camera);
                UpdateCameraList();
                StatusText.Text = $"Added camera: {camera.Name}";
            }
        }
        
        private void ConnectNAS_Click(object sender, RoutedEventArgs e)
        {
            var nasSettingsWindow = new NASSettingsWindow(_nasStorage);
            if (nasSettingsWindow.ShowDialog() == true)
            {
                StatusText.Text = "NAS settings updated";
            }
        }
        
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
        
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        private void SetLayout_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && int.TryParse(menuItem.Tag.ToString(), out int cameraCount))
            {
                SetVideoGridLayout(cameraCount);
                
                // Update combo box selection
                switch (cameraCount)
                {
                    case 1:
                        LayoutComboBox.SelectedIndex = 0;
                        break;
                    case 4:
                        LayoutComboBox.SelectedIndex = 1;
                        break;
                    case 9:
                        LayoutComboBox.SelectedIndex = 2;
                        break;
                    case 16:
                        LayoutComboBox.SelectedIndex = 3;
                        break;
                    case 48:
                        LayoutComboBox.SelectedIndex = 4;
                        break;
                }
            }
        }
        
        private void FullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (WindowStyle == WindowStyle.None)
            {
                // Exit full screen
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                // Enter full screen
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }
        
        private void StartAllRecording_Click(object sender, RoutedEventArgs e)
        {
            _cameraManager.StartAllRecordingAsync();
            StatusText.Text = "Started recording on all cameras";
        }
        
        private void StopAllRecording_Click(object sender, RoutedEventArgs e)
        {
            _cameraManager.StopAllRecordingAsync();
            StatusText.Text = "Stopped recording on all cameras";
        }
        
        private void RecordingSettings_Click(object sender, RoutedEventArgs e)
        {
            var recordingSettingsWindow = new RecordingSettingsWindow();
            recordingSettingsWindow.ShowDialog();
        }
        
        private void AlexaSettings_Click(object sender, RoutedEventArgs e)
        {
            var alexaSettingsWindow = new SmartHomeSettingsWindow("Alexa");
            alexaSettingsWindow.ShowDialog();
        }
        
        private void GoogleHomeSettings_Click(object sender, RoutedEventArgs e)
        {
            var googleHomeSettingsWindow = new SmartHomeSettingsWindow("Google Home");
            googleHomeSettingsWindow.ShowDialog();
        }
        
        private void AppleHomeSettings_Click(object sender, RoutedEventArgs e)
        {
            var appleHomeSettingsWindow = new SmartHomeSettingsWindow("Apple HomeKit");
            appleHomeSettingsWindow.ShowDialog();
        }
        
        private void NetworkScan_Click(object sender, RoutedEventArgs e)
        {
            var networkScanWindow = new NetworkScanWindow();
            networkScanWindow.ShowDialog();
        }
        
        private void SystemDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            var diagnosticsWindow = new SystemDiagnosticsWindow();
            diagnosticsWindow.ShowDialog();
        }
        
        private void WebAccess_Click(object sender, RoutedEventArgs e)
        {
            var webAccessWindow = new WebAccessWindow();
            webAccessWindow.ShowDialog();
        }
        
        private void UserManual_Click(object sender, RoutedEventArgs e)
        {
            // Open user manual (PDF or web page)
            System.Diagnostics.Process.Start("explorer.exe", "https://aiit-inc.com/nvr/manual");
        }
        
        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }
        
        private void CameraListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CameraListView.SelectedItem is CameraViewModel viewModel)
            {
                // Show selected camera in single view
                SetVideoGridLayout(1);
                LayoutComboBox.SelectedIndex = 0;
                
                // Find camera by ID
                var camera = _cameraManager.GetCameraById(viewModel.Id);
                if (camera != null)
                {
                    VideoGrid.Children.Clear();
                    var videoControl = new VideoDisplayControl(camera, _streamingService);
                    VideoGrid.Children.Add(videoControl);
                }
            }
        }
        
        private void LayoutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LayoutComboBox.SelectedIndex >= 0)
            {
                int cameraCount;
                switch (LayoutComboBox.SelectedIndex)
                {
                    case 0:
                        cameraCount = 1;
                        break;
                    case 1:
                        cameraCount = 4;
                        break;
                    case 2:
                        cameraCount = 9;
                        break;
                    case 3:
                        cameraCount = 16;
                        break;
                    case 4:
                        cameraCount = 48;
                        break;
                    default:
                        cameraCount = 4;
                        break;
                }
                
                SetVideoGridLayout(cameraCount);
            }
        }
        
        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {
            if (CameraListView.SelectedItem is CameraViewModel viewModel)
            {
                var camera = _cameraManager.GetCameraById(viewModel.Id);
                if (camera != null)
                {
                    _recordingService.StartRecordingAsync(camera);
                    StatusText.Text = $"Started recording: {camera.Name}";
                    UpdateCameraList();
                }
            }
            else
            {
                StatusText.Text = "No camera selected";
            }
        }
        
        private void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            if (CameraListView.SelectedItem is CameraViewModel viewModel)
            {
                var camera = _cameraManager.GetCameraById(viewModel.Id);
                if (camera != null)
                {
                    _recordingService.StopRecordingAsync(camera);
                    StatusText.Text = $"Stopped recording: {camera.Name}";
                    UpdateCameraList();
                }
            }
            else
            {
                StatusText.Text = "No camera selected";
            }
        }
        
        #endregion
    }
    
    public class CameraViewModel
    {
        private Camera _camera;
        
        public CameraViewModel(Camera camera)
        {
            _camera = camera;
        }
        
        public int Id => _camera.Id;
        public string Name => _camera.Name;
        public string IpAddress => _camera.IpAddress;
        public string StatusColor => _camera.Status == CameraStatus.Online ? "#4CAF50" : "#F44336";
        public Visibility RecordingVisibility => _camera.IsRecording ? Visibility.Visible : Visibility.Collapsed;
    }
}
