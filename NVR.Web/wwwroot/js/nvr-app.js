// NVR Web Application JavaScript

class NVRApp {
  constructor() {
    this.cameras = []
    this.socket = null
    this.isConnected = false
    this.init()
  }

  init() {
    this.initializeWebSocket()
    this.loadInitialData()
    this.setupEventListeners()
    this.startStatusUpdates()
  }

  initializeWebSocket() {
    // Initialize WebSocket connection for real-time updates
    try {
      this.socket = io()

      this.socket.on("connect", () => {
        this.isConnected = true
        this.updateConnectionStatus(true)
        console.log("Connected to NVR server")
      })

      this.socket.on("disconnect", () => {
        this.isConnected = false
        this.updateConnectionStatus(false)
        console.log("Disconnected from NVR server")
      })

      this.socket.on("camera-status-update", (data) => {
        this.handleCameraStatusUpdate(data)
      })

      this.socket.on("recording-status-update", (data) => {
        this.handleRecordingStatusUpdate(data)
      })

      this.socket.on("system-event", (data) => {
        this.handleSystemEvent(data)
      })
    } catch (error) {
      console.warn("WebSocket not available, falling back to polling")
      this.startPolling()
    }
  }

  loadInitialData() {
    this.loadCameras()
    this.loadSystemStatus()
  }

  async loadCameras() {
    try {
      const response = await fetch("/WebViewer/GetCameras")
      const cameras = await response.json()
      this.cameras = cameras
      this.updateUI()
    } catch (error) {
      console.error("Error loading cameras:", error)
      this.showNotification("Error loading cameras", "error")
    }
  }

  async loadSystemStatus() {
    try {
      const response = await fetch("/api/system/status")
      if (response.ok) {
        const status = await response.json()
        this.updateSystemStatus(status)
      }
    } catch (error) {
      console.error("Error loading system status:", error)
    }
  }

  setupEventListeners() {
    // Global keyboard shortcuts
    document.addEventListener("keydown", (e) => {
      if (e.ctrlKey) {
        switch (e.key) {
          case "1":
            e.preventDefault()
            this.setLayout(1)
            break
          case "2":
            e.preventDefault()
            this.setLayout(4)
            break
          case "3":
            e.preventDefault()
            this.setLayout(9)
            break
          case "4":
            e.preventDefault()
            this.setLayout(16)
            break
          case "r":
            e.preventDefault()
            this.refreshCameras()
            break
          case "s":
            e.preventDefault()
            this.toggleAllRecording()
            break
        }
      }
    })

    // Window resize handler
    window.addEventListener("resize", () => {
      this.handleResize()
    })

    // Visibility change handler
    document.addEventListener("visibilitychange", () => {
      if (document.hidden) {
        this.pauseUpdates()
      } else {
        this.resumeUpdates()
      }
    })
  }

  startStatusUpdates() {
    this.statusUpdateInterval = setInterval(() => {
      if (!document.hidden) {
        this.updateStatusBar()
      }
    }, 1000)
  }

  startPolling() {
    this.pollingInterval = setInterval(() => {
      if (!document.hidden) {
        this.loadCameras()
        this.loadSystemStatus()
      }
    }, 5000)
  }

  handleCameraStatusUpdate(data) {
    const camera = this.cameras.find((c) => c.id === data.cameraId)
    if (camera) {
      camera.status = data.status
      camera.lastSeen = data.lastSeen
      this.updateCameraDisplay(camera)
    }
  }

  handleRecordingStatusUpdate(data) {
    const camera = this.cameras.find((c) => c.id === data.cameraId)
    if (camera) {
      camera.isRecording = data.isRecording
      this.updateCameraDisplay(camera)
    }
  }

  handleSystemEvent(data) {
    this.showNotification(data.message, data.type)
    this.addEventToLog(data)
  }

  updateUI() {
    this.updateStatusBar()
    this.updateCameraGrid()
    this.updateDashboardStats()
  }

  updateStatusBar() {
    const onlineCameras = this.cameras.filter((c) => c.status === "Online").length
    const totalCameras = this.cameras.length
    const recordingCameras = this.cameras.filter((c) => c.isRecording).length

    const cameraCountElement = document.getElementById("camera-count")
    const recordingStatusElement = document.getElementById("recording-status")

    if (cameraCountElement) {
      cameraCountElement.innerHTML = `Cameras: <span class="text-info">${onlineCameras}/${totalCameras}</span>`
    }

    if (recordingStatusElement) {
      recordingStatusElement.innerHTML = `Recording: <span class="text-warning">${recordingCameras}</span>`
    }

    this.updateStorageInfo()
  }

  async updateStorageInfo() {
    try {
      const response = await fetch("/api/storage/info")
      if (response.ok) {
        const storage = await response.json()
        const storageElement = document.getElementById("storage-info")
        if (storageElement) {
          const freeSpaceGB = (storage.freeSpace / (1024 * 1024 * 1024)).toFixed(1)
          storageElement.innerHTML = `Storage: <span class="text-primary">${freeSpaceGB} GB Free</span>`
        }
      }
    } catch (error) {
      console.error("Error updating storage info:", error)
    }
  }

