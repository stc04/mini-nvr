using System;
using System.Windows.Controls;
using NVR.Core.Models;
using NVR.Core.Services;

namespace NVR.WPF
{
    public partial class VideoDisplayControl : UserControl
    {
        private readonly Camera _camera;
        private readonly StreamingService _streamingService;

        public VideoDisplayControl(Camera camera, StreamingService streamingService)
        {
            InitializeComponent();
            _camera = camera;
            _streamingService = streamingService;
            
            DataContext = camera;
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
            }
        }
    }
}
