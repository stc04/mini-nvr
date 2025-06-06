using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIIT.NVR.Linux.Services
{
    public class RaspberryPiService
    {
        private readonly ILogger<RaspberryPiService> _logger;
        private readonly LinuxSystemService _systemService;
        
        public RaspberryPiService(ILogger<RaspberryPiService> logger, LinuxSystemService systemService)
        {
            _logger = logger;
            _systemService = systemService;
        }
        
        public async Task<RaspberryPiInfo> GetRaspberryPiInfoAsync()
        {
            var info = new RaspberryPiInfo();
            
            try
            {
                // Get Pi model
                info.Model = await GetPiModelAsync();
                
                // Get revision
                info.Revision = await GetPiRevisionAsync();
                
                // Get serial number
                info.SerialNumber = await GetPiSerialAsync();
                
                // Get GPU memory split
                info.GpuMemory = await GetGpuMemoryAsync();
                
                // Get camera status
                info.CameraEnabled = await IsCameraEnabledAsync();
                
                // Get overclock settings
                info.OverclockSettings = await GetOverclockSettingsAsync();
                
                // Get temperature and throttling status
                info.Temperature = await GetTemperatureAsync();
                info.IsThrottled = await IsThrottledAsync();
                
                return info;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Raspberry Pi information");
                return info;
            }
        }
        
        private async Task<string> GetPiModelAsync()
        {
            try
            {
                if (File.Exists("/proc/device-tree/model"))
                {
                    return (await File.ReadAllTextAsync("/proc/device-tree/model")).Trim('\0');
                }
                
                if (File.Exists("/proc/cpuinfo"))
                {
                    string content = await File.ReadAllTextAsync("/proc/cpuinfo");
                    var lines = content.Split('\n');
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Model"))
                        {
                            return line.Split(':')[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting Pi model");
            }
            
            return "Unknown";
        }
        
        private async Task<string> GetPiRevisionAsync()
        {
            try
            {
                if (File.Exists("/proc/cpuinfo"))
                {
                    string content = await File.ReadAllTextAsync("/proc/cpuinfo");
                    var lines = content.Split('\n');
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Revision"))
                        {
                            return line.Split(':')[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting Pi revision");
            }
            
            return "Unknown";
        }
        
        private async Task<string> GetPiSerialAsync()
        {
            try
            {
                if (File.Exists("/proc/cpuinfo"))
                {
                    string content = await File.ReadAllTextAsync("/proc/cpuinfo");
                    var lines = content.Split('\n');
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Serial"))
                        {
                            return line.Split(':')[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting Pi serial");
            }
            
            return "Unknown";
        }
        
        private async Task<int> GetGpuMemoryAsync()
        {
            try
            {
                string output = await _systemService.RunCommandAsync("/opt/vc/bin/vcgencmd", "get_mem gpu");
                if (output.Contains("gpu="))
                {
                    string memStr = output.Split('=')[1].Replace("M", "").Trim();
                    if (int.TryParse(memStr, out int memory))
                    {
                        return memory;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting GPU memory");
            }
            
            return 0;
        }
        
        private async Task<bool> IsCameraEnabledAsync()
        {
            try
            {
                // Check if camera is enabled in config
                if (File.Exists("/boot/config.txt"))
                {
                    string config = await File.ReadAllTextAsync("/boot/config.txt");
                    return config.Contains("start_x=1") || config.Contains("camera_auto_detect=1");
                }
                
                // Check for camera devices
                return Directory.Exists("/dev") && 
                       (File.Exists("/dev/video0") || Directory.GetFiles("/dev", "video*").Length > 0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking camera status");
            }
            
            return false;
        }
        
        private async Task<OverclockSettings> GetOverclockSettingsAsync()
        {
            var settings = new OverclockSettings();
            
            try
            {
                // Get current frequencies
                string armFreq = await _systemService.RunCommandAsync("/opt/vc/bin/vcgencmd", "get_config arm_freq");
                string coreFreq = await _systemService.RunCommandAsync("/opt/vc/bin/vcgencmd", "get_config core_freq");
                string sdramFreq = await _systemService.RunCommandAsync("/opt/vc/bin/vcgencmd", "get_config sdram_freq");
                
                settings.ArmFrequency = ParseFrequency(armFreq);
                settings.CoreFrequency = ParseFrequency(coreFreq);
                settings.SdramFrequency = ParseFrequency(sdramFreq);
                
                // Get voltage
                string voltage = await _systemService.RunCommandAsync("/opt/vc/bin/vcgencmd", "measure_volts core");
                settings.CoreVoltage = ParseVoltage(voltage);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting overclock settings");
            }
            
            return settings;
        }
        
        private async Task<double> GetTemperatureAsync()
        {
            try
            {
                string output = await _systemService.RunCommandAsync("/opt/vc/bin/vcgencmd", "measure_temp");
                if (output.Contains("temp="))
                {
                    string tempStr = output.Split('=')[1].Replace("'C", "").Trim();
                    if (double.TryParse(tempStr, out double temp))
                    {
                        return temp;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting temperature");
            }
            
            return 0;
        }
        
        private async Task<bool> IsThrottledAsync()
        {
            try
            {
                string output = await _systemService.RunCommandAsync("/opt/vc/bin/vcgencmd", "get_throttled");
                if (output.Contains("throttled="))
                {
                    string throttledStr = output.Split('=')[1].Trim();
                    return throttledStr != "0x0";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking throttle status");
            }
            
            return false;
        }
        
        private int ParseFrequency(string input)
        {
            try
            {
                if (input.Contains("="))
                {
                    string freqStr = input.Split('=')[1].Trim();
                    if (int.TryParse(freqStr, out int freq))
                    {
                        return freq;
                    }
                }
            }
            catch { }
            
            return 0;
        }
        
        private double ParseVoltage(string input)
        {
            try
            {
                if (input.Contains("="))
                {
                    string voltStr = input.Split('=')[1].Replace("V", "").Trim();
                    if (double.TryParse(voltStr, out double volt))
                    {
                        return volt;
                    }
                }
            }
            catch { }
            
            return 0;
        }
        
        public async Task<bool> OptimizeForNVRAsync()
        {
            try
            {
                _logger.LogInformation("Optimizing Raspberry Pi for NVR usage...");
                
                // Increase GPU memory split for video processing
                await SetGpuMemoryAsync(128);
                
                // Enable camera if not already enabled
                await EnableCameraAsync();
                
                // Optimize video codecs
                await OptimizeVideoCodecsAsync();
                
                // Set up hardware acceleration
                await SetupHardwareAccelerationAsync();
                
                _logger.LogInformation("Raspberry Pi optimization completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing Raspberry Pi");
                return false;
            }
        }
        
        private async Task SetGpuMemoryAsync(int memoryMB)
        {
            try
            {
                string configPath = "/boot/config.txt";
                if (File.Exists(configPath))
                {
                    string config = await File.ReadAllTextAsync(configPath);
                    
                    // Update or add gpu_mem setting
                    if (config.Contains("gpu_mem="))
                    {
                        config = System.Text.RegularExpressions.Regex.Replace(
                            config, @"gpu_mem=\d+", $"gpu_mem={memoryMB}");
                    }
                    else
                    {
                        config += $"\ngpu_mem={memoryMB}\n";
                    }
                    
                    await File.WriteAllTextAsync(configPath, config);
                    _logger.LogInformation($"Set GPU memory to {memoryMB}MB");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting GPU memory");
            }
        }
        
        private async Task EnableCameraAsync()
        {
            try
            {
                string configPath = "/boot/config.txt";
                if (File.Exists(configPath))
                {
                    string config = await File.ReadAllTextAsync(configPath);
                    
                    if (!config.Contains("start_x=1"))
                    {
                        config += "\nstart_x=1\n";
                        await File.WriteAllTextAsync(configPath, config);
                        _logger.LogInformation("Enabled camera interface");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error enabling camera");
            }
        }
        
        private async Task OptimizeVideoCodecsAsync()
        {
            try
            {
                // Install hardware-accelerated codecs
                await _systemService.RunCommandAsync("apt-get", "install -y libraspberrypi0 libraspberrypi-dev");
                
                _logger.LogInformation("Optimized video codecs");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error optimizing video codecs");
            }
        }
        
        private async Task SetupHardwareAccelerationAsync()
        {
            try
            {
                // Enable hardware acceleration for video encoding/decoding
                string configPath = "/boot/config.txt";
                if (File.Exists(configPath))
                {
                    string config = await File.ReadAllTextAsync(configPath);
                    
                    var settings = new[]
                    {
                        "dtparam=audio=on",
                        "gpu_mem=128",
                        "start_x=1",
                        "disable_camera_led=1"
                    };
                    
                    foreach (var setting in settings)
                    {
                        if (!config.Contains(setting))
                        {
                            config += $"\n{setting}\n";
                        }
                    }
                    
                    await File.WriteAllTextAsync(configPath, config);
                }
                
                _logger.LogInformation("Setup hardware acceleration");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error setting up hardware acceleration");
            }
        }
    }
    
    public class RaspberryPiInfo
    {
        public string Model { get; set; } = "";
        public string Revision { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public int GpuMemory { get; set; }
        public bool CameraEnabled { get; set; }
        public OverclockSettings OverclockSettings { get; set; } = new();
        public double Temperature { get; set; }
        public bool IsThrottled { get; set; }
    }
    
    public class OverclockSettings
    {
        public int ArmFrequency { get; set; }
        public int CoreFrequency { get; set; }
        public int SdramFrequency { get; set; }
        public double CoreVoltage { get; set; }
    }
}
