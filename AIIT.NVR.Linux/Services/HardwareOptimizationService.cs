using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIIT.NVR.Linux.Services
{
    public class HardwareOptimizationService
    {
        private readonly ILogger<HardwareOptimizationService> _logger;
        private readonly LinuxSystemService _systemService;
        private readonly RaspberryPiService _raspberryPiService;
        
        public HardwareOptimizationService(
            ILogger<HardwareOptimizationService> logger,
            LinuxSystemService systemService,
            RaspberryPiService raspberryPiService = null)
        {
            _logger = logger;
            _systemService = systemService;
            _raspberryPiService = raspberryPiService;
        }
        
        public async Task OptimizeSystemAsync()
        {
            try
            {
                _logger.LogInformation("Starting system optimization...");
                
                var systemInfo = await _systemService.GetSystemInfoAsync();
                
                // Optimize based on available memory
                await OptimizeMemoryUsageAsync(systemInfo.MemoryInfo);
                
                // Optimize CPU settings
                await OptimizeCpuSettingsAsync(systemInfo.CpuInfo);
                
                // Optimize for video processing
                await OptimizeVideoProcessingAsync();
                
                // Raspberry Pi specific optimizations
                if (_raspberryPiService != null)
                {
                    await _raspberryPiService.OptimizeForNVRAsync();
                }
                
                // Set up swap if needed
                await OptimizeSwapAsync(systemInfo.MemoryInfo);
                
                // Optimize network settings
                await OptimizeNetworkAsync();
                
                _logger.LogInformation("System optimization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during system optimization");
            }
        }
        
        private async Task OptimizeMemoryUsageAsync(MemoryInfo memoryInfo)
        {
            try
            {
                _logger.LogInformation($"Optimizing memory usage. Total: {memoryInfo.TotalKB / 1024}MB, Available: {memoryInfo.AvailableKB / 1024}MB");
                
                // If low memory system (< 2GB), optimize aggressively
                if (memoryInfo.TotalKB < 2 * 1024 * 1024)
                {
                    _logger.LogInformation("Low memory system detected, applying aggressive optimizations");
                    
                    // Reduce video buffer sizes
                    Environment.SetEnvironmentVariable("AIIT_NVR_LOW_MEMORY", "true");
                    Environment.SetEnvironmentVariable("AIIT_NVR_MAX_CAMERAS", "8");
                    Environment.SetEnvironmentVariable("AIIT_NVR_BUFFER_SIZE", "1024");
                }
                else if (memoryInfo.TotalKB < 4 * 1024 * 1024)
                {
                    _logger.LogInformation("Medium memory system detected, applying moderate optimizations");
                    
                    Environment.SetEnvironmentVariable("AIIT_NVR_MAX_CAMERAS", "16");
                    Environment.SetEnvironmentVariable("AIIT_NVR_BUFFER_SIZE", "2048");
                }
                else
                {
                    _logger.LogInformation("High memory system detected, using standard settings");
                    
                    Environment.SetEnvironmentVariable("AIIT_NVR_MAX_CAMERAS", "48");
                    Environment.SetEnvironmentVariable("AIIT_NVR_BUFFER_SIZE", "4096");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error optimizing memory usage");
            }
        }
        
        private async Task OptimizeCpuSettingsAsync(CpuInfo cpuInfo)
        {
            try
            {
                _logger.LogInformation($"Optimizing CPU settings. Model: {cpuInfo.ModelName}, Cores: {cpuInfo.Cores}");
                
                // Set CPU governor to performance for better video processing
                if (cpuInfo.Cores > 0)
                {
                    try
                    {
                        await _systemService.RunCommandAsync("cpufreq-set", "-g performance");
                        _logger.LogInformation("Set CPU governor to performance mode");
                    }
                    catch
                    {
                        _logger.LogWarning("Could not set CPU governor (may require root privileges)");
                    }
                }
                
                // Optimize thread count based on CPU cores
                int optimalThreads = Math.Max(1, cpuInfo.Cores - 1); // Leave one core for system
                Environment.SetEnvironmentVariable("AIIT_NVR_WORKER_THREADS", optimalThreads.ToString());
                
                _logger.LogInformation($"Set optimal worker threads to {optimalThreads}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error optimizing CPU settings");
            }
        }
        
        private async Task OptimizeVideoProcessingAsync()
        {
            try
            {
                _logger.LogInformation("Optimizing video processing settings");
                
                // Check for hardware acceleration support
                bool hasHardwareAccel = await CheckHardwareAccelerationAsync();
                
                if (hasHardwareAccel)
                {
                    Environment.SetEnvironmentVariable("AIIT_NVR_HARDWARE_ACCEL", "true");
                    _logger.LogInformation("Hardware acceleration enabled");
                }
                else
                {
                    Environment.SetEnvironmentVariable("AIIT_NVR_HARDWARE_ACCEL", "false");
                    _logger.LogInformation("Using software encoding/decoding");
                }
                
                // Set optimal video settings for resource-constrained systems
                Environment.SetEnvironmentVariable("AIIT_NVR_VIDEO_PRESET", "ultrafast");
                Environment.SetEnvironmentVariable("AIIT_NVR_VIDEO_CRF", "28"); // Higher CRF for smaller files
                Environment.SetEnvironmentVariable("AIIT_NVR_MAX_RESOLUTION", "1080p");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error optimizing video processing");
            }
        }
        
        private async Task<bool> CheckHardwareAccelerationAsync()
        {
            try
            {
                // Check for various hardware acceleration methods
                
                // Check for VideoCore (Raspberry Pi)
                if (File.Exists("/opt/vc/bin/vcgencmd"))
                {
                    return true;
                }
                
                // Check for VA-API
                try
                {
                    await _systemService.RunCommandAsync("vainfo", "");
                    return true;
                }
                catch { }
                
                // Check for VDPAU
                try
                {
                    await _systemService.RunCommandAsync("vdpauinfo", "");
                    return true;
                }
                catch { }
                
                // Check for NVENC (NVIDIA)
                if (Directory.Exists("/proc/driver/nvidia"))
                {
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking hardware acceleration");
                return false;
            }
        }
        
        private async Task OptimizeSwapAsync(MemoryInfo memoryInfo)
        {
            try
            {
                // For systems with less than 2GB RAM, ensure swap is available
                if (memoryInfo.TotalKB < 2 * 1024 * 1024)
                {
                    _logger.LogInformation("Checking swap configuration for low memory system");
                    
                    string swapInfo = await _systemService.RunCommandAsync("swapon", "--show");
                    
                    if (string.IsNullOrWhiteSpace(swapInfo))
                    {
                        _logger.LogInformation("No swap detected, creating swap file");
                        await CreateSwapFileAsync();
                    }
                    else
                    {
                        _logger.LogInformation("Swap already configured");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error optimizing swap");
            }
        }
        
        private async Task CreateSwapFileAsync()
        {
            try
            {
                // Create 1GB swap file
                await _systemService.RunCommandAsync("fallocate", "-l 1G /swapfile");
                await _systemService.RunCommandAsync("chmod", "600 /swapfile");
                await _systemService.RunCommandAsync("mkswap", "/swapfile");
                await _systemService.RunCommandAsync("swapon", "/swapfile");
                
                // Add to fstab for persistence
                string fstabEntry = "/swapfile none swap sw 0 0\n";
                await File.AppendAllTextAsync("/etc/fstab", fstabEntry);
                
                _logger.LogInformation("Created 1GB swap file");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error creating swap file (may require root privileges)");
            }
        }
        
        private async Task OptimizeNetworkAsync()
        {
            try
            {
                _logger.LogInformation("Optimizing network settings");
                
                // Increase network buffer sizes for video streaming
                var networkOptimizations = new Dictionary<string, string>
                {
                    { "net.core.rmem_max", "16777216" },
                    { "net.core.wmem_max", "16777216" },
                    { "net.ipv4.tcp_rmem", "4096 87380 16777216" },
                    { "net.ipv4.tcp_wmem", "4096 65536 16777216" },
                    { "net.core.netdev_max_backlog", "5000" }
                };
                
                foreach (var setting in networkOptimizations)
                {
                    try
                    {
                        await _systemService.RunCommandAsync("sysctl", $"-w {setting.Key}={setting.Value}");
                    }
                    catch
                    {
                        _logger.LogWarning($"Could not set {setting.Key} (may require root privileges)");
                    }
                }
                
                _logger.LogInformation("Network optimization completed");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error optimizing network settings");
            }
        }
    }
}
