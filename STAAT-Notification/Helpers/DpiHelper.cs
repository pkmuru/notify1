using System;
using System.Windows;
using System.Windows.Media;

namespace STAAT_Notification.Helpers
{
    public static class DpiHelper
    {
        public static double GetDpiScale(Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1.0;
        }

        public static double GetSystemDpi()
        {
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            if (dpiXProperty != null)
            {
                var dpiX = (int)dpiXProperty.GetValue(null, null);
                return dpiX / 96.0;
            }
            
            return 1.0;
        }

        public static Point ScalePoint(Point point, double dpiScale)
        {
            return new Point(point.X * dpiScale, point.Y * dpiScale);
        }

        public static Size ScaleSize(Size size, double dpiScale)
        {
            return new Size(size.Width * dpiScale, size.Height * dpiScale);
        }
    }
}