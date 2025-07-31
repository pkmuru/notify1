using STAAT_Notification.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STAAT_Notification
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isPolling = false;
        private System.Windows.Threading.DispatcherTimer _statusTimer;

        public MainWindow()
        {
            InitializeComponent();
            LoadSettings();
            
            // Update status every 30 seconds
            _statusTimer = new System.Windows.Threading.DispatcherTimer();
            _statusTimer.Interval = TimeSpan.FromSeconds(30);
            _statusTimer.Tick += (s, e) => UpdateMuteStatus();
            _statusTimer.Start();
        }

        private void LoadSettings()
        {
            var pollingInterval = SettingsManager.GetPollingInterval();
            PollingIntervalTextBox.Text = pollingInterval.ToString();
            UpdateMuteStatus();
        }

        private void UpdateMuteStatus()
        {
            var settings = SettingsManager.LoadSettings();
            
            if (!settings.IsNotificationsEnabled)
            {
                StatusTextBlock.Text = "Notifications are turned off";
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red);
            }
            else if (settings.IsCurrentlyMuted)
            {
                StatusTextBlock.Text = $"Muted: {settings.MuteStatusText}";
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
            }
            else if (_isPolling)
            {
                StatusTextBlock.Text = $"Polling every {PollingIntervalTextBox.Text} hour(s)";
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Green);
            }
            else
            {
                StatusTextBlock.Text = "Not started";
                StatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(95, 99, 104));
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(PollingIntervalTextBox.Text, out double hours) && hours > 0)
            {
                SettingsManager.SetPollingInterval(hours);
                App.NotificationService.StartPolling(hours);
                _isPolling = true;
                UpdateUI();
                UpdateMuteStatus();
            }
            else
            {
                MessageBox.Show("Please enter a valid polling interval in hours.", "Invalid Input", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            App.NotificationService.StopPolling();
            _isPolling = false;
            UpdateUI();
            UpdateMuteStatus();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            App.NotificationService.ShowTestNotifications();
        }

        private void UpdateUI()
        {
            StartButton.IsEnabled = !_isPolling;
            StopButton.IsEnabled = _isPolling;
            PollingIntervalTextBox.IsEnabled = !_isPolling;
        }
    }
}
