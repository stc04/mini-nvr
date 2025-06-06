import { EventEmitter } from "events"

interface NetworkInterface {
  name: string
  address: string
  netmask: string
  family: string
  mac: string
  internal: boolean
  cidr: string
}

interface NetworkDevice {
  id: string
  ip: string
  hostname: string
  mac: string
  vendor: string
  deviceType: string
  status: string
  lastSeen: Date
  responseTime: number
}

interface NetworkScanResult {
  subnet: string
  devices: NetworkDevice[]
  scanDuration: number
  timestamp: Date
}

export class NetworkScanner extends EventEmitter {
  private isScanning = false
  private abortController: AbortController | null = null

  async getNetworkInterfaces(): Promise<NetworkInterface[]> {
    try {
      // Mock implementation for browser environment
      return [
        {
          name: "eth0",
          address: "192.168.1.100",
          netmask: "255.255.255.0",
          family: "IPv4",
          mac: "00:11:22:33:44:55",
          internal: false,
          cidr: "192.168.1.0/24",
        },
        {
          name: "wlan0",
          address: "192.168.1.101",
          netmask: "255.255.255.0",
          family: "IPv4",
          mac: "66:77:88:99:AA:BB",
          internal: false,
          cidr: "192.168.1.0/24",
        },
      ]
    } catch (error) {
      console.error("Failed to get network interfaces:", error)
      return []
    }
  }

  async scanSubnet(subnet: string, progressCallback?: (progress: number) => void): Promise<NetworkScanResult> {
    try {
      this.isScanning = true
      this.abortController = new AbortController()

      const startTime = Date.now()
      const devices: NetworkDevice[] = []

      // Mock scanning with progress updates
      for (let i = 1; i <= 254; i++) {
        if (this.abortController.signal.aborted) break

        const progress = (i / 254) * 100
        progressCallback?.(progress)

        // Simulate finding some devices
        if (i % 10 === 0) {
          devices.push({
            id: `device-${i}`,
            ip: `${subnet.split("/")[0].split(".").slice(0, 3).join(".")}.${i}`,
            hostname: `device-${i}`,
            mac: `00:11:22:33:44:${i.toString(16).padStart(2, "0")}`,
            vendor: "Unknown",
            deviceType: i % 20 === 0 ? "camera" : i % 30 === 0 ? "nvr" : "unknown",
            status: "online",
            lastSeen: new Date(),
            responseTime: Math.random() * 100 + 10,
          })
        }

        // Simulate delay
        await new Promise((resolve) => setTimeout(resolve, 10))
      }

      const scanDuration = Date.now() - startTime

      return {
        subnet,
        devices,
        scanDuration,
        timestamp: new Date(),
      }
    } catch (error) {
      console.error("Subnet scan failed:", error)
      return {
        subnet,
        devices: [],
        scanDuration: 0,
        timestamp: new Date(),
      }
    } finally {
      this.isScanning = false
      this.abortController = null
    }
  }

  abortScan(): void {
    if (this.abortController) {
      this.abortController.abort()
    }
  }

  isCurrentlyScanning(): boolean {
    return this.isScanning
  }
}
