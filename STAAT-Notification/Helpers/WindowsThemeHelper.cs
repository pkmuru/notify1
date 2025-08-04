using System;
using System.Windows.Media;
using Microsoft.Win32;

namespace STAAT_Notification.Helpers
{
    public static class WindowsThemeHelper
    {
        public static Color GetAccentColor()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent"))
                {
                    if (key != null)
                    {
                        var accentPalette = key.GetValue("AccentPalette") as byte[];
                        if (accentPalette != null && accentPalette.Length >= 16)
                        {
                            // Windows accent color is at position 0-3 in the palette
                            return Color.FromRgb(accentPalette[0], accentPalette[1], accentPalette[2]);
                        }
                    }
                }

                // Fallback to DWM if registry fails
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
                {
                    if (key != null)
                    {
                        var accentColor = key.GetValue("AccentColor");
                        if (accentColor != null)
                        {
                            var colorValue = Convert.ToUInt32(accentColor);
                            return Color.FromArgb(
                                (byte)((colorValue >> 24) & 0xFF),
                                (byte)((colorValue >> 16) & 0xFF),
                                (byte)((colorValue >> 8) & 0xFF),
                                (byte)(colorValue & 0xFF)
                            );
                        }
                    }
                }
            }
            catch
            {
                // Ignore any registry access errors
            }

            // Default Windows 11 blue
            return Color.FromRgb(0, 120, 212);
        }

        public static bool IsLightTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var appsUseLightTheme = key.GetValue("AppsUseLightTheme");
                        if (appsUseLightTheme != null)
                        {
                            return Convert.ToInt32(appsUseLightTheme) == 1;
                        }
                    }
                }
            }
            catch
            {
                // Ignore registry access errors
            }

            return true; // Default to light theme
        }

        public static Color GetHoverAccentColor(Color baseAccent)
        {
            // Darken by 10% for hover
            var factor = 0.9;
            return Color.FromRgb(
                (byte)(baseAccent.R * factor),
                (byte)(baseAccent.G * factor),
                (byte)(baseAccent.B * factor)
            );
        }

        public static Color GetPressedAccentColor(Color baseAccent)
        {
            // Darken by 20% for pressed
            var factor = 0.8;
            return Color.FromRgb(
                (byte)(baseAccent.R * factor),
                (byte)(baseAccent.G * factor),
                (byte)(baseAccent.B * factor)
            );
        }
    }
}