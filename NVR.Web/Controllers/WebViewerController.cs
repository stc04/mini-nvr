using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NVR.Core.Services;
using NVR.Core.Models;
using System.Threading.Tasks;

namespace NVR.Web.Controllers
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
            return Json(_cameraManager.Cameras);
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

        private Camera GetCameraById(int id)
        {
            foreach (var camera in _cameraManager.Cameras)
            {
                if (camera.Id == id) return camera;
            }
            return null;
        }
    }
}
