"use client"

import { useState } from "react"
import { motion } from "framer-motion"
import { Search, Wifi, Monitor, Router, HardDrive, Smartphone, RefreshCw } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select"
import { Progress } from "@/components/ui/progress"
import type { NetworkDevice, NetworkInterface } from "@/types/network"

interface DeviceScannerProps {
  interfaces: NetworkInterface[]
  devices: NetworkDevice[]
  isScanning: boolean
  scanProgress: number
  onScan: (subnet: string) => void
  onRefresh: () => void
}

const deviceIcons = {
  camera: Monitor,
  nvr: HardDrive,
  router: Router,
  switch: Wifi,
  unknown: Smartphone,
}

const deviceColors = {
  camera: "text-blue-400",
  nvr: "text-green-400",
  router: "text-purple-400",
  switch: "text-yellow-400",
  unknown: "text-gray-400",
}

export default function DeviceScanner({
  interfaces = [],
  devices = [],
  isScanning,
  scanProgress,
  onScan,
  onRefresh,
}: DeviceScannerProps) {
  const [selectedInterface, setSelectedInterface] = useState<string>("")

  const handleScan = () => {
    if (selectedInterface && !isScanning) {
      const iface = interfaces.find((i) => i.name === selectedInterface)
      if (iface) {
        onScan(iface.cidr)
      }
    }
  }

  const getDeviceTypeStats = () => {
    const stats = devices.reduce(
      (acc, device) => {
        acc[device.deviceType] = (acc[device.deviceType] || 0) + 1
        return acc
      },
      {} as Record<string, number>,
    )

    return Object.entries(stats).map(([type, count]) => ({
      type,
      count,
      percentage: (count / devices.length) * 100,
    }))
  }

  return (
    <div className="space-y-6">
      {/* Scanner Controls */}
      <Card className="bg-gray-800/50 border-gray-700">
        <CardHeader>
          <CardTitle className="text-white flex items-center gap-2">
            <Search className="w-5 h-5" />
            Network Scanner
          </CardTitle>
          <CardDescription className="text-gray-400">
            Discover devices on your network by scanning network interfaces
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-4">
            <div className="flex-1">
              <Select value={selectedInterface} onValueChange={setSelectedInterface}>
                <SelectTrigger className="bg-gray-700 border-gray-600 text-white">
                  <SelectValue placeholder="Select network interface" />
                </SelectTrigger>
                <SelectContent className="bg-gray-700 border-gray-600">
                  {interfaces.map((iface) => (
                    <SelectItem key={iface.name} value={iface.name} className="text-white">
                      {iface.name} - {iface.address} ({iface.cidr})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <Button
              onClick={handleScan}
              disabled={!selectedInterface || isScanning}
              className="bg-green-600 hover:bg-green-700"
            >
              {isScanning ? "Scanning..." : "Start Scan"}
            </Button>
            <Button onClick={onRefresh} variant="outline" className="border-gray-600 text-gray-300 hover:bg-gray-700">
              <RefreshCw className="w-4 h-4" />
            </Button>
          </div>

          {isScanning && (
            <div className="space-y-2">
              <div className="flex justify-between text-sm text-gray-400">
                <span>Scanning network...</span>
                <span>{Math.round(scanProgress)}%</span>
              </div>
              <Progress value={scanProgress} className="bg-gray-700" />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Device Statistics */}
      {devices.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {getDeviceTypeStats().map(({ type, count, percentage }) => {
            const Icon = deviceIcons[type as keyof typeof deviceIcons] || deviceIcons.unknown
            const colorClass = deviceColors[type as keyof typeof deviceColors] || deviceColors.unknown

            return (
              <motion.div
                key={type}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                className="bg-gray-800/50 border border-gray-700 rounded-lg p-4"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-400 capitalize">{type}s</p>
                    <p className="text-2xl font-bold text-white">{count}</p>
                  </div>
                  <Icon className={`w-8 h-8 ${colorClass}`} />
                </div>
                <div className="mt-2">
                  <div className="w-full bg-gray-700 rounded-full h-2">
                    <div
                      className="bg-green-500 h-2 rounded-full transition-all duration-300"
                      style={{ width: `${percentage}%` }}
                    />
                  </div>
                </div>
              </motion.div>
            )
          })}
        </div>
      )}

      {/* Device List */}
      <Card className="bg-gray-800/50 border-gray-700">
        <CardHeader>
          <CardTitle className="text-white">Discovered Devices ({devices.length})</CardTitle>
          <CardDescription className="text-gray-400">Network devices found during the last scan</CardDescription>
        </CardHeader>
        <CardContent>
          {devices.length === 0 ? (
            <div className="text-center py-8">
              <Search className="w-12 h-12 text-gray-600 mx-auto mb-4" />
              <p className="text-gray-400">No devices found. Start a network scan to discover devices.</p>
            </div>
          ) : (
            <div className="space-y-3">
              {devices.map((device) => {
                const Icon = deviceIcons[device.deviceType as keyof typeof deviceIcons] || deviceIcons.unknown
                const colorClass = deviceColors[device.deviceType as keyof typeof deviceColors] || deviceColors.unknown

                return (
                  <motion.div
                    key={device.id}
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    className="flex items-center justify-between p-4 bg-gray-700/50 rounded-lg border border-gray-600"
                  >
                    <div className="flex items-center space-x-4">
                      <Icon className={`w-6 h-6 ${colorClass}`} />
                      <div>
                        <p className="text-white font-medium">{device.hostname || device.ip}</p>
                        <p className="text-sm text-gray-400">
                          {device.ip} • {device.mac} • {device.deviceType}
                        </p>
                      </div>
                    </div>
                    <div className="text-right">
                      <div
                        className={`inline-flex items-center px-2 py-1 rounded-full text-xs ${
                          device.status === "online" ? "bg-green-500/20 text-green-400" : "bg-red-500/20 text-red-400"
                        }`}
                      >
                        {device.status}
                      </div>
                      <p className="text-xs text-gray-500 mt-1">{device.responseTime?.toFixed(1)}ms</p>
                    </div>
                  </motion.div>
                )
              })}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )
}
