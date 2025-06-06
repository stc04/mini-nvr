using System;
using System.ComponentModel;

namespace NVR.Core.Models
{
    public class Camera : INotifyPropertyChanged
    {
        private string _name;
        private string _ipAddress;
        private int _port;
        private string _username;
        private string _password;
        private CameraStatus _status;
        private CameraType _type;
        private bool _isRecording;
        private DateTime _lastSeen;

        public int Id { get; set; }
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }
        public string IpAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(nameof(IpAddress)); }
        }
        public int Port
        {
            get => _port;
            set { _port = value; OnPropertyChanged(nameof(Port)); }
        }
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }
        public CameraStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(nameof(Status)); }
        }
        public CameraType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }
        public bool IsRecording
        {
            get => _isRecording;
            set { _isRecording = value; OnPropertyChanged(nameof(IsRecording)); }
        }
        public DateTime LastSeen
        {
            get => _lastSeen;
            set { _lastSeen = value; OnPropertyChanged(nameof(LastSeen)); }
        }
        public string StreamUrl { get; set; }
        public string RecordingPath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum CameraStatus
    {
        Online,
        Offline,
        Error,
        Connecting
    }

    public enum CameraType
    {
        IPCamera,
        USBCamera,
        OnvifCamera,
        RTSPCamera,
        SmartHomeCamera
    }
}