  updateConnectionStatus(connected) {
    const statusElement = document.getElementById("system-status")
    if (statusElement) {
      if (connected) {
        statusElement.innerHTML = 'System Status: <span class="text-success">Online</span>'
      } else {
        statusElement.innerHTML = 'System Status: <span class="text-danger">Disconnected</span>'
      }
    }
  }

  updateCameraDisplay(camera) {
    const cameraElement = document.getElementById(`camera-${camera.id}`)
    if (cameraElement) {
      // Update camera status indicators
      const statusBadge = cameraElement.querySelector(".status-badge")
      if (statusBadge) {
        statusBadge.className = `badge ${camera.status === "Online" ? "bg-success" : "bg-danger"}`
        statusBadge.textContent = camera.status
      }

      const recordingBadge = cameraElement.querySelector(".recording-badge")
      if (recordingBadge) {
        recordingBadge.style.display = camera.isRecording ? "block" : "none"
      }
    }
  }

  setLayout(cameraCount) {
    if (typeof window.setLayout === "function") {
      window.setLayout(cameraCount)
    }
  }

  async refreshCameras() {
    await this.loadCameras()
    this.showNotification("Cameras refreshed", "success")
  }

  async toggleAllRecording() {
    const recordingCameras = this.cameras.filter((c) => c.isRecording).length
    const action = recordingCameras > 0 ? "stop" : "start"

    try {
      const response = await fetch(`/api/recording/${action}-all`, {
        method: "POST",
      })

      if (response.ok) {
        this.showNotification(`${action === "start" ? "Started" : "Stopped"} recording on all cameras`, "success")
        await this.loadCameras()
      }
    } catch (error) {
      this.showNotification("Error toggling recording", "error")
    }
  }

  showNotification(message, type = "info") {
    // Create notification element
    const notification = document.createElement("div")
    notification.className = `alert alert-${this.getBootstrapAlertClass(type)} alert-dismissible fade show position-fixed`
    notification.style.cssText = "top: 20px; right: 20px; z-index: 9999; min-width: 300px;"

    notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `

    document.body.appendChild(notification)

    // Auto-remove after 5 seconds
    setTimeout(() => {
      if (notification.parentNode) {
        notification.remove()
      }
    }, 5000)
  }

  getBootstrapAlertClass(type) {
    switch (type) {
      case "success":
        return "success"
      case "error":
        return "danger"
      case "warning":
        return "warning"
      default:
        return "info"
    }
  }

  addEventToLog(event) {
    const eventsTable = document.getElementById("events-tbody")
    if (eventsTable) {
      const row = document.createElement("tr")
      row.innerHTML = `
                <td>${new Date(event.timestamp).toLocaleTimeString()}</td>
                <td>${event.type}</td>
                <td>${event.cameraName || "System"}</td>
                <td><span class="badge bg-${this.getStatusBadgeClass(event.status)}">${event.status}</span></td>
            `

      eventsTable.insertBefore(row, eventsTable.firstChild)

      // Keep only last 50 events
      while (eventsTable.children.length > 50) {
        eventsTable.removeChild(eventsTable.lastChild)
      }
    }
  }

  getStatusBadgeClass(status) {
    switch (status?.toLowerCase()) {
      case "success":
        return "success"
      case "error":
        return "danger"
      case "warning":
        return "warning"
      default:
        return "secondary"
    }
  }

  handleResize() {
    // Adjust video grid layout based on window size
    const videoGrid = document.getElementById("video-grid")
    if (videoGrid) {
      const width = window.innerWidth
      if (width < 576) {
        // Mobile: single column
        this.setLayout(1)
      } else if (width < 768) {
        // Tablet: 2x2
        this.setLayout(4)
      }
    }
  }

  pauseUpdates() {
    if (this.statusUpdateInterval) {
      clearInterval(this.statusUpdateInterval)
    }
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval)
    }
  }

  resumeUpdates() {
    this.startStatusUpdates()
    if (!this.isConnected) {
      this.startPolling()
    }
  }

  destroy() {
    this.pauseUpdates()
    if (this.socket) {
      this.socket.disconnect()
    }
  }
}

// Global functions for backward compatibility
let nvrApp

function startAllRecording() {
  return nvrApp?.toggleAllRecording()
}

function stopAllRecording() {
  return nvrApp?.toggleAllRecording()
}

function armSystem() {
  return fetch("/api/system/arm", { method: "POST" })
    .then(() => nvrApp?.showNotification("System armed", "success"))
    .catch(() => nvrApp?.showNotification("Error arming system", "error"))
}

function disarmSystem() {
  return fetch("/api/system/disarm", { method: "POST" })
    .then(() => nvrApp?.showNotification("System disarmed", "success"))
    .catch(() => nvrApp?.showNotification("Error disarming system", "error"))
}

function refreshCameras() {
  return nvrApp?.refreshCameras()
}

// Initialize app when DOM is ready
document.addEventListener("DOMContentLoaded", () => {
  nvrApp = new NVRApp()
})

// Cleanup on page unload
window.addEventListener("beforeunload", () => {
  if (nvrApp) {
    nvrApp.destroy()
  }
})
