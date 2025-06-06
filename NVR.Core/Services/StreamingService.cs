using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NVR.Core.Models;

namespace NVR.Core.Services
{
    public class StreamingService
    {
        private readonly ConcurrentDictionary<int, StreamSession> _activeSessions;

        public StreamingService()
        {
            _activeSessions = new ConcurrentDictionary<int, StreamSession>();
        }

        public async Task<bool> StartStreamAsync(Camera camera)
        {
            try
            {
                if (_activeSessions.ContainsKey(camera.Id))
                {
                    await StopStreamAsync(camera);
                }

                var session = new StreamSession
                {
                    CameraId = camera.Id,
                    StreamUrl = camera.StreamUrl ?? BuildStreamUrl(camera),
                    IsActive = true,
                    StartTime = DateTime.Now
                };

                _activeSessions.TryAdd(camera.Id, session);
                return true;
            }
            catch (Exception ex)
            {
                // Log error
                return false;
            }
        }

        public async Task<bool> StopStreamAsync(Camera camera)
        {
            if (_activeSessions.TryRemove(camera.Id, out var session))
            {
                session.IsActive = false;
                return true;
            }
            return false;
        }

        public StreamSession GetStreamSession(int cameraId)
        {
            _activeSessions.TryGetValue(cameraId, out var session);
            return session;
        }

        private string BuildStreamUrl(Camera camera)
        {
            return $"rtsp://{camera.Username}:{camera.Password}@{camera.IpAddress}:{camera.Port}/stream";
        }
    }

    public class StreamSession
    {
        public int CameraId { get; set; }
        public string StreamUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartTime { get; set; }
        public int ViewerCount { get; set; }
    }
}
