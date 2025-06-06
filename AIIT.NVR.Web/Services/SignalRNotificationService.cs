using Microsoft.AspNetCore.SignalR;
using AIIT.NVR.Web.Hubs;
using AIIT.NVR.Core.Models;
using System.Threading.Tasks;

namespace AIIT.NVR.Web.Services
{
    public interface ISignalRNotificationService
    {
        Task NotifyCameraStatusChanged(Camera camera);
        Task NotifyMotionDetected(int cameraId, MotionEvent motionEvent);
        Task NotifyRecordingStatusChanged(int cameraId, bool isRecording);
        Task NotifySystemEvent(string eventType, string message, string severity = "info");
        Task NotifySystemStatusUpdate();
        Task NotifyStorageAlert(string message, double usagePercentage);
        Task SendPerformanceUpdate();
    }

    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<NVRHub> _hubContext;

        public SignalRNotificationService(IHubContext<NVRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyCameraStatusChanged(Camera camera)
        {
            await _hubContext.Clients.Group($"Camera_{camera.Id}").SendAsync("CameraStatusUpdate", new
            {
                cameraId = camera.Id,
                name = camera.Name,
                status = camera.IsOnline ? "Online" : "Offline",
                isRecording = camera.IsRecording,
                streamUrl = camera.StreamUrl,
                lastSeen = camera.LastSeen,
                resolution = camera.Resolution,
                frameRate = camera.FrameRate,
                timestamp = DateTime.UtcNow
            });

            // Also notify system status subscribers
            await NotifySystemStatusUpdate();
        }

        public async Task NotifyMotionDetected(int cameraId, MotionEvent motionEvent)
        {
            await _hubContext.Clients.All.SendAsync("MotionDetected", new
            {
                cameraId = cameraId,
                eventId = motionEvent.Id,
                confidence = motionEvent.Confidence,
                detectionZone = motionEvent.DetectionZone,
                timestamp = motionEvent.Timestamp,
                thumbnailUrl = motionEvent.ThumbnailUrl,
                videoUrl = motionEvent.VideoUrl
            });
        }

        public async Task NotifyRecordingStatusChanged(int cameraId, bool isRecording)
        {
            await _hubContext.Clients.Group($"Camera_{cameraId}").SendAsync("RecordingStatusUpdate", new
            {
                cameraId = cameraId,
                isRecording = isRecording,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifySystemEvent(string eventType, string message, string severity = "info")
        {
            await _hubContext.Clients.Group("SystemStatus").SendAsync("SystemEvent", new
            {
                type = eventType,
                message = message,
                severity = severity,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifySystemStatusUpdate()
        {
            var systemStatus = new
            {
                timestamp = DateTime.UtcNow,
                // Add system status data here
            };

            await _hubContext.Clients.Group("SystemStatus").SendAsync("SystemStatusUpdate", systemStatus);
        }

        public async Task NotifyStorageAlert(string message, double usagePercentage)
        {
            await _hubContext.Clients.All.SendAsync("StorageAlert", new
            {
                message = message,
                usagePercentage = usagePercentage,
                severity = usagePercentage > 90 ? "danger" : usagePercentage > 80 ? "warning" : "info",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendPerformanceUpdate()
        {
            var performance = new
            {
                cpuUsage = GetCpuUsage(),
                memoryUsage = GetMemoryUsage(),
                networkUsage = GetNetworkUsage(),
                diskUsage = GetDiskUsage(),
                timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("SystemStatus").SendAsync("PerformanceUpdate", performance);
        }

        private double GetCpuUsage() => Math.Round(Random.Shared.NextDouble() * 30 + 20, 1);
        private double GetMemoryUsage() => Math.Round(Random.Shared.NextDouble() * 20 + 40, 1);
        private double GetNetworkUsage() => Math.Round(Random.Shared.NextDouble() * 40 + 30, 1);
        private double GetDiskUsage() => Math.Round(Random.Shared.NextDouble() * 10 + 65, 1);
    }
}
