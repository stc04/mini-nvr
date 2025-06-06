using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using AIIT.NVR.Core.Services;
using AIIT.NVR.Core.Models;

namespace AIIT.NVR.Linux
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("AI-IT Inc NVR System for Linux");
            Console.WriteLine("===============================");
            Console.WriteLine($"Platform: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Architecture: {RuntimeInformation.OSArchitecture}");
            
            // Check if running on Raspberry Pi
            bool isRaspberryPi = IsRaspberryPi();
            if (isRaspberryPi)
            {
                Console.WriteLine("Detected: Raspberry Pi");
                Console.WriteLine("Optimizing for ARM architecture...");
            }
            
            try
            {
                var host = CreateHostBuilder(args, isRaspberryPi).Build();
                
                // Initialize services
                await InitializeServicesAsync(host.Services);
                
                Console.WriteLine("Starting AI-IT Inc NVR services...");
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Environment.Exit(1);
            }
        }
        
        static IHostBuilder CreateHostBuilder(string[] args, bool isRaspberryPi) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register core services
                    services.AddSingleton<NASStorage>();
                    services.AddSingleton<VideoRecordingService>();
                    services.AddSingleton<StreamingService>();
                    services.AddSingleton<CameraManager>();
                    services.AddSingleton<NetworkScanService>();
                    services.AddSingleton<SystemMonitoringService>();
                    
                    // Add Linux-specific services
                    services.AddSingleton<LinuxSystemService>();
                    services.AddSingleton<HardwareOptimizationService>();
                    
                    if (isRaspberryPi)
                    {
                        services.AddSingleton<RaspberryPiService>();
                        services.AddSingleton<GPUAccelerationService>();
                    }
                    
                    // Add web hosting
                    services.AddSingleton<IWebHostBuilder>(provider =>
                        new WebHostBuilder()
                            .UseKestrel(options =>
                            {
                                options.ListenAnyIP(8080); // HTTP
                                options.ListenAnyIP(8443); // HTTPS
                            })
                            .UseContentRoot(Directory.GetCurrentDirectory())
                            .UseStartup<WebStartup>()
                    );
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddFile("logs/nvr-{Date}.log");
                });
        
        static bool IsRaspberryPi()
        {
            try
            {
                if (File.Exists("/proc/cpuinfo"))
                {
                    string cpuInfo = File.ReadAllText("/proc/cpuinfo");
                    return cpuInfo.Contains("BCM") || cpuInfo.Contains("Raspberry Pi");
                }
                
                if (File.Exists("/proc/device-tree/model"))
                {
                    string model = File.ReadAllText("/proc/device-tree/model");
                    return model.Contains("Raspberry Pi");
                }
            }
            catch
            {
                // Ignore errors
            }
            
            return false;
        }
        
        static async Task InitializeServicesAsync(IServiceProvider services)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            
            try
            {
                // Initialize system monitoring
                var systemMonitor = services.GetRequiredService<SystemMonitoringService>();
                await systemMonitor.StartAsync();
                
                // Initialize hardware optimization
                var hardwareOptimizer = services.GetRequiredService<HardwareOptimizationService>();
                await hardwareOptimizer.OptimizeSystemAsync();
                
                // Check for GPU acceleration
                if (services.GetService<GPUAccelerationService>() != null)
                {
                    var gpuService = services.GetRequiredService<GPUAccelerationService>();
                    await gpuService.InitializeAsync();
                }
                
                logger.LogInformation("All services initialized successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error initializing services");
                throw;
            }
        }
    }
}
