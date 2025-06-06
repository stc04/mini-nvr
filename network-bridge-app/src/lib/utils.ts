import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatBytes(bytes: number, decimals = 2): string {
  if (bytes === 0) return "0 Bytes"

  const k = 1024
  const dm = decimals < 0 ? 0 : decimals
  const sizes = ["Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"]

  const i = Math.floor(Math.log(bytes) / Math.log(k))

  return Number.parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + " " + sizes[i]
}

export function formatDuration(ms: number): string {
  const seconds = Math.floor(ms / 1000)
  const minutes = Math.floor(seconds / 60)
  const hours = Math.floor(minutes / 60)

  if (hours > 0) {
    return `${hours}h ${minutes % 60}m ${seconds % 60}s`
  } else if (minutes > 0) {
    return `${minutes}m ${seconds % 60}s`
  } else {
    return `${seconds}s`
  }
}

export function getDeviceIcon(deviceType: string): string {
  switch (deviceType) {
    case "camera":
      return "📹"
    case "nvr":
      return "🖥️"
    case "router":
      return "🌐"
    case "switch":
      return "🔀"
    default:
      return "❓"
  }
}

export function getStatusColor(status: string): string {
  switch (status) {
    case "online":
    case "active":
    case "completed":
      return "text-green-400"
    case "offline":
    case "inactive":
    case "failed":
      return "text-red-400"
    case "testing":
    case "running":
      return "text-yellow-400"
    default:
      return "text-gray-400"
  }
}
