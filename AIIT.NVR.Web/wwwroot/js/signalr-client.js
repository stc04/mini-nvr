// AI-IT Inc NVR SignalR Client
class NVRSignalRClient {
  constructor() {
    this.connection = null
    this.isConnected = false
    this.reconnectAttempts = 0
    this.maxReconnectAttempts = 10
    this.reconnectDelay = 5000
    this.subscribedCameras = new Set()
    this.eventHandlers = new Map()

    this.init()
  }

  async init() {
    try {
      // Create SignalR connection
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl("/nvrhub", {
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            if (retryContext.previousRetryCount < 5) {
              return 2000
            } else if (retryContext.previousRetryCount < 10) {
              return 5000
            } else {
              return 10000
            }
          },
        })
        .configureLogging(signalR.LogLevel.Information)
        .build()

      this.setupEventHandlers()
      await this.start()
    } catch (error) {
      console.error("Failed to initialize SignalR connection:", error)
      this.scheduleReconnect()
    }
  }

  setupEventHandlers() {
    // Connection events
    this.connection.onclose(async (error) => {
      this.isConnected = false
      this.updateConnectionStatus(false)
      console.log("SignalR connection closed:", error)

      if (error) {
        this.scheduleReconnect()
      }
    })

    this.connection.onreconnecting((error) => {
      this.isConnected = false
      this.updateConnectionStatus(false, "Reconnecting...")
      console.log("SignalR reconnecting:", error)
    })

    this.connection.onreconnected((connectionId) => {
      this.isConnected = true
      this.reconnectAttempts = 0
      this.updateConnectionStatus(true)
      console.log("SignalR reconnected:", connectionId)

      // Resubscribe to previous subscriptions
      this.resubscribeAll()
    })

    // Server event handlers
    this.connection.on("Connected", (data) => {
      console.log("Connected to NVR Hub:", data)
      this.isConnected = true
      this.updateConnectionStatus(true)
      this.triggerEvent("connected", data)
    })

    this.connection.on("CameraStatusUpdate", (data) => {
      console.log("Camera status update:", data)
      this.updateCameraStatus(data)
      this.triggerEvent("cameraStatusUpdate", data)
    })

    this.connection.on("RecordingStatusUpdate", (data) => {
      console.log("Recording status update:", data)
      this.updateRecordingStatus(data)
      this.triggerEvent("recordingStatusUpdate", data)
    })

    this.connection.on("MotionDetected", (data) => {
      console.log("Motion detected:", data)
      this.showMotionAlert(data)
      this.triggerEvent("motionDetected", data)
    })

    this.connection.on("SystemStatusUpdate", (data) => {
      console.log("System status update:", data)
      this.updateSystemStatus(data)
      this.triggerEvent("systemStatusUpdate", data)
    })

    this.connection.on("PerformanceUpdate", (data) => {
      console.log("Performance update:", data)
      this.updatePerformanceMetrics(data)
      this.triggerEvent("performanceUpdate", data)
    })

    this.connection.on("SystemEvent", (data) => {
      console.log("System event:", data)
      this.showSystemNotification(data)
      this.triggerEvent("systemEvent", data)
    })

    this.connection.on("StorageAlert", (data) => {
      console.log("Storage alert:", data)
      this.showStorageAlert(data)
      this.triggerEvent("storageAlert", data)
    })

    this.connection.on("SnapshotCaptured", (data) => {
      console.log("Snapshot captured:", data)
      this.showSnapshotNotification(data)
      this.triggerEvent("snapshotCaptured", data)
    })

    this.connection.on("LiveStreamUrl", (data) => {
      console.log("Live stream URL:", data)
      this.updateStreamUrl(data)
      this.triggerEvent("liveStreamUrl", data)
    })

    this.connection.on("PTZCommandExecuted", (data) => {
      console.log("PTZ command executed:", data)
      this.triggerEvent("ptzCommandExecuted", data)
    })

    this.connection.on("Error", (data) => {
      console.error("Server error:", data)
      this.showErrorNotification(data)
      this.triggerEvent("error", data)
    })
  }

  async start() {
    try {
      await this.connection.start()
      console.log("SignalR connection started successfully")
      this.isConnected = true
      this.reconnectAttempts = 0

      // Subscribe to system status by default
      await this.subscribeToSystemStatus()
    } catch (error) {
      console.error("Failed to start SignalR connection:", error)
      this.scheduleReconnect()
    }
  }

  async stop() {
    if (this.connection) {
      await this.connection.stop()
      this.isConnected = false
      this.updateConnectionStatus(false)
    }
  }

  scheduleReconnect() {
    if (this.reconnectAttempts < this.maxReconnectAttempts) {
      this.reconnectAttempts++
      console.log(`Scheduling reconnect attempt ${this.reconnectAttempts} in ${this.reconnectDelay}ms`)

      setTimeout(async () => {
        try {
          await this.start()
        } catch (error) {
          console.error("Reconnect attempt failed:", error)
          this.scheduleReconnect()
        }
      }, this.reconnectDelay)
    } else {
      console.error("Max reconnect attempts reached")
      this.showErrorNotification({
        message: "Connection lost",
        details: "Unable to reconnect to the server. Please refresh the page.",
      })
    }
  }

  // Subscription methods
  async subscribeToCameraFeed(cameraId) {
    if (this.isConnected) {
      try {
        await this.connection.invoke("SubscribeToCameraFeed", cameraId)
        this.subscribedCameras.add(cameraId)
        console.log(`Subscribed to camera ${cameraId}`)
      } catch (error) {
        console.error(`Failed to subscribe to camera ${cameraId}:`, error)
      }
    }
  }

  async unsubscribeFromCameraFeed(cameraId) {
    if (this.isConnected) {
      try {
        await this.connection.invoke("UnsubscribeFromCameraFeed", cameraId)
        this.subscribedCameras.delete(cameraId)
        console.log(`Unsubscribed from camera ${cameraId}`)
      } catch (error) {
        console.error(`Failed to unsubscribe from camera ${cameraId}:`, error)
      }
    }
  }

  async subscribeToSystemStatus() {
    if (this.isConnected) {
      try {
        await this.connection.invoke("SubscribeToSystemStatus")
        console.log("Subscribed to system status")
      } catch (error) {
        console.error("Failed to subscribe to system status:", error)
      }
    }
  }

  async resubscribeAll() {
    // Resubscribe to system status
    await this.subscribeToSystemStatus()

    // Resubscribe to all cameras
    for (const cameraId of this.subscribedCameras) {
      await this.subscribeToCameraFeed(cameraId)
    }
  }

  // Camera control methods
  async startRecording(cameraId) {
    if (this.isConnected) {
      try {
        await this.connection.invoke("StartRecording", cameraId)
      } catch (error) {
        console.error(`Failed to start recording for camera ${cameraId}:`, error)
      }
    }
  }

  async stopRecording(cameraId) {
    if (this.isConnected) {
      try {
        await this.connection.invoke("StopRecording", cameraId)
      } catch (error) {
        console.error(`Failed to stop recording for camera ${cameraId}:`, error)
      }
    }
  }

  async captureSnapshot(cameraId) {
    if (this.isConnected) {
      try {
        await this.connection.invoke("RequestCameraSnapshot", cameraId)
      } catch (error) {
        console.error(`Failed to capture snapshot for camera ${cameraId}:`, error)
      }
    }
  }

  async sendPTZCommand(cameraId, command, parameters = null) {
    if (this.isConnected) {
      try {
        await this.connection.invoke("SendPTZCommand", cameraId, command, parameters)
      } catch (error) {
        console.error(`Failed to send PTZ command to camera ${cameraId}:`, error)
      }
    }
  }

  async getLiveStreamUrl(cameraId, quality = "medium") {
    if (this.isConnected) {
      try {
        await this.connection.invoke("GetLiveStreamUrl", cameraId, quality)
      } catch (error) {
        console.error(`Failed to get live stream URL for camera ${cameraId}:`, error)
      }
    }
  }

  // UI update methods
  updateConnectionStatus(connected, message = null) {
    const statusElement = document.getElementById("connection-status")
    if (statusElement) {
      if (connected) {
        statusElement.innerHTML = '<i class="fas fa-circle text-success"></i> Connected'
        statusElement.className = "text-success"
      } else {
        const displayMessage = message || "Disconnected"
        statusElement.innerHTML = `<i class="fas fa-circle text-danger"></i> ${displayMessage}`
        statusElement.className = "text-danger"
      }
    }
  }

  updateCameraStatus(data) {
    const cameraElement = document.getElementById(`camera-${data.cameraId}`)
    if (cameraElement) {
      const statusBadge = cameraElement.querySelector(".status-badge")
      const recordingBadge = cameraElement.querySelector(".recording-badge")

      if (statusBadge) {
        statusBadge.className = `badge ${data.status === "Online" ? "bg-success" : "bg-danger"}`
        statusBadge.textContent = data.status
      }

      if (recordingBadge) {
        recordingBadge.style.display = data.isRecording ? "block" : "none"
      }
    }
  }

  updateRecordingStatus(data) {
    const cameraElement = document.getElementById(`camera-${data.cameraId}`)
    if (cameraElement) {
      const recordingBadge = cameraElement.querySelector(".recording-badge")
      if (recordingBadge) {
        recordingBadge.style.display = data.isRecording ? "block" : "none"
      }
    }
  }

  updateSystemStatus(data) {
    // Update camera counts
    const totalCamerasElement = document.getElementById("total-cameras")
    const onlineCamerasElement = document.getElementById("online-cameras")
    const recordingCamerasElement = document.getElementById("recording-cameras")

    if (totalCamerasElement) totalCamerasElement.textContent = data.totalCameras
    if (onlineCamerasElement) onlineCamerasElement.textContent = data.onlineCameras
    if (recordingCamerasElement) recordingCamerasElement.textContent = data.recordingCameras

    // Update storage info
    if (data.storage) {
      const storageElement = document.getElementById("storage-usage")
      if (storageElement) {
        const usagePercent = ((data.storage.usedSpace / data.storage.totalSpace) * 100).toFixed(1)
        storageElement.textContent = `${usagePercent}%`
      }
    }
  }

  updatePerformanceMetrics(data) {
    // Update CPU usage
    const cpuElement = document.getElementById("cpu-usage")
    const cpuProgress = document.getElementById("cpu-progress")
    if (cpuElement && cpuProgress) {
      cpuElement.textContent = `${data.cpuUsage}%`
      cpuProgress.style.width = `${data.cpuUsage}%`
      cpuProgress.className = `progress-bar ${this.getProgressBarClass(data.cpuUsage)}`
    }

    // Update Memory usage
    const memoryElement = document.getElementById("memory-usage")
    const memoryProgress = document.getElementById("memory-progress")
    if (memoryElement && memoryProgress) {
      memoryElement.textContent = `${data.memoryUsage}%`
      memoryProgress.style.width = `${data.memoryUsage}%`
      memoryProgress.className = `progress-bar ${this.getProgressBarClass(data.memoryUsage)}`
    }

    // Update Network usage
    const networkElement = document.getElementById("network-usage")
    const networkProgress = document.getElementById("network-progress")
    if (networkElement && networkProgress) {
      networkElement.textContent = `${data.networkUsage}%`
      networkProgress.style.width = `${data.networkUsage}%`
      networkProgress.className = `progress-bar ${this.getProgressBarClass(data.networkUsage)}`
    }
  }

  getProgressBarClass(percentage) {
    if (percentage < 50) return "bg-success"
    if (percentage < 75) return "bg-warning"
    return "bg-danger"
  }

  updateStreamUrl(data) {
    const videoElement = document.getElementById(`video-${data.cameraId}`)
    if (videoElement) {
      videoElement.src = data.streamUrl
    }
  }

  // Notification methods
  showMotionAlert(data) {
    this.showNotification({
      title: "Motion Detected",
      message: `Motion detected on Camera ${data.cameraId} with ${data.confidence}% confidence`,
      type: "warning",
      duration: 5000,
    })
  }

  showSystemNotification(data) {
    this.showNotification({
      title: "System Event",
      message: data.message,
      type: data.severity,
      duration: 3000,
    })
  }

  showStorageAlert(data) {
    this.showNotification({
      title: "Storage Alert",
      message: data.message,
      type: data.severity,
      duration: 10000,
    })
  }

  showSnapshotNotification(data) {
    this.showNotification({
      title: "Snapshot Captured",
      message: `Snapshot captured from Camera ${data.cameraId}`,
      type: "success",
      duration: 3000,
    })
  }

  showErrorNotification(data) {
    this.showNotification({
      title: "Error",
      message: data.message,
      type: "danger",
      duration: 5000,
    })
  }

  showNotification({ title, message, type = "info", duration = 3000 }) {
    // Create notification element
    const notification = document.createElement("div")
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`
    notification.style.cssText = "top: 20px; right: 20px; z-index: 9999; min-width: 300px; max-width: 400px;"

    notification.innerHTML = `
            <strong>${title}</strong><br>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `

    document.body.appendChild(notification)

    // Auto-remove after duration
    setTimeout(() => {
      if (notification.parentNode) {
        notification.remove()
      }
    }, duration)
  }

  // Event system
  on(eventName, handler) {
    if (!this.eventHandlers.has(eventName)) {
      this.eventHandlers.set(eventName, [])
    }
    this.eventHandlers.get(eventName).push(handler)
  }

  off(eventName, handler) {
    if (this.eventHandlers.has(eventName)) {
      const handlers = this.eventHandlers.get(eventName)
      const index = handlers.indexOf(handler)
      if (index > -1) {
        handlers.splice(index, 1)
      }
    }
  }

  triggerEvent(eventName, data) {
    if (this.eventHandlers.has(eventName)) {
      this.eventHandlers.get(eventName).forEach((handler) => {
        try {
          handler(data)
        } catch (error) {
          console.error(`Error in event handler for ${eventName}:`, error)
        }
      })
    }
  }

  // Utility methods
  getConnectionState() {
    return this.connection ? this.connection.state : "Disconnected"
  }

  isConnectionActive() {
    return this.isConnected && this.connection && this.connection.state === signalR.HubConnectionState.Connected
  }
}

// Global instance
let nvrSignalR = null

// Initialize when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  nvrSignalR = new NVRSignalRClient()

  // Make it globally available
  window.nvrSignalR = nvrSignalR
})

// Cleanup on page unload
window.addEventListener("beforeunload", () => {
  if (nvrSignalR) {
    nvrSignalR.stop()
  }
})
