using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using NVR.Core.Services;
using NVR.Core.Models;
using NVR.WPF.Windows;

namespace NVR.WPF
{
    public partial class MainWindow : Window
    {
        private readonly CameraManager _cameraManager;
        private readonly VideoRecordingService _recordingService;
        private readonly StreamingService _streamingService;
        private readonly NASStorage _nasStorage;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize services
            _nasStorage = new NASStorage();
            _recordingService = new VideoRecordingService(_nasStorage);
            _streamingService = new StreamingService();
            _cameraManager = new CameraManager(_recordingService, _streamingService);
            
            // Bind camera list
            CameraListView.ItemsSource = _cameraManager.Cameras;
            
            // Start status update timer
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateStatus;
            timer.Start();
            
            LoadConfiguration();
        }

        private void AddCamera_Click(object sender, RoutedEventArgs e)
        {
            var addCameraWindow = new AddCameraWindow();
            if (addCameraWindow.ShowDialog() == true)
            {
                var camera = addCameraWindow.Camera;
                Task.Run(async () => await _cameraManager.AddCameraAsync(camera));
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_nasStorage);
            settingsWindow.ShowDialog();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void StartAllRecording_Click(object sender, RoutedEventArgs e)
        {
            await _cameraManager.StartAllRecordingAsync();
            StatusText.Text = "Started recording on all cameras";
        }

        private async void StopAllRecording_Click(object sender, RoutedEventArgs e)
        {
            await _cameraManager.StopAllRecordingAsync();
            StatusText.Text = "Stopped recording on all cameras";
        }

        private void SetLayout_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && int.TryParse(menuItem.Tag.ToString(), out int cameraCount))
            {
                SetVideoGridLayout(cameraCount);
            }
        }

        private void SetVideoGridLayout(int maxCameras)
        {
            int columns, rows;
            
            switch (maxCameras)
            {
                case 1:
                    columns = rows = 1;
                    break;
                case 4:
                    columns = rows = 2;
                    break;
                case 9:
                    columns = rows = 3;
                    break;
                case 16:
                    columns = rows = 4;
                    break;
                case 48:
                    columns = 8;
                    rows = 6;
                    break;
                default:
                    columns = rows = 2;
                    break;
            }
            
            VideoGrid.Columns = columns;
            VideoGrid.Rows = rows;
            
            // Clear existing video controls
            VideoGrid.Children.Clear();
            
            // Add video controls for active cameras
            var activeCameras = _cameraManager.Cameras.Where(c => c.Status == CameraStatus.Online).Take(maxCameras);
            foreach (var camera in activeCameras)
            {
                var videoControl = new VideoDisplayControl(camera, _streamingService);
                VideoGrid.Children.Add(videoControl);
            }
        }

        private void AlexaSettings_Click(object sender, RoutedEventArgs e)
        {
            var alexaWindow = new SmartHomeSettingsWindow("Alexa");
            alexaWindow.ShowDialog();
        }

        private void GoogleHomeSettings_Click(object sender, RoutedEventArgs e)
        {
            var googleWindow = new SmartHomeSettingsWindow("Google Home");
            googleWindow.ShowDialog();
        }

        private void AppleHomeKitSettings_Click(object sender, RoutedEventArgs e)
        {
            var appleWindow = new SmartHomeSettingsWindow("Apple HomeKit");
            appleWindow.ShowDialog();
        }

        private void UpdateStatus(object sender, EventArgs e)
        {
            var connectedCount = _cameraManager.Cameras.Count(c => c.Status == CameraStatus.Online);
            var recordingCount = _cameraManager.Cameras.Count(c => c.IsRecording);
            
            ConnectedCountText.Text = connectedCount.ToString();
            RecordingCountText.Text = recordingCount.ToString();
            
            if (_nasStorage.IsConnected)
            {
                var freeSpaceGB = _nasStorage.FreeSpace / (1024 * 1024 * 1024);
                StorageText.Text = $"{freeSpaceGB:F1} GB Free";
            }
            else
            {
                StorageText.Text = "Disconnected";
            }
        }

        private void LoadConfiguration()
        {
            // Load saved configuration
            // This would typically load from a config file or database
        }

        protected override void OnClosed(EventArgs e)
        {
            _recordingService?.Dispose();
            base.OnClosed(e);
        }
    }
}
