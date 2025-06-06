using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using AIIT.NVR.Core.Models;
using System.Net.Http;

namespace AIIT.NVR.Core.Services
{
    public class NetworkScanService
    {
        private List<DeviceDiscoveryResult> _scanResults;
        
        public NetworkScanService()
        {
            _scanResults = new List<DeviceDiscoveryResult>();
        }
        
        public async Task ScanNetworkAsync()
        {
            _scanResults.Clear();
            
            try
            {
                // Get local IP address to determine network range
                string localIp = GetLocalIPAddress();
                if (string.IsNullOrEmpty(localIp))
                {
                    return;
                }
                
                // Parse IP to get network prefix
                string[] ipParts = localIp.Split('.');
                string networkPrefix = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}";
                
                // Scan common ports for cameras and NAS devices
                List<int> portsToScan = new List<int> { 80, 443, 554, 8000, 8080, 9000, 2020 };
                
                // Scan range of IP addresses
                List<Task> scanTasks = new List<Task>();
                for (int i = 1; i < 255; i++)
                {
                    string ipToScan = $"{networkPrefix}.{i}";
                    scanTasks.Add(ScanIPAsync(ipToScan, portsToScan));
                }
                
                await Task.WhenAll(scanTasks);
                
                // Scan for ONVIF devices
                await ScanForOnvifDevicesAsync();
                
                // Scan for smart home devices
                await ScanForSmartHomeDevicesAsync();
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error scanning network: {ex.Message}");
            }
        }
        
