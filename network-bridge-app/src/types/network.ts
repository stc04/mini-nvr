export interface NetworkDevice {
  id: string
  ip: string
  hostname?: string
  mac?: string
  vendor?: string
  deviceType: "camera" | "nvr" | "switch" | "router" | "unknown"
  status: "online" | "offline" | "testing"
  lastSeen: Date
  responseTime?: number
  ports?: PortScanResult[]
  services?: ServiceInfo[]
}

export interface PortScanResult {
  port: number
  protocol: "tcp" | "udp"
  status: "open" | "closed" | "filtered"
  service?: string
  version?: string
}

export interface ServiceInfo {
  name: string
  port: number
  protocol: string
  version?: string
  banner?: string
}

export interface NetworkInterface {
  name: string
  address: string
  netmask: string
  family: "IPv4" | "IPv6"
  mac: string
  internal: boolean
  cidr: string
}

export interface NetworkScanResult {
  subnet: string
  devices: NetworkDevice[]
  scanDuration: number
  timestamp: Date
}

export interface BridgeConnection {
  id: string
  sourceIp: string
  targetIp: string
  sourcePort: number
  targetPort: number
  protocol: "tcp" | "udp"
  status: "active" | "inactive" | "error"
  bytesTransferred: number
  connectTime: Date
  lastActivity: Date
}

export interface NetworkTest {
  id: string
  type: "ping" | "traceroute" | "bandwidth" | "latency" | "port_scan"
  target: string
  status: "running" | "completed" | "failed"
  startTime: Date
  endTime?: Date
  results: any
  progress: number
}

export interface BandwidthTest {
  downloadSpeed: number // Mbps
  uploadSpeed: number // Mbps
  latency: number // ms
  jitter: number // ms
  packetLoss: number // percentage
}
