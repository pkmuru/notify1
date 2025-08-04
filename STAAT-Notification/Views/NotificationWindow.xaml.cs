using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using STAAT_Notification.Controls;
using STAAT_Notification.Helpers;
using STAAT_Notification.Models;
using STAAT_Notification.Settings;

namespace STAAT_Notification.Views
{
    public partial class NotificationWindow : Window
    {
        public ObservableCollection<Notification> Notifications { get; set; }
        private bool _dpiHookInitialized = false;
        
        public Visibility NoNotificationsVisibility 
        { 
            get { return Notifications?.Count == 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public NotificationWindow()
        {
            InitializeComponent();
            Notifications = new ObservableCollection<Notification>();
            DataContext = this;
            
            // Subscribe to settings changes
            if (SettingsDropdownControl != null)
            {
                SettingsDropdownControl.SettingsChanged += OnSettingsChanged;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PositionWindow();
            AnimateIn();
            ApplyWindowsThemeColors();
        }

        private void PositionWindow()
        {
            try
            {
                // Always use the primary monitor for notifications
                // This ensures consistent positioning near the system tray
                var targetMonitor = ScreenHelper.GetPrimaryMonitor();
                
                if (targetMonitor == null)
                {
                    // If we can't get monitor info, use fallback
                    FallbackPositioning();
                    return;
                }
                
                // Get the notification position
                var position = ScreenHelper.GetNotificationPosition(this, targetMonitor);
                
                // Set the position
                Left = position.X;
                Top = position.Y;
                
                // Handle DPI changes when window moves between monitors
                // We'll hook this up only once when the window is first initialized
                if (!_dpiHookInitialized)
                {
                    SourceInitialized += OnSourceInitialized;
                    _dpiHookInitialized = true;
                }
            }
            catch (Exception ex)
            {
                // Fallback to simple positioning if advanced positioning fails
                System.Diagnostics.Debug.WriteLine($"Advanced positioning failed: {ex.Message}");
                FallbackPositioning();
            }
        }
        
        private void FallbackPositioning()
        {
            // Simple fallback using SystemParameters
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 24;
            Top = workArea.Bottom - Height - 48;
        }
        
        private void OnSourceInitialized(object sender, EventArgs e)
        {
            // Hook into DPI change notifications
            var source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null)
            {
                source.AddHook(WndProc);
            }
        }
        
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_DPICHANGED = 0x02E0;
            
            if (msg == WM_DPICHANGED)
            {
                // Window moved to a monitor with different DPI
                // Reposition the window
                PositionWindow();
                handled = true;
            }
            
            return IntPtr.Zero;
        }

        private void AnimateIn()
        {
            WindowTransform.X = 400;
            Opacity = 0;

            var slideAnimation = new DoubleAnimation
            {
                From = 400,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var fadeAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            WindowTransform.BeginAnimation(TranslateTransform.XProperty, slideAnimation);
            BeginAnimation(OpacityProperty, fadeAnimation);
        }

        public void AnimateOut(Action onComplete = null)
        {
            var slideAnimation = new DoubleAnimation
            {
                From = 0,
                To = 400,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            var fadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            slideAnimation.Completed += (s, e) => onComplete?.Invoke();

            WindowTransform.BeginAnimation(TranslateTransform.XProperty, slideAnimation);
            BeginAnimation(OpacityProperty, fadeAnimation);
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            Notifications.Clear();
            AnimateOut(() => Close());
        }

        private async void DismissNotification_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var notificationId = button?.Tag as string;
            
            if (!string.IsNullOrEmpty(notificationId))
            {
                var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    // Find the container for this notification
                    var container = GetNotificationContainer(notificationId);
                    if (container != null)
                    {
                        // Disable the button to prevent multiple clicks
                        button.IsEnabled = false;
                        
                        // Run the deletion animation
                        await AnimateNotificationDeletion(container);
                    }
                    
                    // Remove from collection after animation
                    Notifications.Remove(notification);
                    
                    if (Notifications.Count == 0)
                    {
                        AnimateOut(() => Close());
                    }
                }
            }
        }

        private void NotificationItem_CloseRequested(object sender, string notificationId)
        {
            if (!string.IsNullOrEmpty(notificationId))
            {
                var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    // Remove from collection (animation already handled by NotificationItem)
                    Notifications.Remove(notification);
                    
                    if (Notifications.Count == 0)
                    {
                        AnimateOut(() => Close());
                    }
                }
            }
        }

        private void NotificationItem_ActionClicked(object sender, Notification notification)
        {
            if (notification != null && !string.IsNullOrEmpty(notification.ActionUrl))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = notification.ActionUrl,
                        UseShellExecute = true
                    });
                    
