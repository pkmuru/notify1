using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using STAAT_Notification.Models;
using STAAT_Notification.Settings;
using STAAT_Notification.Views;

namespace STAAT_Notification.Services
{
    public class NotificationService
    {
        private readonly MockBackendService _backendService;
        private Timer _pollingTimer;
        private Timer _muteCheckTimer;
        private NotificationWindow _currentWindow;
        private double _pollingIntervalHours = 24;
        
        public event EventHandler NotificationStatusChanged;

        public NotificationService()
        {
            _backendService = new MockBackendService();
            
            // Check mute status every minute
            _muteCheckTimer = new Timer(60000); // 1 minute
            _muteCheckTimer.Elapsed += CheckMuteStatus;
            _muteCheckTimer.Start();
        }

        public void StartPolling(double intervalHours = 24)
        {
            _pollingIntervalHours = intervalHours;
            
            // Initial check
            Task.Run(async () => await CheckForNotifications());

            // Setup timer for periodic checks
            _pollingTimer = new Timer(intervalHours * 60 * 60 * 1000); // Convert hours to milliseconds
            _pollingTimer.Elapsed += async (sender, e) => await CheckForNotifications();
            _pollingTimer.Start();
        }

        public void StopPolling()
        {
            _pollingTimer?.Stop();
            _pollingTimer?.Dispose();
            _muteCheckTimer?.Stop();
            _muteCheckTimer?.Dispose();
        }

        private async Task CheckForNotifications()
        {
            try
            {
                var response = await _backendService.GetNotificationsAsync();
                
                if (response?.Notifications != null && response.Notifications.Any())
                {
                    var settings = SettingsManager.LoadSettings();
                    
                    // Check if notifications are completely disabled
                    if (!settings.IsNotificationsEnabled)
                    {
                        return; // Don't show anything if turned off
                    }
                    
                    // If muted, just skip showing notifications (don't queue)
                    if (settings.IsCurrentlyMuted)
                    {
                        return;
                    }
                    
                    // Show notifications immediately
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ShowNotifications(new ObservableCollection<Notification>(response.Notifications));
                    });
                }
            }
            catch (Exception ex)
            {
                // Log error in production
                Console.WriteLine($"Error checking notifications: {ex.Message}");
            }
        }

        private void ShowNotifications(ObservableCollection<Notification> notifications)
        {
            // Close existing window if any
            if (_currentWindow != null && _currentWindow.IsLoaded)
            {
                _currentWindow.Close();
            }

            _currentWindow = new NotificationWindow();
            _currentWindow.LoadNotifications(notifications);
            _currentWindow.Closed += (s, e) => _currentWindow = null;
            _currentWindow.Show();
        }

        public void ShowTestNotifications()
        {
            // Test notifications should always show, bypassing mute settings
            Task.Run(async () =>
            {
                try
                {
                    var response = await _backendService.GetNotificationsAsync();
                    
                    if (response?.Notifications != null && response.Notifications.Any())
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ShowNotifications(new ObservableCollection<Notification>(response.Notifications));
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error showing test notifications: {ex.Message}");
                }
            });
        }

        private void CheckMuteStatus(object sender, ElapsedEventArgs e)
        {
            var settings = SettingsManager.LoadSettings();
            
            // Check if mute period has expired
            if (settings.IsMuted && settings.MuteUntil.HasValue && DateTime.Now >= settings.MuteUntil.Value)
            {
                // Unmute automatically
                settings.IsMuted = false;
                settings.MuteUntil = null;
                settings.SelectedMuteDuration = MuteDuration.None;
                SettingsManager.SaveSettings(settings);
                
                NotificationStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}