using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NVR.Core.Models;

namespace NVR.Core.Services
{
    public class VideoRecordingService
    {
        private readonly ConcurrentDictionary<int, Process> _recordingProcesses;
        private readonly NASStorage _nasStorage;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public VideoRecordingService(NASStorage nasStorage)
        {
            _recordingProcesses = new ConcurrentDictionary<int, Process>();
            _nasStorage = nasStorage;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> StartRecordingAsync(Camera camera)
        {
            try
            {
                if (_recordingProcesses.ContainsKey(camera.Id))
                {
                    await StopRecordingAsync(camera);
                }

                var recordingPath = _nasStorage.GetRecordingPath(camera.Id, DateTime.Now);
                Directory.CreateDirectory(recordingPath);

                var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}.mp4";
                var fullPath = Path.Combine(recordingPath, fileName);

                var ffmpegArgs = BuildFFmpegArgs(camera, fullPath);
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg.exe",
                        Arguments = ffmpegArgs,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                _recordingProcesses.TryAdd(camera.Id, process);

                camera.RecordingPath = fullPath;
                camera.IsRecording = true;

                // Monitor the process
                _ = Task.Run(() => MonitorRecordingProcess(camera.Id, process));

                return true;
            }
            catch (Exception ex)
            {
                // Log error
                return false;
            }
        }

        public async Task<bool> StopRecordingAsync(Camera camera)
        {
            if (_recordingProcesses.TryRemove(camera.Id, out var process))
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                        await process.WaitForExitAsync();
                    }
                    process.Dispose();
                    camera.IsRecording = false;
                    return true;
                }
                catch (Exception ex)
                {
                    // Log error
                    return false;
                }
            }
            return false;
        }

        private string BuildFFmpegArgs(Camera camera, string outputPath)
        {
            var inputUrl = camera.StreamUrl;
            if (string.IsNullOrEmpty(inputUrl))
            {
                inputUrl = $"rtsp://{camera.Username}:{camera.Password}@{camera.IpAddress}:{camera.Port}/stream";
            }

            return $"-i \"{inputUrl}\" -c:v libx264 -preset ultrafast -crf 23 -c:a aac -f mp4 \"{outputPath}\"";
        }

        private async Task MonitorRecordingProcess(int cameraId, Process process)
        {
            try
            {
                await process.WaitForExitAsync(_cancellationTokenSource.Token);
                _recordingProcesses.TryRemove(cameraId, out _);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            foreach (var process in _recordingProcesses.Values)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill();
                    }
                    process.Dispose();
                }
                catch { }
            }
            _recordingProcesses.Clear();
        }
    }
}
