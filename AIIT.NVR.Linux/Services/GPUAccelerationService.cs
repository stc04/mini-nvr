using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIIT.NVR.Linux.Services
{
    public class GPUAccelerationService
    {
        private readonly ILogger<GPUAccelerationService> _logger;
        private readonly LinuxSystemService _systemService;
        
        public GPUAccelerationService(ILogger<GPUAccelerationService> logger, LinuxSystemService systemService)
        {
            _logger = logger;
            _systemService = systemService;
        }
        
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing GPU acceleration...");
                
                var gpuInfo = await DetectGPUAsync();
                
                if (gpuInfo.IsAvailable)
                {
                    await SetupGPUAccelerationAsync(gpuInfo);
                    _logger.LogInformation($"GPU acceleration initialized: {gpuInfo.Type}");
                }
                else
                {
                    _logger.LogInformation("No GPU acceleration available, using CPU encoding");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing GPU acceleration");
            }
        }
        
        private async Task<GPUInfo> DetectGPUAsync()
        {
            var gpuInfo = new GPUInfo();
            
            try
            {
                // Check for Raspberry Pi VideoCore
                if (await CheckVideoCore())
                {
                    gpuInfo.Type = "VideoCore";
                    gpuInfo.IsAvailable = true;
                    gpuInfo.SupportedCodecs = new[] { "H.264", "MJPEG" };
                    return gpuInfo;
                }
                
                // Check for NVIDIA GPU
                if (await CheckNVIDIA())
                {
                    gpuInfo.Type = "NVIDIA";
                    gpuInfo.IsAvailable = true;
                    gpuInfo.SupportedCodecs = new[] { "H.264", "H.265", "MJPEG" };
                    return gpuInfo;
                }
                
                // Check for Intel GPU (VA-API)
                if (await CheckIntelVAAPI())
                {
                    gpuInfo.Type = "Intel VA-API";
                    gpuInfo.IsAvailable = true;
                    gpuInfo.SupportedCodecs = new[] { "H.264", "H.265", "MJPEG" };
                    return gpuInfo;
                }
                
                // Check for AMD GPU (VA-API)
                if (await CheckAMDVAAPI())
                {
                    gpuInfo.Type = "AMD VA-API";
                    gpuInfo.IsAvailable = true;
                    gpuInfo.SupportedCodecs = new[] { "H.264", "H.265", "MJPEG" };
                    return gpuInfo;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error detecting GPU");
            }
            
            return gpuInfo;
        }
        
        private async Task<bool> CheckVideoCore()
        {
            try
            {
                if (File.Exists("/opt/vc/bin/vcgencmd"))
                {
                    string output = await _systemService.RunCommandAsync("/opt/vc/bin/vcgencmd", "codec_enabled H264");
                    return output.Contains("H264=enabled");
                }
            }
            catch { }
            
            return false;
        }
        
        private async Task<bool> CheckNVIDIA()
        {
            try
            {
                await _systemService.RunCommandAsync("nvidia-smi", "");
                return true;
            }
            catch { }
            
            return false;
        }
        
        private async Task<bool> CheckIntelVAAPI()
        {
            try
            {
                string output = await _systemService.RunCommandAsync("vainfo", "");
                return output.Contains("Intel") && output.Contains("VAEntrypointEncSlice");
            }
            catch { }
            
            return false;
        }
        
        private async Task<bool> CheckAMDVAAPI()
        {
            try
            {
                string output = await _systemService.RunCommandAsync("vainfo", "");
                return output.Contains("AMD") && output.Contains("VAEntrypointEncSlice");
            }
            catch { }
            
            return false;
        }
        
        private async Task SetupGPUAccelerationAsync(GPUInfo gpuInfo)
        {
            try
            {
                switch (gpuInfo.Type)
                {
                    case "VideoCore":
                        await SetupVideoCoreAsync();
                        break;
                    case "NVIDIA":
                        await SetupNVIDIAAsync();
                        break;
                    case "Intel VA-API":
                    case "AMD VA-API":
                        await SetupVAAPIAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error setting up {gpuInfo.Type} acceleration");
            }
        }
        
        private async Task SetupVideoCoreAsync()
        {
            try
            {
                // Set environment variables for VideoCore acceleration
                Environment.SetEnvironmentVariable("AIIT_NVR_GPU_TYPE", "VideoCore");
                Environment.SetEnvironmentVariable("AIIT_NVR_ENCODER", "h264_omx");
                Environment.SetEnvironmentVariable("AIIT_NVR_DECODER", "h264_mmal");
                
                _logger.LogInformation("VideoCore acceleration configured");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting up VideoCore acceleration");
            }
        }
        
        private async Task SetupNVIDIAAsync()
        {
            try
            {
                // Set environment variables for NVIDIA acceleration
                Environment.SetEnvironmentVariable("AIIT_NVR_GPU_TYPE", "NVIDIA");
                Environment.SetEnvironmentVariable("AIIT_NVR_ENCODER", "h264_nvenc");
                Environment.SetEnvironmentVariable("AIIT_NVR_DECODER", "h264_cuvid");
                
                _logger.LogInformation("NVIDIA acceleration configured");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting up NVIDIA acceleration");
            }
        }
        
        private async Task SetupVAAPIAsync()
        {
            try
            {
                // Set environment variables for VA-API acceleration
                Environment.SetEnvironmentVariable("AIIT_NVR_GPU_TYPE", "VAAPI");
                Environment.SetEnvironmentVariable("AIIT_NVR_ENCODER", "h264_vaapi");
                Environment.SetEnvironmentVariable("AIIT_NVR_DECODER", "h264_vaapi");
                
                _logger.LogInformation("VA-API acceleration configured");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting up VA-API acceleration");
            }
        }
        
        public string GetOptimalFFmpegArgs(string inputUrl, string outputPath)
        {
            string gpuType = Environment.GetEnvironmentVariable("AIIT_NVR_GPU_TYPE") ?? "CPU";
            string encoder = Environment.GetEnvironmentVariable("AIIT_NVR_ENCODER") ?? "libx264";
            string decoder = Environment.GetEnvironmentVariable("AIIT_NVR_DECODER") ?? "";
            
            string args = $"-i \"{inputUrl}\"";
            
            // Add decoder if available
            if (!string.IsNullOrEmpty(decoder))
            {
                args = $"-c:v {decoder} {args}";
            }
            
            // Add encoder and optimization settings
            switch (gpuType)
            {
                case "VideoCore":
                    args += $" -c:v {encoder} -preset ultrafast -b:v 2M -maxrate 2M -bufsize 1M";
                    break;
                case "NVIDIA":
                    args += $" -c:v {encoder} -preset fast -b:v 4M -maxrate 4M -bufsize 2M";
                    break;
                case "VAAPI":
                    args += $" -vaapi_device /dev/dri/renderD128 -c:v {encoder} -b:v 3M -maxrate 3M -bufsize 1.5M";
                    break;
                default:
                    args += $" -c:v libx264 -preset ultrafast -crf 28 -maxrate 2M -bufsize 1M";
                    break;
            }
            
            args += $" -c:a aac -f mp4 \"{outputPath}\"";
            
            return args;
        }
    }
    
    public class GPUInfo
    {
        public string Type { get; set; } = "";
        public bool IsAvailable { get; set; }
        public string[] SupportedCodecs { get; set; } = Array.Empty<string>();
        public string Memory { get; set; } = "";
    }
}