        private async Task ScanIPAsync(string ip, List<int> ports)
        {
            // First check if host is reachable with ping
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ip, 100);
                    if (reply.Status != IPStatus.Success)
                    {
                        return;
                    }
                }
                
                // If ping successful, try connecting to ports
                foreach (int port in ports)
                {
                    try
                    {
                        using (TcpClient client = new TcpClient())
                        {
                            var connectTask = client.ConnectAsync(ip, port);
                            if (await Task.WhenAny(connectTask, Task.Delay(200)) == connectTask)
                            {
                                // Port is open, try to identify device
                                DeviceType deviceType = await IdentifyDeviceAsync(ip, port);
                                if (deviceType != DeviceType.Unknown)
                                {
                                    var deviceName = await GetDeviceNameAsync(ip, port, deviceType);
                                    
                                    _scanResults.Add(new DeviceDiscoveryResult
                                    {
                                        IpAddress = ip,
                                        Port = port,
                                        DeviceType = deviceType,
                                        IsCompatible = true,
                                        DeviceName = deviceName
                                    });
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Port is closed or connection failed
                    }
                }
            }
            catch
            {
                // Ping failed, host is unreachable
            }
        }
        
        private async Task<DeviceType> IdentifyDeviceAsync(string ip, int port)
        {
            // Try to identify device type based on response headers or other characteristics
            
            if (port == 554)
            {
                return DeviceType.IPCamera; // RTSP port typically used by IP cameras
            }
            
            try
            {
                // Try HTTP request to get device info
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                
                var response = await client.GetAsync($"http://{ip}:{port}");
                string content = await response.Content.ReadAsStringAsync();
                
                // Check for Kodi
                if (content.Contains("kodi", StringComparison.OrdinalIgnoreCase) || 
                    content.Contains("xbmc", StringComparison.OrdinalIgnoreCase) ||
                    port == 8080 && content.Contains("jsonrpc", StringComparison.OrdinalIgnoreCase))
                {
                    return DeviceType.MediaCenter;
                }
                
                // Check for Plex
                if (content.Contains("plex", StringComparison.OrdinalIgnoreCase) || port == 32400)
                {
                    return DeviceType.MediaCenter;
                }
                
                // Check for Emby
                if (content.Contains("emby", StringComparison.OrdinalIgnoreCase) || 
                    content.Contains("jellyfin", StringComparison.OrdinalIgnoreCase))
                {
                    return DeviceType.MediaCenter;
                }
                
                // Check for common camera/NAS identifiers in response
                if (content.Contains("camera", StringComparison.OrdinalIgnoreCase) || 
                    content.Contains("ipcam", StringComparison.OrdinalIgnoreCase))
                {
                    return DeviceType.IPCamera;
                }
                
                if (content.Contains("nas", StringComparison.OrdinalIgnoreCase) || 
                    content.Contains("storage", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("synology", StringComparison.OrdinalIgnoreCase) ||
                    content.Contains("qnap", StringComparison.OrdinalIgnoreCase))
                {
                    return DeviceType.NASDevice;
                }
                
                if (content.Contains("smart", StringComparison.OrdinalIgnoreCase) && 
                    content.Contains("home", StringComparison.OrdinalIgnoreCase))
                {
                    return DeviceType.SmartHomeDevice;
                }
            }
            catch
            {
                // Request failed, try other detection methods
            }
            
            // Try Kodi JSON-RPC detection
            if (port == 8080 || port == 9090)
            {
                if (await TestKodiJsonRpcAsync(ip, port))
                {
                    return DeviceType.MediaCenter;
                }
            }
            
            return DeviceType.Unknown;
        }
        
        private async Task<bool> TestKodiJsonRpcAsync(string ip, int port)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(1);
                
                var jsonRpcRequest = new
                {
                    jsonrpc = "2.0",
                    method = "JSONRPC.Ping",
                    id = 1
                };
                
                var json = System.Text.Json.JsonSerializer.Serialize(jsonRpcRequest);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync($"http://{ip}:{port}/jsonrpc", content);
                var responseText = await response.Content.ReadAsStringAsync();
                
                return responseText.Contains("pong", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
        
        private async Task<string> GetDeviceNameAsync(string ip, int port, DeviceType deviceType)
        {
            try
            {
                switch (deviceType)
                {
                    case DeviceType.MediaCenter:
                        return await GetMediaCenterNameAsync(ip, port);
                    case DeviceType.IPCamera:
                        return $"IP Camera ({ip})";
                    case DeviceType.NASDevice:
                        return $"NAS Device ({ip})";
                    case DeviceType.SmartHomeDevice:
                        return $"Smart Home Device ({ip})";
                    default:
                        return $"Device ({ip})";
                }
            }
            catch
            {
                return $"Device ({ip})";
            }
        }
        
        private async Task<string> GetMediaCenterNameAsync(string ip, int port)
        {
            try
            {
                // Try Kodi JSON-RPC to get system info
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(2);
                
                var jsonRpcRequest = new
                {
                    jsonrpc = "2.0",
                    method = "System.GetProperties",
                    @params = new { properties = new[] { "systemname", "version" } },
                    id = 1
                };
                
                var json = System.Text.Json.JsonSerializer.Serialize(jsonRpcRequest);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync($"http://{ip}:{port}/jsonrpc", content);
                var responseText = await response.Content.ReadAsStringAsync();
                
                if (responseText.Contains("systemname"))
                {
                    // Parse the response to get system name
                    return $"Kodi Media Center ({ip})";
                }
                
                return $"Media Center ({ip})";
            }
            catch
            {
                return $"Media Center ({ip})";
            }
        }
        
        private async Task ScanForOnvifDevicesAsync()
        {
            // Implementation for ONVIF device discovery
            // This would use WS-Discovery protocol to find ONVIF-compatible cameras
            
            // Simplified placeholder implementation
            await Task.Delay(500);
        }
        
        private async Task ScanForSmartHomeDevicesAsync()
        {
            // Implementation for smart home device discovery
            // This would scan for devices using protocols like SSDP (UPnP), mDNS, etc.
            
            // Simplified placeholder implementation
            await Task.Delay(500);
        }
        
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }
        
        public List<DeviceDiscoveryResult> GetScanResults()
        {
            return _scanResults;
        }
    }
}
