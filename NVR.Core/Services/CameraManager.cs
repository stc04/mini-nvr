using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NVR.Core.Models;

namespace NVR.Core.Services
{
    public class CameraManager
    {
        private readonly ObservableCollection<Camera> _cameras;
        private readonly VideoRecordingService _recordingService;
        private readonly StreamingService _streamingService;

        public ObservableCollection<Camera> Cameras => _cameras;

        public CameraManager(VideoRecordingService recordingService, StreamingService streamingService)
        {
            _cameras = new ObservableCollection<Camera>();
            _recordingService = recordingService;
            _streamingService = streamingService;
        }

        public async Task<bool> AddCameraAsync(Camera camera)
        {
            try
            {
                // Test connection first
                var isConnected = await TestCameraConnectionAsync(camera);
                if (!isConnected)
                {
                    camera.Status = CameraStatus.Error;
                    return false;
                }

                camera.Id = GetNextCameraId();
                camera.Status = CameraStatus.Online;
                _cameras.Add(camera);

                // Start streaming and recording
                await _streamingService.StartStreamAsync(camera);
                if (camera.IsRecording)
                {
                    await _recordingService.StartRecordingAsync(camera);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log error
                camera.Status = CameraStatus.Error;
                return false;
            }
        }

        public async Task<bool> RemoveCameraAsync(int cameraId)
        {
            var camera = _cameras.FirstOrDefault(c => c.Id == cameraId);
            if (camera == null) return false;

            await _streamingService.StopStreamAsync(camera);
            await _recordingService.StopRecordingAsync(camera);
            _cameras.Remove(camera);
            return true;
        }

        public async Task<bool> TestCameraConnectionAsync(Camera camera)
        {
            try
            {
                // Implement camera connection test based on type
                switch (camera.Type)
                {
                    case CameraType.OnvifCamera:
                        return await TestOnvifConnectionAsync(camera);
                    case CameraType.RTSPCamera:
                        return await TestRTSPConnectionAsync(camera);
                    case CameraType.IPCamera:
                        return await TestIPCameraConnectionAsync(camera);
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> TestOnvifConnectionAsync(Camera camera)
        {
            // Implement ONVIF discovery and connection test
            return await Task.FromResult(true); // Placeholder
        }

        private async Task<bool> TestRTSPConnectionAsync(Camera camera)
        {
            // Test RTSP stream connection
            return await Task.FromResult(true); // Placeholder
        }

        private async Task<bool> TestIPCameraConnectionAsync(Camera camera)
        {
            // Test IP camera HTTP connection
            return await Task.FromResult(true); // Placeholder
        }

        private int GetNextCameraId()
        {
            return _cameras.Count > 0 ? _cameras.Max(c => c.Id) + 1 : 1;
        }

        public async Task StartAllRecordingAsync()
        {
            foreach (var camera in _cameras.Where(c => c.Status == CameraStatus.Online))
            {
                camera.IsRecording = true;
                await _recordingService.StartRecordingAsync(camera);
            }
        }

        public async Task StopAllRecordingAsync()
        {
            foreach (var camera in _cameras)
            {
                camera.IsRecording = false;
                await _recordingService.StopRecordingAsync(camera);
            }
        }
    }
}
