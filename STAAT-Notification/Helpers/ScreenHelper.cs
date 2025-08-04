using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace STAAT_Notification.Helpers
{
    public static class ScreenHelper
    {
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, DpiType dpiType, out uint dpiX, out uint dpiY);

        private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;
        private const uint MONITOR_DEFAULTTOPRIMARY = 0x00000001;

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private enum DpiType
        {
            Effective = 0,
            Angular = 1,
            Raw = 2
        }

        public class MonitorInfo
        {
            public Rect Bounds { get; set; }
            public Rect WorkingArea { get; set; }
            public double DpiScale { get; set; }
            public bool IsPrimary { get; set; }
            public IntPtr Handle { get; set; }
        }

        public static List<MonitorInfo> GetAllMonitors()
        {
            var monitors = new List<MonitorInfo>();
            
            EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                var info = new MONITORINFO { cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO)) };
                if (GetMonitorInfo(hMonitor, ref info))
                {
                    var dpiScale = GetDpiScaleForMonitor(hMonitor);
                    
                    monitors.Add(new MonitorInfo
                    {
                        Handle = hMonitor,
                        Bounds = new Rect(info.rcMonitor.left, info.rcMonitor.top, 
                            info.rcMonitor.right - info.rcMonitor.left, 
                            info.rcMonitor.bottom - info.rcMonitor.top),
                        WorkingArea = new Rect(info.rcWork.left, info.rcWork.top, 
                            info.rcWork.right - info.rcWork.left, 
                            info.rcWork.bottom - info.rcWork.top),
                        DpiScale = dpiScale,
                        IsPrimary = (info.dwFlags & 1) != 0
                    });
                }
                return true;
            }, IntPtr.Zero);
            
            return monitors;
        }

        public static MonitorInfo GetMonitorFromWindow(Window window)
        {
            var windowInteropHelper = new WindowInteropHelper(window);
            var handle = windowInteropHelper.Handle;
            
            if (handle == IntPtr.Zero)
            {
                // Window not yet created, get primary monitor
                return GetPrimaryMonitor();
            }
            
            var hMonitor = MonitorFromWindow(handle, MONITOR_DEFAULTTONEAREST);
            return GetMonitorInfo(hMonitor);
        }

        public static MonitorInfo GetPrimaryMonitor()
        {
            var monitors = GetAllMonitors();
            return monitors.FirstOrDefault(m => m.IsPrimary) ?? monitors.FirstOrDefault();
        }

        public static MonitorInfo GetMonitorAtPoint(Point point)
        {
            var monitors = GetAllMonitors();
            return monitors.FirstOrDefault(m => m.Bounds.Contains(point)) ?? GetPrimaryMonitor();
        }

        private static MonitorInfo GetMonitorInfo(IntPtr hMonitor)
        {
            var info = new MONITORINFO { cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO)) };
            if (GetMonitorInfo(hMonitor, ref info))
            {
                var dpiScale = GetDpiScaleForMonitor(hMonitor);
                
                return new MonitorInfo
                {
                    Handle = hMonitor,
                    Bounds = new Rect(info.rcMonitor.left, info.rcMonitor.top, 
                        info.rcMonitor.right - info.rcMonitor.left, 
                        info.rcMonitor.bottom - info.rcMonitor.top),
                    WorkingArea = new Rect(info.rcWork.left, info.rcWork.top, 
                        info.rcWork.right - info.rcWork.left, 
                        info.rcWork.bottom - info.rcWork.top),
                    DpiScale = dpiScale,
                    IsPrimary = (info.dwFlags & 1) != 0
                };
            }
            
            return GetPrimaryMonitor();
        }

        private static double GetDpiScaleForMonitor(IntPtr hMonitor)
        {
            try
            {
                if (GetDpiForMonitor(hMonitor, DpiType.Effective, out uint dpiX, out uint dpiY) == 0)
                {
                    return dpiX / 96.0;
                }
            }
            catch
            {
                // GetDpiForMonitor may not be available on older Windows versions
            }
            
            return 1.0;
        }

        public static Point GetNotificationPosition(Window window, MonitorInfo monitor)
        {
            // Get the actual window size (these are already in logical pixels)
            var windowWidth = window.Width;
            var windowHeight = window.Height;
            
            // Windows 11 style padding (in logical pixels)
            const double horizontalPadding = 24;
            const double verticalPadding = 24;
            
            // The working area from Windows API is in physical pixels
            // We need to work in logical pixels for WPF positioning
            var workingAreaLogical = new Rect(
                monitor.WorkingArea.X / monitor.DpiScale,
                monitor.WorkingArea.Y / monitor.DpiScale,
                monitor.WorkingArea.Width / monitor.DpiScale,
                monitor.WorkingArea.Height / monitor.DpiScale
            );
            
            // Calculate position in logical pixels
            var left = workingAreaLogical.Right - windowWidth - horizontalPadding;
            var top = workingAreaLogical.Bottom - windowHeight - verticalPadding;
            
            // Ensure window stays within working area bounds
            left = Math.Max(workingAreaLogical.Left + horizontalPadding, left);
            top = Math.Max(workingAreaLogical.Top + verticalPadding, top);
            
            return new Point(left, top);
        }
        
        public static TaskbarPosition GetTaskbarPosition(MonitorInfo monitor)
        {
            var screenBounds = monitor.Bounds;
            var workingArea = monitor.WorkingArea;
            
            // Calculate the differences
            var leftDiff = workingArea.Left - screenBounds.Left;
            var topDiff = workingArea.Top - screenBounds.Top;
            var rightDiff = screenBounds.Right - workingArea.Right;
            var bottomDiff = screenBounds.Bottom - workingArea.Bottom;
            
            // Find the largest difference
            var maxDiff = Math.Max(Math.Max(leftDiff, topDiff), Math.Max(rightDiff, bottomDiff));
            
            if (maxDiff == bottomDiff && bottomDiff > 0)
                return TaskbarPosition.Bottom;
            else if (maxDiff == topDiff && topDiff > 0)
                return TaskbarPosition.Top;
            else if (maxDiff == leftDiff && leftDiff > 0)
                return TaskbarPosition.Left;
            else if (maxDiff == rightDiff && rightDiff > 0)
                return TaskbarPosition.Right;
            else
                return TaskbarPosition.Bottom; // Default
        }
        
        public enum TaskbarPosition
        {
            Bottom,
            Top,
            Left,
            Right
        }
    }
}