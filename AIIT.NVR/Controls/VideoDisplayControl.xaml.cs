using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AIIT.NVR.Core.Models;
using AIIT.NVR.Core.Services;

namespace AIIT.NVR
{
    public partial class VideoDisplayControl : UserControl
    {
        private Camera _camera;
        private StreamingService _streamingService;
        
        public VideoDisplayControl(Camera camera, StreamingService streamingService)
        {
            InitializeComponent();
            _camera = camera;
            _streamingService = streamingService;
            
            DataContext = new VideoDisplayViewModel(camera);
            InitializeVideoPlayer();
        }
        
        private void InitializeVideoPlayer()
        {
            // Initialize video player with camera stream
            var session = _streamingService.GetStreamSession(_camera.Id);
            if (session != null)
            {
                // Set up video player with stream URL
                // This would integrate with a video player component like VLC.NET or similar
                // For demo purposes, we're just showing a placeholder
            }
        }
        
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Open camera settings dialog
            var cameraSettingsWindow = new CameraSettingsWindow(_camera);
            cameraSettingsWindow.ShowDialog();
        }
        
        private void Record_Click(object sender, RoutedEventArgs e)
        {
            // Toggle recording for this camera
            if (_camera.IsRecording)
            {
                // Stop recording
                App.Current.Services.GetService<VideoRecordingService>().StopRecordingAsync(_camera);
            }
            else
            {
                // Start recording
                App.Current.Services.GetService<VideoRecordingService>().StartRecordingAsync(_camera);
            }
            
            // Update view model
            DataContext = new VideoDisplayViewModel(_camera);
        }
    }
    
    public class VideoDisplayViewModel
    {
        private Camera _camera;
        
        public VideoDisplayViewModel(Camera camera)
        {
            _camera = camera;
        }
        
        public string Name => _camera.Name;
        public string Status => _camera.Status.ToString();
        public string StatusColor => _camera.Status == CameraStatus.Online ? "#4CAF50" : "#F44336";
        public Visibility RecordingVisibility => _camera.IsRecording ? Visibility.Visible : Visibility.Collapsed;
    }
}
