using System;
using Microsoft.Win32;
using STAAT_Notification.Models;
using STAAT_Notification.Services;

namespace STAAT_Notification.Settings
{
    public static class SettingsManager
    {
        private const string REGISTRY_KEY_PATH = @"SOFTWARE\STAAT\Notifications";
        
        public static NotificationSettings LoadSettings()
        {
            var settings = new NotificationSettings();
            
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    if (key != null)
                    {
                        settings.IsNotificationsEnabled = Convert.ToBoolean(key.GetValue("IsEnabled", 1));
                        settings.IsMuted = Convert.ToBoolean(key.GetValue("IsMuted", 0));
                        
                        var muteUntilStr = key.GetValue("MuteUntil") as string;
                        if (!string.IsNullOrEmpty(muteUntilStr) && DateTime.TryParse(muteUntilStr, out var muteUntil))
                        {
                            settings.MuteUntil = muteUntil;
                        }
                        
                        var muteDuration = key.GetValue("MuteDuration", 0);
                        if (muteDuration != null)
                        {
                            settings.SelectedMuteDuration = (MuteDuration)Convert.ToInt32(muteDuration);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error or handle as needed
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
            
            return settings;
        }
        
        public static void SaveSettings(NotificationSettings settings)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    if (key != null)
                    {
                        key.SetValue("IsEnabled", settings.IsNotificationsEnabled ? 1 : 0);
                        key.SetValue("IsMuted", settings.IsMuted ? 1 : 0);
                        key.SetValue("MuteDuration", (int)settings.SelectedMuteDuration);
                        
                        if (settings.MuteUntil.HasValue)
                        {
                            key.SetValue("MuteUntil", settings.MuteUntil.Value.ToString("O"));
                        }
                        else
                        {
                            key.DeleteValue("MuteUntil", false);
                        }
                    }
                }
                
                // Sync with API asynchronously
                _ = ApiSyncService.SyncSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                // Log error or handle as needed
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }
        
        public static double GetPollingInterval()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY_PATH))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("PollingInterval", 24);
                        return Convert.ToDouble(value);
                    }
                }
            }
            catch { }
            
            return 24; // Default to 24 hours
        }
        
        public static void SetPollingInterval(double hours)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY_PATH))
                {
                    key?.SetValue("PollingInterval", hours);
                }
            }
            catch { }
        }
    }
}