                    // Close the notification window after action is clicked
                    AnimateOut(() => Close());
                }
                catch (Exception ex)
                {
                    // Log error but don't show to user
                    Console.WriteLine($"Error opening URL: {ex.Message}");
                }
            }
        }

        private FrameworkElement GetNotificationContainer(string notificationId)
        {
            // Find the container in the ItemsControl
            for (int i = 0; i < NotificationsItemsControl.Items.Count; i++)
            {
                var container = NotificationsItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container != null)
                {
                    var notificationItem = FindVisualChild<NotificationItem>(container);
                    if (notificationItem?.NotificationId == notificationId)
                    {
                        return notificationItem;
                    }
                }
            }
            return null;
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;
                
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private async Task AnimateNotificationDeletion(FrameworkElement container)
        {
            var storyboard = Resources["DeleteNotificationStoryboard"] as Storyboard;
            if (storyboard != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                
                EventHandler completedHandler = null;
                completedHandler = (s, e) =>
                {
                    storyboard.Completed -= completedHandler;
                    tcs.SetResult(true);
                };
                
                storyboard.Completed += completedHandler;
                storyboard.Begin(container);
                
                await tcs.Task;
                
                // Add height collapse animation
                await AnimateHeightCollapse(container);
            }
        }

        private async Task AnimateHeightCollapse(FrameworkElement element)
        {
            var heightAnimation = new DoubleAnimation
            {
                From = element.ActualHeight,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var tcs = new TaskCompletionSource<bool>();
            heightAnimation.Completed += (s, e) => tcs.SetResult(true);
            
            element.BeginAnimation(HeightProperty, heightAnimation);
            await tcs.Task;
        }

        private void ApplyWindowsThemeColors()
        {
            try
            {
                var accentColor = WindowsThemeHelper.GetAccentColor();
                var isLightTheme = WindowsThemeHelper.IsLightTheme();
                
                // Update accent colors
                if (Resources.Contains("AccentColor"))
                {
                    Resources["AccentColor"] = new SolidColorBrush(accentColor);
                }
                
                if (Resources.Contains("AccentHover"))
                {
                    Resources["AccentHover"] = new SolidColorBrush(WindowsThemeHelper.GetHoverAccentColor(accentColor));
                }
                
                if (Resources.Contains("AccentPressed"))
                {
                    Resources["AccentPressed"] = new SolidColorBrush(WindowsThemeHelper.GetPressedAccentColor(accentColor));
                }
                
                // Adjust colors based on theme
                if (!isLightTheme)
                {
                    // Dark theme adjustments
                    Resources["NotificationBackground"] = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                    Resources["NotificationItemBackground"] = new SolidColorBrush(Color.FromRgb(42, 42, 42));
                    Resources["NotificationItemHover"] = new SolidColorBrush(Color.FromRgb(48, 48, 48));
                    Resources["HeaderBackground"] = new SolidColorBrush(Color.FromRgb(32, 32, 32));
                    Resources["PrimaryText"] = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                    Resources["SecondaryText"] = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                    Resources["TertiaryText"] = new SolidColorBrush(Color.FromRgb(150, 150, 150));
                    Resources["DividerColor"] = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                    Resources["ScrollBarThumb"] = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                    Resources["ScrollBarThumbHover"] = new SolidColorBrush(Color.FromRgb(120, 120, 120));
                    Resources["ScrollBarThumbPressed"] = new SolidColorBrush(Color.FromRgb(140, 140, 140));
                    Resources["SettingsDropdownBg"] = new SolidColorBrush(Color.FromRgb(42, 42, 42));
                    Resources["SettingsDropdownBorder"] = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                    Resources["NotificationBorder"] = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                }
            }
            catch
            {
                // Ignore any errors and use default colors
            }
        }

        public void LoadNotifications(ObservableCollection<Notification> notifications)
        {
            Notifications.Clear();
            foreach (var notification in notifications)
            {
                Notifications.Add(notification);
            }
            
            NotificationsItemsControl.ItemsSource = Notifications;
        }

        private void SettingsToggle_Click(object sender, RoutedEventArgs e)
        {
            var toggleButton = sender as ToggleButton;
            if (toggleButton != null)
            {
                SettingsPopup.IsOpen = toggleButton.IsChecked ?? false;
            }
        }

        private void OnSettingsChanged(object sender, NotificationSettings settings)
        {
            // Settings have been changed and saved
            // The NotificationService will handle the actual muting logic
            if (!settings.IsNotificationsEnabled)
            {
                // If notifications are turned off, close the window
                AnimateOut(() => Close());
            }
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Allow dragging the window
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                try
                {
                    DragMove();
                }
                catch (InvalidOperationException)
                {
                    // Can only initiate drag when left mouse button is pressed
                }
            }
        }

    }
}