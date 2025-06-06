"use client"

import { useState, useEffect, useCallback } from "react"
import { motion } from "framer-motion"
import { NetworkScanner } from "@/lib/network-scanner"
import { NetworkBridge } from "@/lib/network-bridge"
import type { NetworkDevice, NetworkInterface, BridgeConnection, NetworkTest } from "@/types/network"
import NetworkDashboard from "@/components/NetworkDashboard"
import DeviceScanner from "@/components/DeviceScanner"
import BridgeManager from "@/components/BridgeManager"
import NetworkTester from "@/components/NetworkTester"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"

export default function Home() {
  // Initialize services with proper error handling
  const [networkScanner, setNetworkScanner] = useState<NetworkScanner | null>(null)
  const [networkBridge, setNetworkBridge] = useState<NetworkBridge | null>(null)
  const [devices, setDevices] = useState<NetworkDevice[]>([])
  const [interfaces, setInterfaces] = useState<NetworkInterface[]>([])
  const [connections, setConnections] = useState<BridgeConnection[]>([])
  const [tests, setTests] = useState<NetworkTest[]>([])
  const [isScanning, setIsScanning] = useState(false)
  const [scanProgress, setScanProgress] = useState(0)
  const [error, setError] = useState<string | null>(null)
  const [isInitialized, setIsInitialized] = useState(false)

  // Initialize services on mount
  useEffect(() => {
    const initializeServices = async () => {
      try {
        // Create instances with proper error handling
        const scanner = new NetworkScanner()
        const bridge = new NetworkBridge()

        setNetworkScanner(scanner)
        setNetworkBridge(bridge)

        // Set up bridge event listeners
        bridge.on("connectionEstablished", (connection: BridgeConnection) => {
          setConnections((prev) => [...prev, connection])
        })

        bridge.on("connectionClosed", (connection: BridgeConnection) => {
          setConnections((prev) => prev.filter((c) => c.id !== connection.id))
        })

        bridge.on("testCompleted", (test: NetworkTest) => {
          setTests((prev) => prev.map((t) => (t.id === test.id ? test : t)))
        })

        // Load initial data
        await loadNetworkInterfaces(scanner)

        setIsInitialized(true)
        setError(null)
      } catch (err) {
        console.error("Failed to initialize services:", err)
        setError("Failed to initialize network services. Some features may not work properly.")
        setIsInitialized(true) // Still allow UI to render
      }
    }

    initializeServices()

    // Cleanup function
    return () => {
      if (networkBridge) {
        networkBridge.removeAllListeners()
      }
    }
  }, [])

  const loadNetworkInterfaces = useCallback(
    async (scanner?: NetworkScanner) => {
      const scannerInstance = scanner || networkScanner
      if (!scannerInstance) return

      try {
        const ifaces = await scannerInstance.getNetworkInterfaces()
        setInterfaces(ifaces || [])
      } catch (error) {
        console.error("Failed to load network interfaces:", error)
        setInterfaces([])
      }
    },
    [networkScanner],
  )

  const handleScanNetwork = useCallback(
    async (subnet: string) => {
      if (!networkScanner) {
        setError("Network scanner not initialized")
        return
      }

      setIsScanning(true)
      setScanProgress(0)
      setError(null)

      try {
        const result = await networkScanner.scanSubnet(subnet, setScanProgress)
        setDevices(result?.devices || [])
      } catch (error) {
        console.error("Network scan failed:", error)
        setError("Network scan failed. Please try again.")
        setDevices([])
      } finally {
        setIsScanning(false)
        setScanProgress(0)
      }
    },
    [networkScanner],
  )

  const handleCreateBridge = useCallback(
    async (protocol: "tcp" | "udp", sourcePort: number, targetIp: string, targetPort: number) => {
      if (!networkBridge) {
        setError("Network bridge not initialized")
        return
      }

      try {
        if (protocol === "tcp") {
          await networkBridge.createTCPBridge(sourcePort, targetIp, targetPort)
        } else {
          await networkBridge.createUDPBridge(sourcePort, targetIp, targetPort)
        }
        setError(null)
      } catch (error) {
        console.error("Failed to create bridge:", error)
        setError("Failed to create network bridge")
      }
    },
    [networkBridge],
  )

  const handleRunTest = useCallback(
    async (type: NetworkTest["type"], target: string, options?: any) => {
      if (!networkBridge) {
        setError("Network bridge not initialized")
        return
      }

      try {
        const testId = await networkBridge.runNetworkTest(type, target, options)
        const test = networkBridge.getTests().find((t) => t.id === testId)
        if (test) {
          setTests((prev) => [...prev, test])
        }
        setError(null)
      } catch (error) {
        console.error("Failed to run test:", error)
        setError("Failed to run network test")
      }
    },
    [networkBridge],
  )

  const handleRefreshInterfaces = useCallback(() => {
    loadNetworkInterfaces()
  }, [loadNetworkInterfaces])

  const handleDestroyBridge = useCallback(
    (bridgeId: string) => {
      if (!networkBridge) return

      try {
        networkBridge.destroyBridge(bridgeId)
        setError(null)
      } catch (error) {
        console.error("Failed to destroy bridge:", error)
        setError("Failed to destroy bridge")
      }
    },
    [networkBridge],
  )

  // Show loading state while initializing
  if (!isInitialized) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 flex items-center justify-center">
        <div className="text-center">
          <div className="w-16 h-16 bg-gradient-to-r from-green-400 to-blue-500 rounded-lg flex items-center justify-center mx-auto mb-4">
            <span className="text-white font-bold text-2xl">AI</span>
          </div>
          <h2 className="text-xl font-bold text-white mb-2">Initializing Network Services</h2>
          <p className="text-gray-400">Please wait while we set up the network bridge...</p>
          <div className="mt-4">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-green-400 mx-auto"></div>
          </div>
        </div>
      </div>
    )
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900">
      {/* Header */}
      <header className="bg-gray-800/50 backdrop-blur-sm border-b border-gray-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <motion.div
              initial={{ opacity: 0, x: -20 }}
              animate={{ opacity: 1, x: 0 }}
              className="flex items-center space-x-3"
            >
              <div className="w-10 h-10 bg-gradient-to-r from-green-400 to-blue-500 rounded-lg flex items-center justify-center">
                <span className="text-white font-bold text-lg">AI</span>
              </div>
              <div>
                <h1 className="text-xl font-bold text-white">AI-IT Inc NVR</h1>
                <p className="text-sm text-gray-400">Network Bridge Testing</p>
              </div>
            </motion.div>

            <div className="flex items-center space-x-4">
              <div className="text-right">
                <p className="text-sm text-gray-300">{devices.length} devices found</p>
                <p className="text-xs text-gray-500">{connections.length} active connections</p>
              </div>
              {error && (
                <div className="bg-red-500/20 border border-red-500/50 rounded-lg px-3 py-1">
                  <p className="text-red-400 text-xs">{error}</p>
                </div>
              )}
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <Tabs defaultValue="dashboard" className="space-y-6">
          <TabsList className="grid w-full grid-cols-4 bg-gray-800/50 border border-gray-700">
            <TabsTrigger value="dashboard" className="data-[state=active]:bg-green-500/20">
              Dashboard
            </TabsTrigger>
            <TabsTrigger value="scanner" className="data-[state=active]:bg-green-500/20">
              Device Scanner
            </TabsTrigger>
            <TabsTrigger value="bridge" className="data-[state=active]:bg-green-500/20">
              Bridge Manager
            </TabsTrigger>
            <TabsTrigger value="tester" className="data-[state=active]:bg-green-500/20">
              Network Tester
            </TabsTrigger>
          </TabsList>

          <TabsContent value="dashboard">
            <NetworkDashboard devices={devices} interfaces={interfaces} connections={connections} tests={tests} />
          </TabsContent>

          <TabsContent value="scanner">
            <DeviceScanner
              interfaces={interfaces}
              devices={devices}
              isScanning={isScanning}
              scanProgress={scanProgress}
              onScan={handleScanNetwork}
              onRefresh={handleRefreshInterfaces}
            />
          </TabsContent>

          <TabsContent value="bridge">
            <BridgeManager
              devices={devices}
              connections={connections}
              onCreateBridge={handleCreateBridge}
              onDestroyBridge={handleDestroyBridge}
            />
          </TabsContent>

          <TabsContent value="tester">
            <NetworkTester devices={devices} tests={tests} onRunTest={handleRunTest} />
          </TabsContent>
        </Tabs>
      </main>
    </div>
  )
}
