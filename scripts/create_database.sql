-- AI-IT Inc NVR System Database Schema
-- Creates all necessary tables for the Network Video Recorder system
-- Supports cameras, recordings, users, smart home integration, and system configuration

-- Enable foreign key constraints (SQLite)
PRAGMA foreign_keys = ON;

-- Drop existing tables if they exist (for clean reinstall)
DROP TABLE IF EXISTS RecordingSegments;
DROP TABLE IF EXISTS Recordings;
DROP TABLE IF EXISTS CameraEvents;
DROP TABLE IF EXISTS SmartHomeDevices;
DROP TABLE IF EXISTS NASStorageDevices;
DROP TABLE IF EXISTS CameraStreams;
DROP TABLE IF EXISTS Cameras;
DROP TABLE IF EXISTS UserSessions;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS SystemConfiguration;
DROP TABLE IF EXISTS NetworkScanResults;
DROP TABLE IF EXISTS KodiIntegration;

-- Create Users table
CREATE TABLE Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(100),
    FullName VARCHAR(100),
    Role VARCHAR(20) DEFAULT 'User', -- Admin, User, Viewer
    IsActive BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastLoginAt DATETIME,
    Settings TEXT -- JSON string for user preferences
);

-- Create default admin user (password: admin123 - should be changed!)
INSERT INTO Users (Username, PasswordHash, Email, FullName, Role, IsActive) 
VALUES ('admin', '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'admin@ai-it.com', 'System Administrator', 'Admin', 1);

-- Create UserSessions table for session management
CREATE TABLE UserSessions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    SessionToken VARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME NOT NULL,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    IpAddress VARCHAR(45),
    UserAgent TEXT,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- Create Cameras table
CREATE TABLE Cameras (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(100) NOT NULL,
    IpAddress VARCHAR(45) NOT NULL,
    Port INTEGER DEFAULT 80,
    Username VARCHAR(50),
    Password VARCHAR(100),
    StreamUrl VARCHAR(500),
    StreamType VARCHAR(20) DEFAULT 'RTSP', -- RTSP, HTTP, ONVIF
    Resolution VARCHAR(20) DEFAULT '1920x1080',
    FrameRate INTEGER DEFAULT 30,
    Manufacturer VARCHAR(50),
    Model VARCHAR(50),
    FirmwareVersion VARCHAR(50),
    IsActive BOOLEAN DEFAULT 1,
    IsRecording BOOLEAN DEFAULT 0,
    Position VARCHAR(100), -- Location description
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastSeenAt DATETIME,
    Settings TEXT, -- JSON string for camera-specific settings
    ThumbnailPath VARCHAR(500)
);

-- Create sample cameras
INSERT INTO Cameras (Name, IpAddress, Port, StreamUrl, StreamType, Position, IsActive) VALUES
('Front Door Camera', '192.168.1.100', 554, 'rtsp://192.168.1.100:554/stream1', 'RTSP', 'Front Entrance', 1),
('Backyard Camera', '192.168.1.101', 554, 'rtsp://192.168.1.101:554/stream1', 'RTSP', 'Back Garden', 1),
('Living Room Camera', '192.168.1.102', 80, 'http://192.168.1.102/video.cgi', 'HTTP', 'Living Room', 1);

-- Create CameraStreams table for multiple streams per camera
CREATE TABLE CameraStreams (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CameraId INTEGER NOT NULL,
    StreamName VARCHAR(50) NOT NULL, -- Main, Sub, Mobile
    StreamUrl VARCHAR(500) NOT NULL,
    Resolution VARCHAR(20),
    FrameRate INTEGER,
    Bitrate INTEGER,
    Codec VARCHAR(20),
    IsActive BOOLEAN DEFAULT 1,
    FOREIGN KEY (CameraId) REFERENCES Cameras(Id) ON DELETE CASCADE
);

-- Create NAS Storage Devices table
CREATE TABLE NASStorageDevices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(100) NOT NULL,
    IpAddress VARCHAR(45) NOT NULL,
    Port INTEGER DEFAULT 22,
    Protocol VARCHAR(20) DEFAULT 'SMB', -- SMB, NFS, FTP, SFTP
    SharePath VARCHAR(500),
    Username VARCHAR(50),
    Password VARCHAR(100),
    TotalSpace BIGINT, -- in bytes
    UsedSpace BIGINT, -- in bytes
    IsActive BOOLEAN DEFAULT 1,
    IsDefault BOOLEAN DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    LastCheckedAt DATETIME,
    Settings TEXT -- JSON string for NAS-specific settings
);

