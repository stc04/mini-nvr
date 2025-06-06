using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace NVR.Core.Services
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RemoteAccessController : ControllerBase
    {
        private readonly CameraManager _cameraManager;
        private readonly VideoRecordingService _recordingService;
        private readonly StreamingService _streamingService;

        public RemoteAccessController(
            CameraManager cameraManager,
            VideoRecordingService recordingService,
            StreamingService streamingService)
        {
            _cameraManager = cameraManager;
            _recordingService = recordingService;
            _streamingService = streamingService;
        }

        [HttpGet("cameras")]
        public IActionResult GetCameras()
        {
            return Ok(_cameraManager.Cameras);
        }

        [HttpPost("cameras/{id}/start-recording")]
        public async Task<IActionResult> StartRecording(int id)
        {
            var camera = GetCameraById(id);
            if (camera == null) return NotFound();

            var result = await _recordingService.StartRecordingAsync(camera);
            return result ? Ok() : BadRequest();
        }

        [HttpPost("cameras/{id}/stop-recording")]
        public async Task<IActionResult> StopRecording(int id)
        {
            var camera = GetCameraById(id);
            if (camera == null) return NotFound();

            var result = await _recordingService.StopRecordingAsync(camera);
            return result ? Ok() : BadRequest();
        }

        [HttpGet("cameras/{id}/stream")]
        public IActionResult GetStream(int id)
        {
            var session = _streamingService.GetStreamSession(id);
            if (session == null) return NotFound();

            return Ok(new { streamUrl = session.StreamUrl });
        }

        [HttpPost("system/arm")]
        public async Task<IActionResult> ArmSystem()
        {
            await _cameraManager.StartAllRecordingAsync();
            return Ok();
        }

        [HttpPost("system/disarm")]
        public async Task<IActionResult> DisarmSystem()
        {
            await _cameraManager.StopAllRecordingAsync();
            return Ok();
        }

        private Models.Camera GetCameraById(int id)
        {
            foreach (var camera in _cameraManager.Cameras)
            {
                if (camera.Id == id) return camera;
            }
            return null;
        }
    }
}
