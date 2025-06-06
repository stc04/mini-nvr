"use client"

import { motion } from "framer-motion"
import type { NetworkDevice, NetworkInterface, BridgeConnection, NetworkTest } from "@/types/network"
import {
  ComputerDesktopIcon,
  SignalIcon,
  CpuChipIcon,
  ClockIcon,
  ExclamationTriangleIcon,
} from "@heroicons/react/24/outline"
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
} from "recharts"

interface NetworkDashboardProps {
  devices: NetworkDevice[]
  interfaces: NetworkInterface[]
  connections: BridgeConnection[]
  tests: NetworkTest[]
}

export default function NetworkDashboard({
  devices = [],
  interfaces = [],
  connections = [],
  tests = [],
}: NetworkDashboardProps) {
  // Add safety checks for all arrays
  const safeDevices = devices || []
  const safeInterfaces = interfaces || []
  const safeConnections = connections || []
  const safeTests = tests || []

  const onlineDevices = safeDevices.filter((d) => d.status === "online")
  const cameras = safeDevices.filter((d) => d.deviceType === "camera")
  const nvrs = safeDevices.filter((d) => d.deviceType === "nvr")
  const activeConnections = safeConnections.filter((c) => c.status === "active")
  const recentTests = safeTests.slice(-10)

  const deviceTypeData = [
    { name: "Cameras", value: cameras.length, color: "#10b981" },
    { name: "NVRs", value: nvrs.length, color: "#3b82f6" },
    { name: "Routers", value: safeDevices.filter((d) => d.deviceType === "router").length, color: "#f59e0b" },
    { name: "Unknown", value: safeDevices.filter((d) => d.deviceType === "unknown").length, color: "#6b7280" },
  ]

  const networkActivityData = recentTests.map((test, index) => ({
    name: `Test ${index + 1}`,
    latency: test.results?.avgLatency || 0,
    success: test.status === "completed" ? 100 : 0,
  }))

  return (
    <div className="space-y-6">
      {/* Overview Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.1 }}
          className="bg-gray-800/50 backdrop-blur-sm rounded-xl p-6 border border-gray-700"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-400">Total Devices</p>
              <p className="text-3xl font-bold text-white">{safeDevices.length}</p>
              <p className="text-sm text-green-400">{onlineDevices.length} online</p>
            </div>
            <ComputerDesktopIcon className="w-12 h-12 text-green-400" />
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
          className="bg-gray-800/50 backdrop-blur-sm rounded-xl p-6 border border-gray-700"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-400">Active Bridges</p>
              <p className="text-3xl font-bold text-white">{activeConnections.length}</p>
              <p className="text-sm text-blue-400">{connections.length} total</p>
            </div>
            <SignalIcon className="w-12 h-12 text-blue-400" />
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="bg-gray-800/50 backdrop-blur-sm rounded-xl p-6 border border-gray-700"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-400">Network Tests</p>
              <p className="text-3xl font-bold text-white">{safeTests.length}</p>
              <p className="text-sm text-yellow-400">
                {safeTests.filter((t) => t.status === "running").length} running
              </p>
            </div>
            <CpuChipIcon className="w-12 h-12 text-yellow-400" />
          </div>
        </motion.div>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.4 }}
          className="bg-gray-800/50 backdrop-blur-sm rounded-xl p-6 border border-gray-700"
        >
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-400">Avg Latency</p>
              <p className="text-3xl font-bold text-white">
                {recentTests.length > 0
                  ? Math.round(
                      recentTests.reduce((acc, test) => acc + (test.results?.avgLatency || 0), 0) / recentTests.length,
                    )
                  : 0}
                ms
              </p>
              <p className="text-sm text-purple-400">Last 10 tests</p>
            </div>
            <ClockIcon className="w-12 h-12 text-purple-400" />
          </div>
        </motion.div>
      </div>

      {/* Charts Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Device Types Chart */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.5 }}
          className="bg-gray-800/50 backdrop-blur-sm rounded-xl p-6 border border-gray-700"
        >
          <h3 className="text-lg font-semibold text-white mb-4">Device Types</h3>
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie
                data={deviceTypeData}
                cx="50%"
                cy="50%"
                outerRadius={100}
                fill="#8884d8"
                dataKey="value"
                label={({ name, value }) => `${name}: ${value}`}
              >
                {deviceTypeData.map((entry, index) => (
                  <Cell key={`cell-${index}`} fill={entry.color} />
                ))}
              </Pie>
              <Tooltip />
            </PieChart>
          </ResponsiveContainer>
        </motion.div>

        {/* Network Activity Chart */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.6 }}
          className="bg-gray-800/50 backdrop-blur-sm rounded-xl p-6 border border-gray-700"
        >
          <h3 className="text-lg font-semibold text-white mb-4">Network Activity</h3>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={networkActivityData}>
              <CartesianGrid strokeDasharray="3 3" stroke="#374151" />
              <XAxis dataKey="name" stroke="#9ca3af" />
              <YAxis stroke="#9ca3af" />
              <Tooltip
                contentStyle={{
                  backgroundColor: "#1f2937",
                  border: "1px solid #374151",
                  borderRadius: "8px",
                }}
              />
              <Line type="monotone" dataKey="latency" stroke="#10b981" strokeWidth={2} name="Latency (ms)" />
              <Line type="monotone" dataKey="success" stroke="#3b82f6" strokeWidth={2} name="Success Rate (%)" />
            </LineChart>
          </ResponsiveContainer>
        </motion.div>
      </div>

      {/* Recent Activity */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.7 }}
        className="bg-gray-800/50 backdrop-blur-sm rounded-xl p-6 border border-gray-700"
      >
        <h3 className="text-lg font-semibold text-white mb-4">Recent Activity</h3>
        <div className="space-y-3">
          {safeTests.length > 0 ? (
            safeTests
              .slice(-5)
              .reverse()
              .map((test) => (
                <div key={test.id} className="flex items-center justify-between p-3 bg-gray-700/30 rounded-lg">
                  <div className="flex items-center space-x-3">
                    <div
                      className={`w-3 h-3 rounded-full ${
                        test.status === "completed"
                          ? "bg-green-400"
                          : test.status === "running"
                            ? "bg-yellow-400"
                            : "bg-red-400"
                      }`}
                    />
                    <div>
                      <p className="text-white font-medium">
                        {test.type.replace("_", " ").toUpperCase()} test to {test.target}
                      </p>
                      <p className="text-sm text-gray-400">{test.startTime.toLocaleTimeString()}</p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className="text-sm text-gray-300">
                      {test.status === "completed" && test.results?.avgLatency
                        ? `${Math.round(test.results.avgLatency)}ms`
                        : test.status}
                    </p>
                  </div>
                </div>
              ))
          ) : (
            <div className="text-center py-8">
              <ExclamationTriangleIcon className="w-12 h-12 text-gray-500 mx-auto mb-3" />
              <p className="text-gray-400">No recent activity</p>
            </div>
          )}
        </div>
      </motion.div>

      {/* Network Interfaces */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ delay: 0.8 }}
        className="bg-gray-800/50 backdrop-blur-sm rounded-xl p-6 border border-gray-700"
      >
        <h3 className="text-lg font-semibold text-white mb-4">Network Interfaces</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {safeInterfaces.map((iface) => (
            <div key={iface.name} className="p-4 bg-gray-700/30 rounded-lg">
              <div className="flex items-center justify-between mb-2">
                <h4 className="font-medium text-white">{iface.name}</h4>
                <span className="text-xs bg-green-500/20 text-green-400 px-2 py-1 rounded">Active</span>
              </div>
              <div className="space-y-1 text-sm">
                <p className="text-gray-300">IP: {iface.address}</p>
                <p className="text-gray-400">CIDR: {iface.cidr}</p>
                <p className="text-gray-400">MAC: {iface.mac}</p>
              </div>
            </div>
          ))}
        </div>
      </motion.div>
    </div>
  )
}
