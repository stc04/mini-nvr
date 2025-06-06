using System;
using System.IO;

namespace NVR.Core.Models
{
    public class NASStorage
    {
        public string Name { get; set; }
        public string NetworkPath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public long TotalSpace { get; set; }
        public long FreeSpace { get; set; }
        public bool IsConnected { get; set; }
        public DateTime LastChecked { get; set; }

        public string GetRecordingPath(int cameraId, DateTime timestamp)
        {
            var datePath = timestamp.ToString("yyyy/MM/dd");
            return Path.Combine(NetworkPath, "Recordings", $"Camera_{cameraId}", datePath);
        }

        public bool CheckConnection()
        {
            try
            {
                return Directory.Exists(NetworkPath);
            }
            catch
            {
                return false;
            }
        }
    }
}
