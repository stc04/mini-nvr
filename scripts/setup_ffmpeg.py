#!/usr/bin/env python3
"""
AI-IT Inc NVR System - FFmpeg Setup Script
Automatically downloads and configures FFmpeg for video processing
Supports Windows, Linux, and Raspberry Pi platforms
"""

import os
import sys
import platform
import urllib.request
import zipfile
import tarfile
import subprocess
import shutil
from pathlib import Path

class FFmpegSetup:
    def __init__(self):
        self.system = platform.system().lower()
        self.architecture = platform.machine().lower()
        self.script_dir = Path(__file__).parent
        self.project_root = self.script_dir.parent
        self.ffmpeg_dir = self.project_root / "ffmpeg"
        
        # FFmpeg download URLs
        self.download_urls = {
            'windows': {
                'x64': 'https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip',
                'x86': 'https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip'
            },
            'linux': {
                'x86_64': 'https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz',
                'armv7l': 'https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-armhf-static.tar.xz',
                'aarch64': 'https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-arm64-static.tar.xz'
            }
        }

    def detect_platform(self):
        """Detect the current platform and architecture"""
        print(f"🔍 Detecting platform...")
        print(f"   System: {self.system}")
        print(f"   Architecture: {self.architecture}")
        
        # Check if running on Raspberry Pi
        if self.system == 'linux':
            try:
                with open('/proc/cpuinfo', 'r') as f:
                    cpuinfo = f.read()
                    if 'raspberry pi' in cpuinfo.lower() or 'bcm' in cpuinfo.lower():
                        print("   🍓 Raspberry Pi detected!")
                        return True
            except:
                pass
        
        return False

    def check_existing_ffmpeg(self):
        """Check if FFmpeg is already installed"""
        print("🔍 Checking for existing FFmpeg installation...")
        
        # Check system PATH
        try:
            result = subprocess.run(['ffmpeg', '-version'], 
                                  capture_output=True, text=True, timeout=5)
            if result.returncode == 0:
                version_line = result.stdout.split('\n')[0]
                print(f"   ✅ Found system FFmpeg: {version_line}")
                return True
        except:
            pass
        
        # Check local installation
        local_ffmpeg = self.ffmpeg_dir / ('ffmpeg.exe' if self.system == 'windows' else 'ffmpeg')
        if local_ffmpeg.exists():
            try:
                result = subprocess.run([str(local_ffmpeg), '-version'], 
                                      capture_output=True, text=True, timeout=5)
                if result.returncode == 0:
                    version_line = result.stdout.split('\n')[0]
                    print(f"   ✅ Found local FFmpeg: {version_line}")
                    return True
            except:
                pass
        
        print("   ❌ FFmpeg not found")
        return False

    def download_ffmpeg(self):
        """Download FFmpeg for the current platform"""
        print("📥 Downloading FFmpeg...")
        
        # Determine download URL
        if self.system == 'windows':
            arch_key = 'x64' if '64' in self.architecture else 'x86'
            url = self.download_urls['windows'][arch_key]
            filename = 'ffmpeg-windows.zip'
        elif self.system == 'linux':
            if 'arm' in self.architecture or 'aarch64' in self.architecture:
                if 'aarch64' in self.architecture:
                    arch_key = 'aarch64'
                else:
                    arch_key = 'armv7l'
            else:
                arch_key = 'x86_64'
            
            url = self.download_urls['linux'][arch_key]
            filename = f'ffmpeg-linux-{arch_key}.tar.xz'
        else:
            raise Exception(f"Unsupported platform: {self.system}")
        
        # Create download directory
        download_dir = self.script_dir / "downloads"
        download_dir.mkdir(exist_ok=True)
        download_path = download_dir / filename
        
        print(f"   📡 Downloading from: {url}")
        print(f"   💾 Saving to: {download_path}")
        
        try:
            # Download with progress
            def progress_hook(block_num, block_size, total_size):
                if total_size > 0:
                    percent = min(100, (block_num * block_size * 100) // total_size)
                    print(f"\r   📊 Progress: {percent}%", end='', flush=True)
            
            urllib.request.urlretrieve(url, download_path, progress_hook)
            print(f"\n   ✅ Download completed: {download_path}")
            return download_path
            
        except Exception as e:
            print(f"\n   ❌ Download failed: {e}")
            raise

    def extract_ffmpeg(self, archive_path):
        """Extract FFmpeg from downloaded archive"""
        print("📦 Extracting FFmpeg...")
        
        # Create FFmpeg directory
        self.ffmpeg_dir.mkdir(exist_ok=True)
        
        try:
            if archive_path.suffix == '.zip':
                # Windows ZIP file
                with zipfile.ZipFile(archive_path, 'r') as zip_ref:
                    # Find FFmpeg binaries in the archive
                    for file_info in zip_ref.filelist:
                        if file_info.filename.endswith(('ffmpeg.exe', 'ffprobe.exe', 'ffplay.exe')):
                            # Extract to ffmpeg directory
                            file_info.filename = os.path.basename(file_info.filename)
                            zip_ref.extract(file_info, self.ffmpeg_dir)
                            print(f"   ✅ Extracted: {file_info.filename}")
            
            elif archive_path.suffix in ['.tar', '.xz'] or '.tar.' in archive_path.name:
                # Linux TAR file
                with tarfile.open(archive_path, 'r:*') as tar_ref:
                    # Find FFmpeg binaries in the archive
                    for member in tar_ref.getmembers():
                        if member.name.endswith(('ffmpeg', 'ffprobe', 'ffplay')) and member.isfile():
                            # Extract to ffmpeg directory
                            member.name = os.path.basename(member.name)
                            tar_ref.extract(member, self.ffmpeg_dir)
                            
                            # Make executable on Linux
                            extracted_path = self.ffmpeg_dir / member.name
                            extracted_path.chmod(0o755)
                            print(f"   ✅ Extracted: {member.name}")
            
            print("   🎉 Extraction completed!")
            
        except Exception as e:
            print(f"   ❌ Extraction failed: {e}")
            raise

    def install_system_dependencies(self):
        """Install system dependencies for video processing"""
        print("📦 Installing system dependencies...")
        
        if self.system == 'linux':
            # Check if we're on a Debian/Ubuntu system
            try:
                subprocess.run(['apt', '--version'], check=True, capture_output=True)
                
                print("   🔧 Installing codec libraries...")
                dependencies = [
                    'libx264-dev', 'libx265-dev', 'libvpx-dev', 'libfdk-aac-dev',
                    'libmp3lame-dev', 'libopus-dev', 'libvorbis-dev', 'libtheora-dev',
                    'libva-dev', 'libvdpau-dev'  # Hardware acceleration
                ]
                
                for dep in dependencies:
                    try:
                        result = subprocess.run(['sudo', 'apt', 'install', '-y', dep], 
                                              capture_output=True, text=True)
                        if result.returncode == 0:
                            print(f"   ✅ Installed: {dep}")
                        else:
                            print(f"   ⚠️  Failed to install: {dep}")
                    except:
                        print(f"   ⚠️  Could not install: {dep}")
                        
            except subprocess.CalledProcessError:
                print("   ℹ️  Not a Debian/Ubuntu system, skipping apt packages")
                
        elif self.system == 'windows':
            print("   ℹ️  Windows: Using static FFmpeg build (no additional dependencies needed)")

    def configure_gpu_acceleration(self):
        """Configure GPU acceleration if available"""
        print("🚀 Configuring GPU acceleration...")
        
        gpu_config = {
            'nvidia': False,
            'intel': False,
            'amd': False
        }
        
        if self.system == 'linux':
            # Check for NVIDIA GPU
            try:
                result = subprocess.run(['nvidia-smi'], capture_output=True, text=True)
                if result.returncode == 0:
                    gpu_config['nvidia'] = True
                    print("   🎮 NVIDIA GPU detected - NVENC/NVDEC available")
            except:
                pass
            
            # Check for Intel GPU
            try:
                result = subprocess.run(['lspci'], capture_output=True, text=True)
                if 'intel' in result.stdout.lower() and 'vga' in result.stdout.lower():
                    gpu_config['intel'] = True
                    print("   💻 Intel GPU detected - QuickSync available")
            except:
                pass
                
        elif self.system == 'windows':
            # Windows GPU detection would require more complex WMI queries
            print("   💻 Windows: GPU acceleration will be auto-detected by FFmpeg")
            gpu_config['nvidia'] = True  # Assume available
            gpu_config['intel'] = True
        
        # Save GPU configuration
        config_file = self.ffmpeg_dir / 'gpu_config.txt'
        with open(config_file, 'w') as f:
            for gpu, available in gpu_config.items():
                f.write(f"{gpu}={available}\n")
        
        print(f"   💾 GPU configuration saved to: {config_file}")

    def test_ffmpeg_installation(self):
        """Test the FFmpeg installation"""
        print("🧪 Testing FFmpeg installation...")
        
        ffmpeg_path = self.ffmpeg_dir / ('ffmpeg.exe' if self.system == 'windows' else 'ffmpeg')
        
        if not ffmpeg_path.exists():
            print("   ❌ FFmpeg binary not found!")
            return False
        
        try:
            # Test basic functionality
            result = subprocess.run([str(ffmpeg_path), '-version'], 
                                  capture_output=True, text=True, timeout=10)
            
            if result.returncode == 0:
                version_info = result.stdout.split('\n')[0]
                print(f"   ✅ FFmpeg is working: {version_info}")
                
                # Test codec support
                codecs_result = subprocess.run([str(ffmpeg_path), '-codecs'], 
                                             capture_output=True, text=True, timeout=5)
                
                important_codecs = ['h264', 'h265', 'vp8', 'vp9', 'aac', 'mp3']
                supported_codecs = []
                
                for codec in important_codecs:
                    if codec in codecs_result.stdout.lower():
                        supported_codecs.append(codec)
                
                print(f"   🎬 Supported codecs: {', '.join(supported_codecs)}")
                
                # Test hardware acceleration
                hwaccels_result = subprocess.run([str(ffmpeg_path), '-hwaccels'], 
                                               capture_output=True, text=True, timeout=5)
                
                if 'cuda' in hwaccels_result.stdout or 'nvenc' in hwaccels_result.stdout:
                    print("   🚀 NVIDIA hardware acceleration available")
                if 'qsv' in hwaccels_result.stdout:
                    print("   🚀 Intel QuickSync acceleration available")
                if 'vaapi' in hwaccels_result.stdout:
                    print("   🚀 VA-API acceleration available")
                
                return True
            else:
                print(f"   ❌ FFmpeg test failed: {result.stderr}")
                return False
                
        except Exception as e:
            print(f"   ❌ FFmpeg test error: {e}")
            return False

    def create_wrapper_scripts(self):
        """Create wrapper scripts for easy FFmpeg access"""
        print("📝 Creating wrapper scripts...")
        
        # Create batch/shell script for easy access
        if self.system == 'windows':
            wrapper_path = self.project_root / 'ffmpeg.bat'
            with open(wrapper_path, 'w') as f:
                f.write(f'@echo off\n')
                f.write(f'"{self.ffmpeg_dir}\\ffmpeg.exe" %*\n')
            print(f"   ✅ Created: {wrapper_path}")
            
        else:
            wrapper_path = self.project_root / 'ffmpeg.sh'
            with open(wrapper_path, 'w') as f:
                f.write(f'#!/bin/bash\n')
                f.write(f'"{self.ffmpeg_dir}/ffmpeg" "$@"\n')
            wrapper_path.chmod(0o755)
            print(f"   ✅ Created: {wrapper_path}")

    def setup(self):
        """Main setup process"""
        print("🎬 AI-IT Inc NVR - FFmpeg Setup")
        print("=" * 50)
        
        try:
            # Detect platform
            is_raspberry_pi = self.detect_platform()
            
            # Check if FFmpeg already exists
            if self.check_existing_ffmpeg():
                print("✅ FFmpeg is already installed and working!")
                
                # Still configure GPU acceleration
                self.configure_gpu_acceleration()
                return True
            
            # Install system dependencies
            self.install_system_dependencies()
            
            # Download FFmpeg
            archive_path = self.download_ffmpeg()
            
            # Extract FFmpeg
            self.extract_ffmpeg(archive_path)
            
            # Configure GPU acceleration
            self.configure_gpu_acceleration()
            
            # Test installation
            if self.test_ffmpeg_installation():
                # Create wrapper scripts
                self.create_wrapper_scripts()
                
                print("\n🎉 FFmpeg setup completed successfully!")
                print(f"   📁 Installation directory: {self.ffmpeg_dir}")
                print("   🚀 Ready for AI-IT Inc NVR video processing!")
                
                # Cleanup
                if archive_path.exists():
                    archive_path.unlink()
                    print(f"   🧹 Cleaned up download: {archive_path}")
                
                return True
            else:
                print("\n❌ FFmpeg setup failed!")
                return False
                
        except Exception as e:
            print(f"\n💥 Setup failed with error: {e}")
            return False

def main():
    """Main entry point"""
    setup = FFmpegSetup()
    success = setup.setup()
    
    if success:
        print("\n🎊 Setup completed! You can now run the AI-IT Inc NVR system.")
        sys.exit(0)
    else:
        print("\n💔 Setup failed. Please check the errors above and try again.")
        sys.exit(1)

if __name__ == "__main__":
    main()