-- Create Smart Home Devices table
CREATE TABLE SmartHomeDevices (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(100) NOT NULL,
    DeviceType VARCHAR(50) NOT NULL, -- Alexa, GoogleHome, HomeKit, Hub
    IpAddress VARCHAR(45),
    MacAddress VARCHAR(17),
    DeviceId VARCHAR(100),
    Manufacturer VARCHAR(50),
    Model VARCHAR(50),
    FirmwareVersion VARCHAR(50),
    IsActive BOOLEAN DEFAULT 1,
    IsConnected BOOLEAN DEFAULT 0,
    LastSeenAt DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    Settings TEXT, -- JSON string for device-specific settings
    Capabilities TEXT -- JSON array of supported features
);

-- Create Kodi Integration table
CREATE TABLE KodiIntegration (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name VARCHAR(100) NOT NULL,
    IpAddress VARCHAR(45) NOT NULL,
    Port INTEGER DEFAULT 8080,
    Username VARCHAR(50),
    Password VARCHAR(100),
    Version VARCHAR(20),
    SystemName VARCHAR(100),
    IsActive BOOLEAN DEFAULT 1,
    IsConnected BOOLEAN DEFAULT 0,
    LastSeenAt DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    Settings TEXT, -- JSON string for Kodi-specific settings
    AddOnInstalled BOOLEAN DEFAULT 0 -- Whether our NVR addon is installed
);

-- Create Recordings table
CREATE TABLE Recordings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CameraId INTEGER NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    FileSize BIGINT,
    Duration INTEGER, -- in seconds
    StartTime DATETIME NOT NULL,
    EndTime DATETIME,
    RecordingType VARCHAR(20) DEFAULT 'Continuous', -- Continuous, Motion, Event, Manual
    TriggerEvent VARCHAR(100), -- What triggered the recording
    Resolution VARCHAR(20),
    FrameRate INTEGER,
    Codec VARCHAR(20),
    Bitrate INTEGER,
    IsCorrupted BOOLEAN DEFAULT 0,
    ThumbnailPath VARCHAR(500),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    StorageDeviceId INTEGER,
    FOREIGN KEY (CameraId) REFERENCES Cameras(Id) ON DELETE CASCADE,
    FOREIGN KEY (StorageDeviceId) REFERENCES NASStorageDevices(Id)
);

-- Create Recording Segments table for chunked recordings
CREATE TABLE RecordingSegments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RecordingId INTEGER NOT NULL,
    SegmentNumber INTEGER NOT NULL,
    FileName VARCHAR(255) NOT NULL,
    FilePath VARCHAR(500) NOT NULL,
    FileSize BIGINT,
    Duration INTEGER, -- in seconds
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    IsCorrupted BOOLEAN DEFAULT 0,
    FOREIGN KEY (RecordingId) REFERENCES Recordings(Id) ON DELETE CASCADE
);

-- Create Camera Events table for motion detection, alerts, etc.
CREATE TABLE CameraEvents (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CameraId INTEGER NOT NULL,
    EventType VARCHAR(50) NOT NULL, -- Motion, PersonDetected, VehicleDetected, SoundDetected
    EventTime DATETIME DEFAULT CURRENT_TIMESTAMP,
    Confidence REAL, -- 0.0 to 1.0 for AI detection confidence
    BoundingBox TEXT, -- JSON string for detection coordinates
    SnapshotPath VARCHAR(500),
    RecordingId INTEGER, -- Associated recording if any
    IsProcessed BOOLEAN DEFAULT 0,
    ProcessedAt DATETIME,
    Metadata TEXT, -- JSON string for additional event data
    FOREIGN KEY (CameraId) REFERENCES Cameras(Id) ON DELETE CASCADE,
    FOREIGN KEY (RecordingId) REFERENCES Recordings(Id)
);

