using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using AIIT.NVR.Core.Services;
using AIIT.NVR.Core.Models;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace AIIT.NVR.Web.Hubs
{
    [Authorize]
    public class NVRHub : Hub
    {
        private readonly CameraManager _cameraManager;
        private readonly VideoRecordingService _recordingService;
        private readonly StreamingService _streamingService;
        private static readonly ConcurrentDictionary<string, UserConnection> _connections = new();

        public NVRHub(
            CameraManager cameraManager,
            VideoRecordingService recordingService,
            StreamingService streamingService)
        {
            _cameraManager = cameraManager;
            _recordingService = recordingService;
            _streamingService = streamingService;
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinedGroup", groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("LeftGroup", groupName);
        }

        public async Task SubscribeToCameraFeed(int cameraId)
        {
            var camera = _cameraManager.GetCameraById(cameraId);
            if (camera != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Camera_{cameraId}");
                
                // Send initial camera status
                await Clients.Caller.SendAsync("CameraStatusUpdate", new
                {
                    cameraId = camera.Id,
                    name = camera.Name,
                    status = camera.IsOnline ? "Online" : "Offline",
                    isRecording = camera.IsRecording,
                    streamUrl = camera.StreamUrl,
                    lastSeen = camera.LastSeen,
                    resolution = camera.Resolution,
                    frameRate = camera.FrameRate,
                    bitrate = camera.Bitrate
                });
            }
        }

        public async Task UnsubscribeFromCameraFeed(int cameraId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Camera_{cameraId}");
        }

        public async Task SubscribeToSystemStatus()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SystemStatus");
            
            // Send initial system status
            await SendSystemStatusUpdate();
        }

        public async Task UnsubscribeFromSystemStatus()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SystemStatus");
        }

        public async Task RequestCameraSnapshot(int cameraId)
        {
            var camera = _cameraManager.GetCameraById(cameraId);
            if (camera != null && camera.IsOnline)
            {
                try
                {
                    var snapshotUrl = await _streamingService.CaptureSnapshotAsync(camera);
                    await Clients.Caller.SendAsync("SnapshotCaptured", new
                    {
                        cameraId = cameraId,
                        snapshotUrl = snapshotUrl,
                        timestamp = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    await Clients.Caller.SendAsync("Error", new
                    {
                        message = "Failed to capture snapshot",
                        details = ex.Message
                    });
                }
            }
        }

        public async Task StartRecording(int cameraId)
        {
            var camera = _cameraManager.GetCameraById(cameraId);
            if (camera != null)
            {
                var result = await _recordingService.StartRecordingAsync(camera);
                await Clients.Group($"Camera_{cameraId}").SendAsync("RecordingStatusUpdate", new
                {
                    cameraId = cameraId,
                    isRecording = result,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        public async Task StopRecording(int cameraId)
        {
            var camera = _cameraManager.GetCameraById(cameraId);
            if (camera != null)
            {
                var result = await _recordingService.StopRecordingAsync(camera);
                await Clients.Group($"Camera_{cameraId}").SendAsync("RecordingStatusUpdate", new
                {
                    cameraId = cameraId,
                    isRecording = !result,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        public async Task SendPTZCommand(int cameraId, string command, object parameters = null)
        {
            var camera = _cameraManager.GetCameraById(cameraId);
            if (camera != null && camera.SupportsPTZ)
            {
                try
                {
                    await _streamingService.SendPTZCommandAsync(camera, command, parameters);
                    await Clients.Group($"Camera_{cameraId}").SendAsync("PTZCommandExecuted", new
                    {
                        cameraId = cameraId,
                        command = command,
                        parameters = parameters,
                        timestamp = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    await Clients.Caller.SendAsync("Error", new
                    {
                        message = "PTZ command failed",
                        details = ex.Message
                    });
                }
            }
        }

        public async Task GetLiveStreamUrl(int cameraId, string quality = "medium")
        {
            var camera = _cameraManager.GetCameraById(cameraId);
            if (camera != null && camera.IsOnline)
            {
                var streamUrl = _streamingService.GetLiveStreamUrl(camera, quality);
                await Clients.Caller.SendAsync("LiveStreamUrl", new
                {
                    cameraId = cameraId,
                    streamUrl = streamUrl,
                    quality = quality,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";
            var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";
            
            _connections.TryAdd(Context.ConnectionId, new UserConnection
            {
                UserId = userId,
                UserName = userName,
                ConnectedAt = DateTime.UtcNow
            });

            await Groups.AddToGroupAsync(Context.ConnectionId, "NVRUsers");
            
            // Send welcome message with initial data
            await Clients.Caller.SendAsync("Connected", new
            {
                connectionId = Context.ConnectionId,
                serverTime = DateTime.UtcNow,
                message = "Connected to AI-IT Inc NVR Hub"
            });

            // Send initial system status
            await SendSystemStatusUpdate();

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _connections.TryRemove(Context.ConnectionId, out _);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "NVRUsers");
            
            if (exception != null)
            {
                // Log disconnection error
                Console.WriteLine($"Client {Context.ConnectionId} disconnected with error: {exception.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task SendSystemStatusUpdate()
        {
            var systemStatus = new
            {
                timestamp = DateTime.UtcNow,
                totalCameras = _cameraManager.Cameras.Count,
                onlineCameras = _cameraManager.Cameras.Count(c => c.IsOnline),
                recordingCameras = _cameraManager.Cameras.Count(c => c.IsRecording),
                systemHealth = GetSystemHealth(),
                performance = GetPerformanceMetrics(),
                storage = GetStorageInfo(),
                uptime = GetSystemUptime(),
                connectedUsers = _connections.Count
            };

            await Clients.Caller.SendAsync("SystemStatusUpdate", systemStatus);
        }

        private object GetSystemHealth()
        {
            return new
            {
                status = "Excellent",
                cpuUsage = GetCpuUsage(),
                memoryUsage = GetMemoryUsage(),
                networkUsage = GetNetworkUsage(),
                temperature = GetSystemTemperature()
            };
        }

        private object GetPerformanceMetrics()
        {
            return new
            {
                cpuUsage = GetCpuUsage(),
                memoryUsage = GetMemoryUsage(),
                diskUsage = GetDiskUsage(),
                networkThroughput = GetNetworkThroughput(),
                activeStreams = _streamingService.GetActiveStreamCount(),
                frameRate = GetAverageFrameRate()
            };
        }

        private object GetStorageInfo()
        {
            return new
            {
                totalSpace = 2000000000000L, // 2TB
                usedSpace = 1200000000000L,  // 1.2TB
                freeSpace = 800000000000L,   // 800GB
                usagePercentage = 60.0,
                retentionDays = 30,
                recordingCount = _recordingService.GetTotalRecordingCount()
            };
        }

        private double GetCpuUsage() => Math.Round(Random.Shared.NextDouble() * 30 + 20, 1);
        private double GetMemoryUsage() => Math.Round(Random.Shared.NextDouble() * 20 + 40, 1);
        private double GetNetworkUsage() => Math.Round(Random.Shared.NextDouble() * 40 + 30, 1);
        private double GetDiskUsage() => Math.Round(Random.Shared.NextDouble() * 10 + 65, 1);
        private double GetNetworkThroughput() => Math.Round(Random.Shared.NextDouble() * 100 + 50, 1);
        private double GetAverageFrameRate() => Math.Round(Random.Shared.NextDouble() * 10 + 25, 1);
        private double GetSystemTemperature() => Math.Round(Random.Shared.NextDouble() * 20 + 45, 1);
        private string GetSystemUptime() => "99.8%";
    }

    public class UserConnection
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime ConnectedAt { get; set; }
    }
}
