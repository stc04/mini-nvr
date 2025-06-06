using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NVR.Core.Services.SmartHomeIntegration
{
    public class AlexaIntegration
    {
        private readonly CameraManager _cameraManager;
        private readonly VideoRecordingService _recordingService;

        public AlexaIntegration(CameraManager cameraManager, VideoRecordingService recordingService)
        {
            _cameraManager = cameraManager;
            _recordingService = recordingService;
        }

        public async Task<bool> HandleAlexaCommand(string command, Dictionary<string, object> parameters)
        {
            try
            {
                switch (command.ToLower())
                {
                    case "start_recording":
                        return await HandleStartRecording(parameters);
                    case "stop_recording":
                        return await HandleStopRecording(parameters);
                    case "show_camera":
                        return await HandleShowCamera(parameters);
                    case "arm_system":
                        return await HandleArmSystem();
                    case "disarm_system":
                        return await HandleDisarmSystem();
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                // Log error
                return false;
            }
        }

        private async Task<bool> HandleStartRecording(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("camera_name"))
            {
                var cameraName = parameters["camera_name"].ToString();
                var camera = FindCameraByName(cameraName);
                if (camera != null)
                {
                    camera.IsRecording = true;
                    return await _recordingService.StartRecordingAsync(camera);
                }
            }
            else
            {
                // Start all cameras
                await _cameraManager.StartAllRecordingAsync();
                return true;
            }
            return false;
        }

        private async Task<bool> HandleStopRecording(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("camera_name"))
            {
                var cameraName = parameters["camera_name"].ToString();
                var camera = FindCameraByName(cameraName);
                if (camera != null)
                {
                    return await _recordingService.StopRecordingAsync(camera);
                }
            }
            else
            {
                // Stop all cameras
                await _cameraManager.StopAllRecordingAsync();
                return true;
            }
            return false;
        }

        private async Task<bool> HandleShowCamera(Dictionary<string, object> parameters)
        {
            // Implementation for showing camera feed on Alexa devices with screens
            return await Task.FromResult(true);
        }

        private async Task<bool> HandleArmSystem()
        {
            await _cameraManager.StartAllRecordingAsync();
            return true;
        }

        private async Task<bool> HandleDisarmSystem()
        {
            await _cameraManager.StopAllRecordingAsync();
            return true;
        }

        private Models.Camera FindCameraByName(string name)
        {
            foreach (var camera in _cameraManager.Cameras)
            {
                if (camera.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return camera;
                }
            }
            return null;
        }
    }
}
