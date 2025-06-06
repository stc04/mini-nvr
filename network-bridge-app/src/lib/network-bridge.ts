import { EventEmitter } from "events"
import type { BridgeConnection, NetworkTest } from "@/types/network"

export class NetworkBridge extends EventEmitter {
  private connections: Map<string, BridgeConnection> = new Map()
  private tests: Map<string, NetworkTest> = new Map()
  private nextConnectionId = 1
  private nextTestId = 1

  constructor() {
    super()
    this.setMaxListeners(50) // Increase max listeners to prevent warnings
  }

  async createTCPBridge(sourcePort: number, targetIp: string, targetPort: number): Promise<string> {
    try {
      const connectionId = `tcp-bridge-${this.nextConnectionId++}`

      const connection: BridgeConnection = {
        id: connectionId,
        protocol: "tcp",
        sourcePort,
        targetIp,
        targetPort,
        status: "active",
        createdAt: new Date(),
        bytesTransferred: 0,
        connectionsCount: 0,
      }

      this.connections.set(connectionId, connection)

      // Simulate bridge creation
      setTimeout(() => {
        this.emit("connectionEstablished", connection)
      }, 100)

      return connectionId
    } catch (error) {
      console.error("Failed to create TCP bridge:", error)
      throw new Error("Failed to create TCP bridge")
    }
  }

  async createUDPBridge(sourcePort: number, targetIp: string, targetPort: number): Promise<string> {
    try {
      const connectionId = `udp-bridge-${this.nextConnectionId++}`

      const connection: BridgeConnection = {
        id: connectionId,
        protocol: "udp",
        sourcePort,
        targetIp,
        targetPort,
        status: "active",
        createdAt: new Date(),
        bytesTransferred: 0,
        connectionsCount: 0,
      }

      this.connections.set(connectionId, connection)

      // Simulate bridge creation
      setTimeout(() => {
        this.emit("connectionEstablished", connection)
      }, 100)

      return connectionId
    } catch (error) {
      console.error("Failed to create UDP bridge:", error)
      throw new Error("Failed to create UDP bridge")
    }
  }

  destroyBridge(bridgeId: string): void {
    try {
      const connection = this.connections.get(bridgeId)
      if (connection) {
        connection.status = "closed"
        this.connections.delete(bridgeId)
        this.emit("connectionClosed", connection)
      }
    } catch (error) {
      console.error("Failed to destroy bridge:", error)
      throw new Error("Failed to destroy bridge")
    }
  }

  async runNetworkTest(type: NetworkTest["type"], target: string, options?: any): Promise<string> {
    try {
      const testId = `test-${this.nextTestId++}`

      const test: NetworkTest = {
        id: testId,
        type,
        target,
        status: "running",
        startTime: new Date(),
        options,
      }

      this.tests.set(testId, test)

      // Simulate test execution
      setTimeout(
        () => {
          const completedTest: NetworkTest = {
            ...test,
            status: "completed",
            endTime: new Date(),
            result: this.generateMockTestResult(type, target),
          }

          this.tests.set(testId, completedTest)
          this.emit("testCompleted", completedTest)
        },
        2000 + Math.random() * 3000,
      ) // Random delay between 2-5 seconds

      return testId
    } catch (error) {
      console.error("Failed to run network test:", error)
      throw new Error("Failed to run network test")
    }
  }

  private generateMockTestResult(type: NetworkTest["type"], target: string): any {
    switch (type) {
      case "ping":
        return {
          packetsTransmitted: 4,
          packetsReceived: 4,
          packetLoss: 0,
          minTime: 10.2,
          maxTime: 15.8,
          avgTime: 12.5,
          stdDev: 2.1,
        }

      case "traceroute":
        return {
          hops: [
            { hop: 1, ip: "192.168.1.1", hostname: "router.local", time: 1.2 },
            { hop: 2, ip: "10.0.0.1", hostname: "gateway", time: 5.4 },
            { hop: 3, ip: target, hostname: "", time: 12.8 },
          ],
        }

      case "bandwidth":
        return {
          downloadSpeed: Math.random() * 100 + 50, // 50-150 Mbps
          uploadSpeed: Math.random() * 50 + 25, // 25-75 Mbps
          latency: Math.random() * 20 + 5, // 5-25 ms
          jitter: Math.random() * 5 + 1, // 1-6 ms
        }

      case "port-scan":
        return {
          openPorts: [22, 80, 443, 554, 8080].filter(() => Math.random() > 0.3),
          closedPorts: [21, 23, 25, 53].filter(() => Math.random() > 0.7),
          filteredPorts: [135, 139, 445].filter(() => Math.random() > 0.8),
        }

      default:
        return { success: true, message: "Test completed successfully" }
    }
  }

  getConnections(): BridgeConnection[] {
    return Array.from(this.connections.values())
  }

  getTests(): NetworkTest[] {
    return Array.from(this.tests.values())
  }

  getConnection(id: string): BridgeConnection | undefined {
    return this.connections.get(id)
  }

  getTest(id: string): NetworkTest | undefined {
    return this.tests.get(id)
  }
}
