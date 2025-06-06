#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
AI-IT Inc NVR Kodi Addon
Professional security monitoring integration for Kodi

Copyright (C) 2025 AI-IT Inc
Licensed under GPL-3.0
"""

import sys
import urllib.parse as urlparse
import xbmc
import xbmcgui
import xbmcplugin
import xbmcaddon
import xbmcvfs
import json
import requests
from datetime import datetime, timedelta

# Addon information
ADDON = xbmcaddon.Addon()
ADDON_ID = ADDON.getAddonInfo('id')
ADDON_NAME = ADDON.getAddonInfo('name')
ADDON_VERSION = ADDON.getAddonInfo('version')
ADDON_PATH = ADDON.getAddonInfo('path')
ADDON_PROFILE = xbmcvfs.translatePath(ADDON.getAddonInfo('profile'))

# Plugin handle
HANDLE = int(sys.argv[1])

# NVR API endpoints
class NVRApi:
    def __init__(self):
        self.host = ADDON.getSetting('nvr_host') or 'localhost'
        self.port = ADDON.getSetting('nvr_port') or '8080'
        self.username = ADDON.getSetting('nvr_username') or 'admin'
        self.password = ADDON.getSetting('nvr_password') or 'admin123'
        self.use_https = ADDON.getSettingBool('use_https')
        
        protocol = 'https' if self.use_https else 'http'
        self.base_url = f"{protocol}://{self.host}:{self.port}/api"
        
        self.session = requests.Session()
        self.session.auth = (self.username, self.password)
        self.session.timeout = 10
        
    def test_connection(self):
        """Test connection to NVR system"""
        try:
            response = self.session.get(f"{self.base_url}/status")
            return response.status_code == 200
        except Exception as e:
            xbmc.log(f"[{ADDON_ID}] Connection test failed: {str(e)}", xbmc.LOGERROR)
            return False
    
    def get_cameras(self):
        """Get list of available cameras"""
        try:
            response = self.session.get(f"{self.base_url}/cameras")
            if response.status_code == 200:
                return response.json()
            return []
        except Exception as e:
            xbmc.log(f"[{ADDON_ID}] Failed to get cameras: {str(e)}", xbmc.LOGERROR)
            return []
    
    def get_camera_stream_url(self, camera_id, quality='medium'):
        """Get streaming URL for camera"""
        quality_map = {
            'low': '480p',
            'medium': '720p', 
            'high': '1080p',
            'ultra': '4k'
        }
        resolution = quality_map.get(quality, '720p')
        return f"{self.base_url}/cameras/{camera_id}/stream?quality={resolution}"
    
    def get_recordings(self, camera_id=None, start_date=None, end_date=None):
        """Get list of recordings"""
        try:
            params = {}
            if camera_id:
                params['camera_id'] = camera_id
            if start_date:
                params['start_date'] = start_date
            if end_date:
                params['end_date'] = end_date
                
            response = self.session.get(f"{self.base_url}/recordings", params=params)
            if response.status_code == 200:
                return response.json()
            return []
        except Exception as e:
            xbmc.log(f"[{ADDON_ID}] Failed to get recordings: {str(e)}", xbmc.LOGERROR)
            return []
    
    def get_motion_events(self, camera_id=None, limit=50):
        """Get motion detection events"""
        try:
            params = {'limit': limit}
            if camera_id:
                params['camera_id'] = camera_id
                
            response = self.session.get(f"{self.base_url}/motion-events", params=params)
            if response.status_code == 200:
                return response.json()
            return []
        except Exception as e:
            xbmc.log(f"[{ADDON_ID}] Failed to get motion events: {str(e)}", xbmc.LOGERROR)
            return []
    
    def ptz_control(self, camera_id, command, value=None):
        """Control PTZ camera"""
        try:
            data = {'command': command}
            if value:
                data['value'] = value
                
            response = self.session.post(f"{self.base_url}/cameras/{camera_id}/ptz", json=data)
            return response.status_code == 200
        except Exception as e:
            xbmc.log(f"[{ADDON_ID}] PTZ control failed: {str(e)}", xbmc.LOGERROR)
            return False
    
    def take_snapshot(self, camera_id):
        """Take snapshot from camera"""
        try:
            response = self.session.post(f"{self.base_url}/cameras/{camera_id}/snapshot")
            return response.status_code == 200
        except Exception as e:
            xbmc.log(f"[{ADDON_ID}] Snapshot failed: {str(e)}", xbmc.LOGERROR)
            return False

# Initialize NVR API
nvr_api = NVRApi()

def build_url(query):
    """Build plugin URL with query parameters"""
    return f"{sys.argv[0]}?{urlparse.urlencode(query)}"

def add_directory_item(title, url, is_folder=True, info_labels=None, art=None, context_menu=None):
    """Add directory item to Kodi"""
    list_item = xbmcgui.ListItem(label=title)
    
    if info_labels:
        list_item.setInfo('video', info_labels)
    
    if art:
        list_item.setArt(art)
    
    if context_menu:
        list_item.addContextMenuItems(context_menu)
    
    xbmcplugin.addDirectoryItem(HANDLE, url, list_item, is_folder)

def show_notification(message, title=ADDON_NAME, icon=xbmcgui.NOTIFICATION_INFO, time=5000):
    """Show notification to user"""
    xbmcgui.Dialog().notification(title, message, icon, time)

def main_menu():
    """Display main menu"""
    # Test connection first
    if not nvr_api.test_connection():
        show_notification("Cannot connect to NVR system. Check settings.", icon=xbmcgui.NOTIFICATION_ERROR)
        xbmcplugin.endOfDirectory(HANDLE, False)
        return
    
    # Main menu items
    menu_items = [
        ("📹 Live Cameras", build_url({'mode': 'live_cameras'}), True),
        ("📼 Recordings", build_url({'mode': 'recordings'}), True),
        ("🚨 Motion Events", build_url({'mode': 'motion_events'}), True),
        ("📸 Snapshots", build_url({'mode': 'snapshots'}), True),
        ("🔲 Multi-Camera Grid", build_url({'mode': 'grid_view'}), False),
        ("⚙️ Settings", build_url({'mode': 'settings'}), True),
    ]
    
    for title, url, is_folder in menu_items:
        add_directory_item(title, url, is_folder)
    
    xbmcplugin.setContent(HANDLE, 'videos')
    xbmcplugin.endOfDirectory(HANDLE)

def show_live_cameras():
    """Display live cameras"""
    cameras = nvr_api.get_cameras()
    
    if not cameras:
        show_notification("No cameras found")
        xbmcplugin.endOfDirectory(HANDLE, False)
        return
    
    for camera in cameras:
        camera_id = camera.get('id')
        camera_name = camera.get('name', f"Camera {camera_id}")
        camera_status = camera.get('status', 'unknown')
        
        # Status indicator
        status_icon = "🟢" if camera_status == 'online' else "🔴"
        title = f"{status_icon} {camera_name}"
        
        # Stream URL
        quality = ADDON.getSetting('stream_quality') or 'medium'
        stream_url = nvr_api.get_camera_stream_url(camera_id, quality)
        
        # Context menu for PTZ and snapshot
        context_menu = []
        if camera.get('ptz_capable'):
            context_menu.extend([
                ("PTZ Control", f"RunPlugin({build_url({'mode': 'ptz_control', 'camera_id': camera_id})})"),
            ])
        context_menu.append(
            ("Take Snapshot", f"RunPlugin({build_url({'mode': 'take_snapshot', 'camera_id': camera_id})})")
        )
        
        # Info labels
        info_labels = {
            'title': camera_name,
            'plot': f"Live stream from {camera_name}\nStatus: {camera_status}",
            'mediatype': 'video'
        }
        
        # Art
        art = {
            'thumb': camera.get('thumbnail_url', ''),
            'fanart': camera.get('thumbnail_url', '')
        }
        
        add_directory_item(title, stream_url, False, info_labels, art, context_menu)
    
    xbmcplugin.setContent(HANDLE, 'videos')
    xbmcplugin.endOfDirectory(HANDLE)

def show_recordings():
    """Display recordings menu"""
    menu_items = [
        ("📅 Today", build_url({'mode': 'recordings_date', 'date': 'today'})),
        ("📅 Yesterday", build_url({'mode': 'recordings_date', 'date': 'yesterday'})),
        ("📅 This Week", build_url({'mode': 'recordings_date', 'date': 'week'})),
        ("📅 This Month", build_url({'mode': 'recordings_date', 'date': 'month'})),
        ("📅 Custom Date", build_url({'mode': 'recordings_custom_date'})),
        ("📹 By Camera", build_url({'mode': 'recordings_by_camera'})),
    ]
    
    for title, url in menu_items:
        add_directory_item(title, url, True)
    
    xbmcplugin.endOfDirectory(HANDLE)

def show_recordings_by_date(date_filter):
    """Show recordings filtered by date"""
    # Calculate date range
    now = datetime.now()
    if date_filter == 'today':
        start_date = now.replace(hour=0, minute=0, second=0, microsecond=0)
        end_date = now
    elif date_filter == 'yesterday':
        yesterday = now - timedelta(days=1)
        start_date = yesterday.replace(hour=0, minute=0, second=0, microsecond=0)
        end_date = yesterday.replace(hour=23, minute=59, second=59, microsecond=999999)
    elif date_filter == 'week':
        start_date = now - timedelta(days=7)
        end_date = now
    elif date_filter == 'month':
        start_date = now - timedelta(days=30)
        end_date = now
    else:
        start_date = None
        end_date = None
    
    # Get recordings
    recordings = nvr_api.get_recordings(
        start_date=start_date.isoformat() if start_date else None,
        end_date=end_date.isoformat() if end_date else None
    )
    
    if not recordings:
        show_notification("No recordings found for selected period")
        xbmcplugin.endOfDirectory(HANDLE, False)
        return
    
    for recording in recordings:
        camera_name = recording.get('camera_name', 'Unknown Camera')
        start_time = recording.get('start_time', '')
        duration = recording.get('duration', 0)
        file_size = recording.get('file_size', 0)
        
        # Format duration
        duration_str = f"{duration // 60}:{duration % 60:02d}"
        
        # Format file size
        if file_size > 1024 * 1024 * 1024:
            size_str = f"{file_size / (1024 * 1024 * 1024):.1f} GB"
        elif file_size > 1024 * 1024:
            size_str = f"{file_size / (1024 * 1024):.1f} MB"
        else:
            size_str = f"{file_size / 1024:.1f} KB"
        
        title = f"{camera_name} - {start_time} ({duration_str})"
        
        info_labels = {
            'title': title,
            'plot': f"Recording from {camera_name}\nDuration: {duration_str}\nSize: {size_str}",
            'duration': duration,
            'mediatype': 'video'
        }
        
        playback_url = recording.get('playback_url', '')
        add_directory_item(title, playback_url, False, info_labels)
    
    xbmcplugin.setContent(HANDLE, 'videos')
    xbmcplugin.endOfDirectory(HANDLE)

def show_motion_events():
    """Display motion detection events"""
    events = nvr_api.get_motion_events()
    
    if not events:
        show_notification("No motion events found")
        xbmcplugin.endOfDirectory(HANDLE, False)
        return
    
    for event in events:
        camera_name = event.get('camera_name', 'Unknown Camera')
        timestamp = event.get('timestamp', '')
        confidence = event.get('confidence', 0)
        
        # Confidence indicator
        if confidence >= 80:
            confidence_icon = "🔴"  # High confidence
        elif confidence >= 60:
            confidence_icon = "🟡"  # Medium confidence
        else:
            confidence_icon = "🟢"  # Low confidence
        
        title = f"{confidence_icon} {camera_name} - {timestamp} ({confidence}%)"
        
        info_labels = {
            'title': title,
            'plot': f"Motion detected on {camera_name}\nConfidence: {confidence}%\nTime: {timestamp}",
            'mediatype': 'video'
        }
        
        # Link to recording if available
        recording_url = event.get('recording_url', '')
        if recording_url:
            add_directory_item(title, recording_url, False, info_labels)
        else:
            add_directory_item(title, "", True, info_labels)
    
    xbmcplugin.setContent(HANDLE, 'videos')
    xbmcplugin.endOfDirectory(HANDLE)

def show_grid_view():
    """Display multi-camera grid view"""
    cameras = nvr_api.get_cameras()
    online_cameras = [cam for cam in cameras if cam.get('status') == 'online']
    
    if len(online_cameras) < 2:
        show_notification("Need at least 2 online cameras for grid view")
        return
    
    # Create playlist for grid view
    playlist = xbmc.PlayList(xbmc.PLAYLIST_VIDEO)
    playlist.clear()
    
    quality = ADDON.getSetting('grid_quality') or 'low'  # Lower quality for grid
    
    for camera in online_cameras[:9]:  # Max 9 cameras in 3x3 grid
        camera_id = camera.get('id')
        camera_name = camera.get('name', f"Camera {camera_id}")
        stream_url = nvr_api.get_camera_stream_url(camera_id, quality)
        
        list_item = xbmcgui.ListItem(label=camera_name)
        list_item.setInfo('video', {'title': camera_name})
        playlist.add(stream_url, list_item)
    
    # Start playback
    xbmc.Player().play(playlist)

def ptz_control_menu(camera_id):
    """Show PTZ control menu"""
    dialog = xbmcgui.Dialog()
    
    options = [
        "Pan Left",
        "Pan Right", 
        "Tilt Up",
        "Tilt Down",
        "Zoom In",
        "Zoom Out",
        "Home Position"
    ]
    
    selected = dialog.select("PTZ Control", options)
    
    if selected >= 0:
        commands = ['pan_left', 'pan_right', 'tilt_up', 'tilt_down', 'zoom_in', 'zoom_out', 'home']
        command = commands[selected]
        
        if nvr_api.ptz_control(camera_id, command):
            show_notification(f"PTZ command sent: {options[selected]}")
        else:
            show_notification("PTZ command failed", icon=xbmcgui.NOTIFICATION_ERROR)

def take_snapshot(camera_id):
    """Take snapshot from camera"""
    if nvr_api.take_snapshot(camera_id):
        show_notification("Snapshot captured successfully")
    else:
        show_notification("Snapshot failed", icon=xbmcgui.NOTIFICATION_ERROR)

def open_settings():
    """Open addon settings"""
    ADDON.openSettings()

def router(paramstring):
    """Route addon calls"""
    params = dict(urlparse.parse_qsl(paramstring))
    mode = params.get('mode')
    
    if mode is None:
        main_menu()
    elif mode == 'live_cameras':
        show_live_cameras()
    elif mode == 'recordings':
        show_recordings()
    elif mode == 'recordings_date':
        show_recordings_by_date(params.get('date'))
    elif mode == 'recordings_by_camera':
        # TODO: Implement recordings by camera
        show_recordings()
    elif mode == 'motion_events':
        show_motion_events()
    elif mode == 'grid_view':
        show_grid_view()
    elif mode == 'ptz_control':
        ptz_control_menu(params.get('camera_id'))
    elif mode == 'take_snapshot':
        take_snapshot(params.get('camera_id'))
    elif mode == 'settings':
        open_settings()
    else:
        main_menu()

if __name__ == '__main__':
    # Log addon start
    xbmc.log(f"[{ADDON_ID}] Starting AI-IT Inc NVR addon v{ADDON_VERSION}", xbmc.LOGINFO)
    
    # Route the call
    router(sys.argv[2][1:])
