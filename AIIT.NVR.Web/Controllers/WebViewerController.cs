using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AIIT.NVR.Core.Services;
using AIIT.NVR.Core.Models;
using System.Threading.Tasks;
using System.Linq;

namespace AIIT.NVR.Web.Controllers
{
    public class WebViewerController : Controller
    {
        private readonly CameraManager _cameraManager;
        private readonly VideoRecordingService _recordingService;
        private readonly StreamingService _streamingService;

        public WebViewerController(
            CameraManager cameraManager,
            VideoRecordingService recordingService,
            StreamingService streamingService)
        {
            _cameraManager = cameraManager;
            _recordingService = recordingService;
            _streamingService = streamingService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SplashPage()
        {
            return View();
        }

        public IActionResult Live()
        {
            ViewBag.Cameras = _cameraManager.Cameras;
            return View();
        }

        public IActionResult Playback()
        {
            ViewBag.Cameras = _cameraManager.Cameras;
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetCameras()
        {
            var cameras = _cameraManager.Cameras.Select(c => new
            {
                id = c.Id,
                name = c.Name,
                location = c.Location,
                status = c.IsOnline ? "Online" : "Offline",
                isRecording = c.IsRecording,
                streamUrl = c.StreamUrl,
                lastSeen = c.LastSeen,
                resolution = c.Resolution,
                frameRate = c.FrameRate
            });

            return Json(cameras);
        }

        [HttpGet]
        public IActionResult GetSystemStatus()
        {
            var status = new
            {
                totalCameras = _cameraManager.Cameras.Count,
                onlineCameras = _cameraManager.Cameras.Count(c => c.IsOnline),
                recordingCameras = _cameraManager.Cameras.Count(c => c.IsRecording),
                systemHealth = "Excellent",
                cpuUsage = 25.5,
                memoryUsage = 45.2,
                networkUsage = 60.8,
                storageUsed = 65.3,
                uptime = "99.8%"
            };

            return Json(status);
        }

        [HttpPost]
        public async Task<IActionResult> StartRecording(int cameraId)
        {
            var camera = GetCameraById(cameraId);
            if (camera == null) return NotFound();

            var result = await _recordingService.StartRecordingAsync(camera);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> StopRecording(int cameraId)
        {
            var camera = GetCameraById(cameraId);
            if (camera == null) return NotFound();

            var result = await _recordingService.StopRecordingAsync(camera);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> StartAllRecording()
        {
            var results = new List<bool>();
            foreach (var camera in _cameraManager.Cameras.Where(c => c.IsOnline && !c.IsRecording))
            {
                var result = await _recordingService.StartRecordingAsync(camera);
                results.Add(result);
            }

            return Json(new { success = results.All(r => r), count = results.Count(r => r) });
        }

        [HttpPost]
        public async Task<IActionResult> StopAllRecording()
        {
            var results = new List<bool>();
            foreach (var camera in _cameraManager.Cameras.Where(c => c.IsRecording))
            {
                var result = await _recordingService.StopRecordingAsync(camera);
                results.Add(result);
            }

            return Json(new { success = results.All(r => r), count = results.Count(r => r) });
        }

        [HttpGet]
        public IActionResult GetRecentEvents()
        {
            var events = new[]
            {
                new { type = "Motion Detected", camera = "Camera 1 - Front Door", time = "2 min ago", severity = "info" },
                new { type = "Recording Started", camera = "Camera 3 - Parking Lot", time = "5 min ago", severity = "success" },
                new { type = "Camera Online", camera = "Camera 2 - Back Yard", time = "10 min ago", severity = "success" },
                new { type = "Motion Detected", camera = "Camera 4 - Side Gate", time = "15 min ago", severity = "info" },
                new { type = "System Armed", camera = "Security System", time = "1 hour ago", severity = "warning" }
            };

            return Json(events);
        }

        [HttpGet]
        public IActionResult GetStorageInfo()
        {
            var storage = new
            {
                totalSpace = 2000000000000L, // 2TB in bytes
                usedSpace = 1200000000000L,  // 1.2TB in bytes
                freeSpace = 800000000000L,   // 800GB in bytes
                retentionDays = 30,
                recordingCount = 15642
            };

            return Json(storage);
        }

        [HttpPost]
        public IActionResult ArmSystem()
        {
            // Implementation for arming the security system
            return Json(new { success = true, message = "System armed successfully" });
        }

        [HttpPost]
        public IActionResult DisarmSystem()
        {
            // Implementation for disarming the security system
            return Json(new { success = true, message = "System disarmed successfully" });
        }

        private Camera GetCameraById(int id)
        {
            return _cameraManager.GetCameraById(id);
        }
    }
}
