using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AIIT.NVR.Linux.Services
{
    public class LinuxSystemService
    {
        private readonly ILogger<LinuxSystemService> _logger;
        
        public LinuxSystemService(ILogger<LinuxSystemService> logger)
        {
            _logger = logger;
        }
        
        public async Task<SystemInfo> GetSystemInfoAsync()
        {
            var systemInfo = new SystemInfo();
            
            try
            {
                // Get CPU information
                systemInfo.CpuInfo = await GetCpuInfoAsync();
                
                // Get memory information
                systemInfo.MemoryInfo = await GetMemoryInfoAsync();
                
                // Get disk information
                systemInfo.DiskInfo = await GetDiskInfoAsync();
                
                // Get network information
                systemInfo.NetworkInfo = await GetNetworkInfoAsync();
                
                // Get temperature (if available)
                systemInfo.Temperature = await GetTemperatureAsync();
                
                // Get GPU information
                systemInfo.GpuInfo = await GetGpuInfoAsync();
                
                return systemInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system information");
                return systemInfo;
            }
        }
        
        private async Task<CpuInfo> GetCpuInfoAsync()
        {
            var cpuInfo = new CpuInfo();
            
            try
            {
                if (File.Exists("/proc/cpuinfo"))
                {
                    string content = await File.ReadAllTextAsync("/proc/cpuinfo");
                    var lines = content.Split('\n');
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("model name"))
                        {
                            cpuInfo.ModelName = line.Split(':')[1].Trim();
                        }
                        else if (line.StartsWith("cpu cores"))
                        {
                            int.TryParse(line.Split(':')[1].Trim(), out cpuInfo.Cores);
                        }
                        else if (line.StartsWith("cpu MHz"))
                        {
                            double.TryParse(line.Split(':')[1].Trim(), out cpuInfo.FrequencyMHz);
                        }
                    }
                }
                
                // Get CPU usage
                cpuInfo.Usage = await GetCpuUsageAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting CPU information");
            }
            
            return cpuInfo;
        }
        
        private async Task<double> GetCpuUsageAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "top",
                    Arguments = "-bn1 | grep 'Cpu(s)' | awk '{print $2}' | cut -d'%' -f1",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                if (double.TryParse(output.Trim(), out double usage))
                {
                    return usage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting CPU usage");
            }
            
            return 0;
        }
        
        private async Task<MemoryInfo> GetMemoryInfoAsync()
        {
            var memoryInfo = new MemoryInfo();
            
            try
            {
                if (File.Exists("/proc/meminfo"))
                {
                    string content = await File.ReadAllTextAsync("/proc/meminfo");
                    var lines = content.Split('\n');
                    
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("MemTotal:"))
                        {
                            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 2 && long.TryParse(parts[1], out long total))
                            {
                                memoryInfo.TotalKB = total;
                            }
                        }
                        else if (line.StartsWith("MemAvailable:"))
                        {
                            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length >= 2 && long.TryParse(parts[1], out long available))
                            {
                                memoryInfo.AvailableKB = available;
                            }
                        }
                    }
                    
                    memoryInfo.UsedKB = memoryInfo.TotalKB - memoryInfo.AvailableKB;
                    memoryInfo.UsagePercent = (double)memoryInfo.UsedKB / memoryInfo.TotalKB * 100;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting memory information");
            }
            
            return memoryInfo;
        }
        
        private async Task<DiskInfo> GetDiskInfoAsync()
        {
            var diskInfo = new DiskInfo();
            
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "df",
                    Arguments = "-h /",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                var lines = output.Split('\n');
                if (lines.Length >= 2)
                {
                    var parts = lines[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 5)
                    {
                        diskInfo.TotalSpace = parts[1];
                        diskInfo.UsedSpace = parts[2];
                        diskInfo.AvailableSpace = parts[3];
                        diskInfo.UsagePercent = parts[4];
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting disk information");
            }
            
            return diskInfo;
        }
        
        private async Task<NetworkInfo> GetNetworkInfoAsync()
        {
            var networkInfo = new NetworkInfo();
            
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ip",
                    Arguments = "addr show",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                
                using var process = Process.Start(startInfo);
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                // Parse network interfaces
                networkInfo.Interfaces = ParseNetworkInterfaces(output);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting network information");
            }
            
            return networkInfo;
        }
        
        private async Task<double> GetTemperatureAsync()
        {
            try
            {
                // Try Raspberry Pi temperature first
                if (File.Exists("/sys/class/thermal/thermal_zone0/temp"))
                {
                    string tempStr = await File.ReadAllTextAsync("/sys/class/thermal/thermal_zone0/temp");
                    if (int.TryParse(tempStr.Trim(), out int temp))
                    {
                        return temp / 1000.0; // Convert from millidegrees to degrees
                    }
                }
                
                // Try other temperature sources
                var tempFiles = new[]
                {
                    "/sys/class/hwmon/hwmon0/temp1_input",
                    "/sys/class/hwmon/hwmon1/temp1_input"
                };
                
                foreach (var file in tempFiles)
                {
                    if (File.Exists(file))
                    {
                        string tempStr = await File.ReadAllTextAsync(file);
                        if (int.TryParse(tempStr.Trim(), out int temp))
                        {
                            return temp / 1000.0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting temperature");
            }
            
            return 0;
        }
        
        private async Task<GpuInfo> GetGpuInfoAsync()
        {
            var gpuInfo = new GpuInfo();
            
            try
            {
                // Check for VideoCore GPU (Raspberry Pi)
                if (File.Exists("/opt/vc/bin/vcgencmd"))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "/opt/vc/bin/vcgencmd",
                        Arguments = "get_mem gpu",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    
                    using var process = Process.Start(startInfo);
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    
                    if (output.Contains("gpu="))
                    {
                        gpuInfo.Type = "VideoCore";
                        gpuInfo.Memory = output.Split('=')[1].Trim();
                        gpuInfo.IsAvailable = true;
                    }
                }
                
                // Check for other GPUs
                if (Directory.Exists("/sys/class/drm"))
                {
                    var drmDirs = Directory.GetDirectories("/sys/class/drm");
                    gpuInfo.IsAvailable = drmDirs.Length > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting GPU information");
            }
            
            return gpuInfo;
        }
        
        private List<string> ParseNetworkInterfaces(string output)
        {
            var interfaces = new List<string>();
            var lines = output.Split('\n');
            
            foreach (var line in lines)
            {
                if (line.Contains(": <") && !line.Contains("lo:"))
                {
                    var parts = line.Split(':');
                    if (parts.Length >= 2)
                    {
                        interfaces.Add(parts[1].Trim());
                    }
                }
            }
            
            return interfaces;
        }
        
        public async Task<bool> InstallDependenciesAsync()
        {
            try
            {
                _logger.LogInformation("Installing system dependencies...");
                
                // Update package list
                await RunCommandAsync("apt-get", "update");
                
                // Install required packages
                var packages = new[]
                {
                    "ffmpeg",
                    "v4l-utils",
                    "gstreamer1.0-tools",
                    "gstreamer1.0-plugins-base",
                    "gstreamer1.0-plugins-good",
                    "gstreamer1.0-plugins-bad",
                    "gstreamer1.0-plugins-ugly",
                    "libgstreamer1.0-dev",
                    "libgstreamer-plugins-base1.0-dev"
                };
                
                foreach (var package in packages)
                {
                    await RunCommandAsync("apt-get", $"install -y {package}");
                }
                
                _logger.LogInformation("Dependencies installed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error installing dependencies");
                return false;
            }
        }
        
        private async Task<string> RunCommandAsync(string command, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                throw new Exception($"Command failed: {error}");
            }
            
            return output;
        }
    }
    
    public class SystemInfo
    {
        public CpuInfo CpuInfo { get; set; } = new();
        public MemoryInfo MemoryInfo { get; set; } = new();
        public DiskInfo DiskInfo { get; set; } = new();
        public NetworkInfo NetworkInfo { get; set; } = new();
        public double Temperature { get; set; }
        public GpuInfo GpuInfo { get; set; } = new();
    }
    
    public class CpuInfo
    {
        public string ModelName { get; set; } = "";
        public int Cores { get; set; }
        public double FrequencyMHz { get; set; }
        public double Usage { get; set; }
    }
    
    public class MemoryInfo
    {
        public long TotalKB { get; set; }
        public long UsedKB { get; set; }
        public long AvailableKB { get; set; }
        public double UsagePercent { get; set; }
    }
    
    public class DiskInfo
    {
        public string TotalSpace { get; set; } = "";
        public string UsedSpace { get; set; } = "";
        public string AvailableSpace { get; set; } = "";
        public string UsagePercent { get; set; } = "";
    }
    
    public class NetworkInfo
    {
        public List<string> Interfaces { get; set; } = new();
    }
    
    public class GpuInfo
    {
        public string Type { get; set; } = "";
        public string Memory { get; set; } = "";
        public bool IsAvailable { get; set; }
    }
}