-- Create System Configuration table
CREATE TABLE SystemConfiguration (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ConfigKey VARCHAR(100) NOT NULL UNIQUE,
    ConfigValue TEXT,
    ConfigType VARCHAR(20) DEFAULT 'String', -- String, Integer, Boolean, JSON
    Description TEXT,
    Category VARCHAR(50), -- General, Recording, Network, Storage, etc.
    IsUserEditable BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Insert default system configuration
INSERT INTO SystemConfiguration (ConfigKey, ConfigValue, ConfigType, Description, Category) VALUES
('SystemName', 'AI-IT Inc NVR System', 'String', 'Display name for the NVR system', 'General'),
('MaxCameras', '48', 'Integer', 'Maximum number of cameras supported', 'General'),
('RecordingQuality', 'High', 'String', 'Default recording quality (Low, Medium, High)', 'Recording'),
('RecordingDuration', '24', 'Integer', 'Default recording duration in hours', 'Recording'),
('MotionDetectionEnabled', 'true', 'Boolean', 'Enable motion detection by default', 'Recording'),
('StorageRetentionDays', '30', 'Integer', 'Number of days to keep recordings', 'Storage'),
('WebServerPort', '8080', 'Integer', 'Port for web interface', 'Network'),
('StreamingPort', '8554', 'Integer', 'Port for RTSP streaming', 'Network'),
('EnableRemoteAccess', 'true', 'Boolean', 'Allow remote access to the system', 'Network'),
('EnableSmartHomeIntegration', 'true', 'Boolean', 'Enable smart home device integration', 'SmartHome'),
('EnableKodiIntegration', 'true', 'Boolean', 'Enable Kodi media center integration', 'SmartHome'),
('FFmpegPath', '', 'String', 'Path to FFmpeg executable', 'System'),
('DatabaseVersion', '1.0', 'String', 'Current database schema version', 'System');

-- Create Network Scan Results table
CREATE TABLE NetworkScanResults (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ScanTime DATETIME DEFAULT CURRENT_TIMESTAMP,
    IpAddress VARCHAR(45) NOT NULL,
    Port INTEGER,
    DeviceType VARCHAR(50), -- Camera, NAS, SmartHome, MediaCenter
    DeviceName VARCHAR(100),
    Manufacturer VARCHAR(50),
    Model VARCHAR(50),
    FirmwareVersion VARCHAR(50),
    IsCompatible BOOLEAN DEFAULT 0,
    ResponseTime INTEGER, -- in milliseconds
    Services TEXT, -- JSON array of detected services
    Capabilities TEXT, -- JSON array of device capabilities
    LastSeen DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better performance
CREATE INDEX idx_cameras_active ON Cameras(IsActive);
CREATE INDEX idx_cameras_ip ON Cameras(IpAddress);
CREATE INDEX idx_recordings_camera_time ON Recordings(CameraId, StartTime);
CREATE INDEX idx_recordings_start_time ON Recordings(StartTime);
CREATE INDEX idx_events_camera_time ON CameraEvents(CameraId, EventTime);
CREATE INDEX idx_events_type ON CameraEvents(EventType);
CREATE INDEX idx_users_username ON Users(Username);
CREATE INDEX idx_sessions_token ON UserSessions(SessionToken);
CREATE INDEX idx_sessions_expires ON UserSessions(ExpiresAt);
CREATE INDEX idx_config_key ON SystemConfiguration(ConfigKey);
CREATE INDEX idx_scan_results_ip ON NetworkScanResults(IpAddress);
CREATE INDEX idx_scan_results_time ON NetworkScanResults(ScanTime);

-- Create views for common queries
CREATE VIEW ActiveCameras AS
SELECT * FROM Cameras WHERE IsActive = 1;

CREATE VIEW RecentRecordings AS
SELECT 
    r.*,
    c.Name as CameraName,
    c.Position as CameraPosition
FROM Recordings r
JOIN Cameras c ON r.CameraId = c.Id
WHERE r.StartTime >= datetime('now', '-7 days')
ORDER BY r.StartTime DESC;

CREATE VIEW TodaysEvents AS
SELECT 
    e.*,
    c.Name as CameraName,
    c.Position as CameraPosition
FROM CameraEvents e
JOIN Cameras c ON e.CameraId = c.Id
WHERE date(e.EventTime) = date('now')
ORDER BY e.EventTime DESC;

CREATE VIEW StorageUsage AS
SELECT 
    c.Name as CameraName,
    COUNT(r.Id) as RecordingCount,
    SUM(r.FileSize) as TotalSize,
    AVG(r.Duration) as AvgDuration
FROM Cameras c
LEFT JOIN Recordings r ON c.Id = r.CameraId
WHERE c.IsActive = 1
GROUP BY c.Id, c.Name;

-- Create triggers for automatic updates
CREATE TRIGGER update_camera_last_seen
AFTER INSERT ON Recordings
BEGIN
    UPDATE Cameras 
    SET LastSeenAt = CURRENT_TIMESTAMP 
    WHERE Id = NEW.CameraId;
END;

CREATE TRIGGER update_config_timestamp
AFTER UPDATE ON SystemConfiguration
BEGIN
    UPDATE SystemConfiguration 
    SET UpdatedAt = CURRENT_TIMESTAMP 
    WHERE Id = NEW.Id;
END;

-- Create trigger to clean up old recordings based on retention policy
CREATE TRIGGER cleanup_old_recordings
AFTER INSERT ON Recordings
BEGIN
    DELETE FROM Recordings 
    WHERE StartTime < datetime('now', '-' || 
        (SELECT ConfigValue FROM SystemConfiguration WHERE ConfigKey = 'StorageRetentionDays') || ' days');
END;

-- Insert sample data for testing
INSERT INTO SmartHomeDevices (Name, DeviceType, IpAddress, Manufacturer, IsActive, IsConnected) VALUES
('Living Room Echo', 'Alexa', '192.168.1.50', 'Amazon', 1, 1),
('Kitchen Google Home', 'GoogleHome', '192.168.1.51', 'Google', 1, 1),
('Bedroom HomePod', 'HomeKit', '192.168.1.52', 'Apple', 1, 0);

INSERT INTO NASStorageDevices (Name, IpAddress, Protocol, SharePath, TotalSpace, UsedSpace, IsActive, IsDefault) VALUES
('Main NAS Server', '192.168.1.200', 'SMB', '//192.168.1.200/nvr-storage', 2000000000000, 500000000000, 1, 1);

-- Sample camera events
INSERT INTO CameraEvents (CameraId, EventType, Confidence, SnapshotPath) VALUES
(1, 'Motion', 0.85, '/snapshots/front_door_motion_001.jpg'),
(2, 'PersonDetected', 0.92, '/snapshots/backyard_person_001.jpg'),
(1, 'VehicleDetected', 0.78, '/snapshots/front_door_vehicle_001.jpg');

-- Sample recordings
INSERT INTO Recordings (CameraId, FileName, FilePath, FileSize, Duration, StartTime, EndTime, RecordingType, Resolution, FrameRate, Codec) VALUES
(1, 'front_door_20231201_120000.mp4', '/recordings/2023/12/01/front_door_20231201_120000.mp4', 1048576000, 3600, '2023-12-01 12:00:00', '2023-12-01 13:00:00', 'Continuous', '1920x1080', 30, 'H.264'),
(2, 'backyard_20231201_120000.mp4', '/recordings/2023/12/01/backyard_20231201_120000.mp4', 1048576000, 3600, '2023-12-01 12:00:00', '2023-12-01 13:00:00', 'Continuous', '1920x1080', 30, 'H.264'),
(3, 'living_room_20231201_120000.mp4', '/recordings/2023/12/01/living_room_20231201_120000.mp4', 524288000, 3600, '2023-12-01 12:00:00', '2023-12-01 13:00:00', 'Continuous', '1280x720', 15, 'H.264');

-- Final success message
SELECT 'AI-IT Inc NVR Database Created Successfully!' as Status,
       COUNT(*) as TablesCreated
FROM sqlite_master 
WHERE type = 'table' AND name NOT LIKE 'sqlite_%';

-- Show database statistics
SELECT 
    'Database Statistics' as Info,
    (SELECT COUNT(*) FROM Users) as Users,
    (SELECT COUNT(*) FROM Cameras) as Cameras,
    (SELECT COUNT(*) FROM Recordings) as Recordings,
    (SELECT COUNT(*) FROM CameraEvents) as Events,
    (SELECT COUNT(*) FROM SmartHomeDevices) as SmartDevices,
    (SELECT COUNT(*) FROM SystemConfiguration) as ConfigItems;

PRAGMA table_info(Cameras);
