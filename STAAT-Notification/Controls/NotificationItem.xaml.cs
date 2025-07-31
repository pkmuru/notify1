using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using STAAT_Notification.Models;

namespace STAAT_Notification.Controls
{
    public partial class NotificationItem : UserControl
    {
        public static readonly DependencyProperty NotificationIdProperty =
            DependencyProperty.Register("NotificationId", typeof(string), typeof(NotificationItem));

        public static readonly DependencyProperty AppNameProperty =
            DependencyProperty.Register("AppName", typeof(string), typeof(NotificationItem), 
                new PropertyMetadata("Unknown App"));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(NotificationItem), 
                new PropertyMetadata("Notification"));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(NotificationItem), 
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(string), typeof(NotificationItem), 
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TimestampDisplayProperty =
            DependencyProperty.Register("TimestampDisplay", typeof(string), typeof(NotificationItem), 
                new PropertyMetadata("Just now"));

        public static readonly DependencyProperty ActionTextProperty =
            DependencyProperty.Register("ActionText", typeof(string), typeof(NotificationItem), 
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ActionUrlProperty =
            DependencyProperty.Register("ActionUrl", typeof(string), typeof(NotificationItem));

        public static readonly DependencyProperty HasActionProperty =
            DependencyProperty.Register("HasAction", typeof(bool), typeof(NotificationItem), 
                new PropertyMetadata(false));

        public static readonly DependencyProperty NotificationDataProperty =
            DependencyProperty.Register("NotificationData", typeof(Notification), typeof(NotificationItem));

        public string NotificationId
        {
            get { return (string)GetValue(NotificationIdProperty); }
            set { SetValue(NotificationIdProperty, value); }
        }

        public string AppName
        {
            get { return (string)GetValue(AppNameProperty); }
            set { SetValue(AppNameProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public string TimestampDisplay
        {
            get { return (string)GetValue(TimestampDisplayProperty); }
            set { SetValue(TimestampDisplayProperty, value); }
        }

        public string ActionText
        {
            get { return (string)GetValue(ActionTextProperty); }
            set { SetValue(ActionTextProperty, value); }
        }

        public string ActionUrl
        {
            get { return (string)GetValue(ActionUrlProperty); }
            set { SetValue(ActionUrlProperty, value); }
        }

        public bool HasAction
        {
            get { return (bool)GetValue(HasActionProperty); }
            set { SetValue(HasActionProperty, value); }
        }

        public Notification NotificationData
        {
            get { return (Notification)GetValue(NotificationDataProperty); }
            set { SetValue(NotificationDataProperty, value); }
        }

        public event EventHandler<string> CloseRequested;
        public event EventHandler<Notification> ActionClicked;

        public NotificationItem()
        {
            InitializeComponent();
            
            // Set design-time data
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                AppName = "Sample App";
                Title = "Sample Notification";
                Message = "This is a sample notification message for design preview.";
                Icon = "/docs/shell-1x.png";
                TimestampDisplay = "2 minutes ago";
                ActionText = "View Details";
                HasAction = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            AnimateAndClose();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            ActionClicked?.Invoke(this, NotificationData);
        }

        private void AnimateAndClose()
        {
            var storyboard = new Storyboard();
            
            var fadeAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(fadeAnimation, this);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath("Opacity"));
            
            var slideAnimation = new DoubleAnimation
            {
                To = 50,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            Storyboard.SetTarget(slideAnimation, SlideTransform);
            Storyboard.SetTargetProperty(slideAnimation, new PropertyPath("X"));
            
            storyboard.Children.Add(fadeAnimation);
            storyboard.Children.Add(slideAnimation);
            
            storyboard.Completed += (s, e) =>
            {
                CloseRequested?.Invoke(this, NotificationId);
            };
            
            storyboard.Begin();
        }

        public void SetNotification(Notification notification)
        {
            if (notification != null)
            {
                NotificationData = notification;
                NotificationId = notification.Id;
                AppName = notification.AppName;
                Title = notification.Title;
                Message = notification.Message;
                Icon = notification.Icon;
                TimestampDisplay = notification.TimestampDisplay;
                ActionText = notification.Action;
                ActionUrl = notification.ActionUrl;
                HasAction = notification.HasAction;
            }
        }
    }
}