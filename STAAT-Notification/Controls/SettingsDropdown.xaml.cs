using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using STAAT_Notification.Models;
using STAAT_Notification.Settings;

namespace STAAT_Notification.Controls
{
    public partial class SettingsDropdown : UserControl
    {
        public event EventHandler<NotificationSettings> SettingsChanged;
        private NotificationSettings _settings;

        public SettingsDropdown()
        {
            InitializeComponent();
            LoadSettings();
            AttachEventHandlers();
        }

        private void LoadSettings()
        {
            _settings = SettingsManager.LoadSettings();
            
            // Set UI state based on settings
            TurnOffNotifications.IsChecked = !_settings.IsNotificationsEnabled;
            
            // Set mute duration radio button
            switch (_settings.SelectedMuteDuration)
            {
                case MuteDuration.None:
                    MuteNone.IsChecked = true;
                    break;
                case MuteDuration.OneHour:
                    MuteOneHour.IsChecked = true;
                    break;
                case MuteDuration.FourHours:
                    MuteFourHours.IsChecked = true;
                    break;
                case MuteDuration.EightHours:
                    MuteEightHours.IsChecked = true;
                    break;
                case MuteDuration.UntilTomorrow:
                    MuteUntilTomorrow.IsChecked = true;
                    break;
            }
            
            UpdateMuteStatus();
        }

        private void AttachEventHandlers()
        {
            TurnOffNotifications.Checked += OnSettingChanged;
            TurnOffNotifications.Unchecked += OnSettingChanged;
            
            MuteNone.Checked += OnMuteDurationChanged;
            MuteOneHour.Checked += OnMuteDurationChanged;
            MuteFourHours.Checked += OnMuteDurationChanged;
            MuteEightHours.Checked += OnMuteDurationChanged;
            MuteUntilTomorrow.Checked += OnMuteDurationChanged;
        }

        private void OnSettingChanged(object sender, RoutedEventArgs e)
        {
            _settings.IsNotificationsEnabled = !(TurnOffNotifications.IsChecked ?? false);
            SaveAndNotify();
        }

        private void OnMuteDurationChanged(object sender, RoutedEventArgs e)
        {
            var radioButton = sender as RadioButton;
            if (radioButton?.IsChecked == true && radioButton.Tag != null)
            {
                if (Enum.TryParse<MuteDuration>(radioButton.Tag.ToString(), out var duration))
                {
                    _settings.SelectedMuteDuration = duration;
                    
                    switch (duration)
                    {
                        case MuteDuration.None:
                            _settings.IsMuted = false;
                            _settings.MuteUntil = null;
                            break;
                        case MuteDuration.OneHour:
                            _settings.IsMuted = true;
                            _settings.MuteUntil = DateTime.Now.AddHours(1);
                            break;
                        case MuteDuration.FourHours:
                            _settings.IsMuted = true;
                            _settings.MuteUntil = DateTime.Now.AddHours(4);
                            break;
                        case MuteDuration.EightHours:
                            _settings.IsMuted = true;
                            _settings.MuteUntil = DateTime.Now.AddHours(8);
                            break;
                        case MuteDuration.UntilTomorrow:
                            _settings.IsMuted = true;
                            var tomorrow = DateTime.Today.AddDays(1).AddHours(9); // 9:00 AM tomorrow
                            _settings.MuteUntil = tomorrow;
                            break;
                    }
                    
                    SaveAndNotify();
                }
            }
        }

        private void UpdateMuteStatus()
        {
            if (_settings.IsCurrentlyMuted)
            {
                MuteStatusText.Text = _settings.MuteStatusText;
                MuteStatusText.Visibility = Visibility.Visible;
            }
            else
            {
                MuteStatusText.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveAndNotify()
        {
            SettingsManager.SaveSettings(_settings);
            UpdateMuteStatus();
            SettingsChanged?.Invoke(this, _settings);
            
            // Sync with API
            _ = SyncSettingsWithAPI();
        }

        private async System.Threading.Tasks.Task SyncSettingsWithAPI()
        {
            try
            {
                await STAAT_Notification.Services.ApiSyncService.SyncSettingsAsync(_settings);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error syncing settings: {ex.Message}");
            }
        }
    }
}