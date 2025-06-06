using Microsoft.AspNetCore.Mvc;
using AIIT.NVR.Core.Services;

namespace AIIT.NVR.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly CameraManager _cameraManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(CameraManager cameraManager, ILogger<HomeController> logger)
        {
            _cameraManager = cameraManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult App()
        {
            ViewBag.TotalCameras = _cameraManager.Cameras.Count;
            ViewBag.OnlineCameras = _cameraManager.Cameras.Count(c => c.IsOnline);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
