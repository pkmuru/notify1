using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using STAAT_Notification.Helpers;
using STAAT_Notification.Models;
using STAAT_Notification.Settings;

namespace STAAT_Notification.Views
{
    public partial class NotificationWindow : Window
    {
        public ObservableCollection<Notification> Notifications { get; set; }
        
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
        }

        private void PositionWindow()
        {
            var dpiScale = DpiHelper.GetDpiScale(this);
            var workArea = SystemParameters.WorkArea;
            
            Left = workArea.Right - (Width * dpiScale) - (16 * dpiScale);
            Top = workArea.Bottom - (Height * dpiScale) - (16 * dpiScale);
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

        private FrameworkElement GetNotificationContainer(string notificationId)
        {
            // Find the container in the ItemsControl
            for (int i = 0; i < NotificationsItemsControl.Items.Count; i++)
            {
                var container = NotificationsItemsControl.ItemContainerGenerator.ContainerFromIndex(i) as ContentPresenter;
                if (container != null)
                {
                    var border = FindVisualChild<Border>(container);
                    if (border?.Tag?.ToString() == notificationId)
                    {
                        return border;
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

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var notification = button?.Tag as Notification;
            
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
    }
}