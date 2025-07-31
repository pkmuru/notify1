using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using STAAT_Notification.Helpers;
using STAAT_Notification.Models;
using STAAT_Notification.Services;
using STAAT_Notification.Settings;
using Application = System.Windows.Application;

namespace STAAT_Notification
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static NotificationService NotificationService { get; private set; }
        private NotifyIcon _trayIcon;
        private MainWindow _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize system tray
            InitializeTrayIcon();

            // Initialize notification service
            NotificationService = new NotificationService();
            NotificationService.NotificationStatusChanged += OnNotificationStatusChanged;

            // Create main window but don't show it (start minimized to tray)
            _mainWindow = new MainWindow();
            _mainWindow.Closing += MainWindow_Closing;
        }

        private void InitializeTrayIcon()
        {
            _trayIcon = TrayIconHelper.CreateTrayIcon();
            
            // Wire up events
            _trayIcon.MouseClick += TrayIcon_MouseClick;
            
            var contextMenu = _trayIcon.ContextMenuStrip;
            contextMenu.Items[0].Click += ShowNotifications_Click; // Show Notifications
            
            // Mute submenu
            var muteMenu = contextMenu.Items[2] as ToolStripMenuItem;
            foreach (ToolStripMenuItem item in muteMenu.DropDownItems)
            {
                item.Click += MuteOption_Click;
            }
            
            contextMenu.Items[4].Click += TurnOffNotifications_Click; // Turn off
            contextMenu.Items[6].Click += Settings_Click; // Settings
            contextMenu.Items[7].Click += Exit_Click; // Exit
        }

        private void TrayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShowNotifications_Click(sender, e);
            }
        }

        private void ShowNotifications_Click(object sender, EventArgs e)
        {
            NotificationService?.ShowTestNotifications();
        }

        private void MuteOption_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item?.Tag is MuteDuration duration)
            {
                var settings = SettingsManager.LoadSettings();
                settings.SelectedMuteDuration = duration;
                
                switch (duration)
                {
                    case MuteDuration.OneHour:
                        settings.IsMuted = true;
                        settings.MuteUntil = DateTime.Now.AddHours(1);
                        break;
                    case MuteDuration.FourHours:
                        settings.IsMuted = true;
                        settings.MuteUntil = DateTime.Now.AddHours(4);
                        break;
                    case MuteDuration.EightHours:
                        settings.IsMuted = true;
                        settings.MuteUntil = DateTime.Now.AddHours(8);
                        break;
                    case MuteDuration.UntilTomorrow:
                        settings.IsMuted = true;
                        settings.MuteUntil = DateTime.Today.AddDays(1).AddHours(9);
                        break;
                }
                
                SettingsManager.SaveSettings(settings);
                UpdateTrayIcon();
            }
        }

        private void TurnOffNotifications_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            var settings = SettingsManager.LoadSettings();
            settings.IsNotificationsEnabled = !item.Checked;
            SettingsManager.SaveSettings(settings);
            UpdateTrayIcon();
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            _mainWindow.Show();
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Activate();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            Shutdown();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Hide to tray instead of closing
            e.Cancel = true;
            _mainWindow.Hide();
        }

        private void OnNotificationStatusChanged(object sender, EventArgs e)
        {
            UpdateTrayIcon();
        }

        private void UpdateTrayIcon()
        {
            var settings = SettingsManager.LoadSettings();
            
            string status = "Active";
            if (!settings.IsNotificationsEnabled)
                status = "Disabled";
            else if (settings.IsCurrentlyMuted)
                status = settings.MuteStatusText;
                
            TrayIconHelper.UpdateTrayIcon(_trayIcon, settings.IsCurrentlyMuted || !settings.IsNotificationsEnabled, 
                                          false, status); // No longer tracking queued notifications
                                          
            // Update context menu checkmarks
            var contextMenu = _trayIcon.ContextMenuStrip;
            if (contextMenu.Items[4] is ToolStripMenuItem turnOffItem)
            {
                turnOffItem.Checked = !settings.IsNotificationsEnabled;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NotificationService?.StopPolling();
            _trayIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
