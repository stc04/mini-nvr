namespace AIIT.NVR.Core.Models
{
    public class DeviceDiscoveryResult
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public DeviceType DeviceType { get; set; }
        public bool IsCompatible { get; set; }
        public string DeviceName { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string FirmwareVersion { get; set; }
    }
    
    public enum DeviceType
    {
        Unknown,
        IPCamera,
        NASDevice,
        SmartHomeDevice,
        OnvifCamera,
        RTSPCamera,
        MediaCenter
    }
}
