using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STAAT_Notification.Models;

namespace STAAT_Notification.Services
{
    public class MockBackendService
    {
        private readonly Random _random = new Random();
        private readonly List<string> _appNames = new List<string> { "WhatsApp", "Microsoft Teams", "Calendar", "News", "System" };
        private readonly List<string> _sampleTitles = new List<string> 
        { 
            "Amanda Brady", "Jane Smith", "Contoso Pitch Review", "Breaking News", "System Update"
        };
        private readonly List<string> _sampleMessages = new List<string>
        {
            "Hey! Can you help me out with this problem really quick?",
            "Great job at the pitch review today! Let's catch up later.,Great job at the pitch review today! Let's catch up later.Great job at the pitch review today! Let's catch up later.",
            "Meeting starts in 15 minutes",
            "Markets are showing significant changes today",
            "A new update is available for your system"
        };
        private readonly List<string> _sampleIcons = new List<string>
        {
            "https://i.pravatar.cc/150?img=1",
            "https://i.pravatar.cc/150?img=2",
            "https://i.pravatar.cc/150?img=3",
            "https://i.pravatar.cc/150?img=4",
            "https://i.pravatar.cc/150?img=5"
        };
        
        private readonly List<(string action, string url)> _sampleActions = new List<(string, string)>
        {
            ("Reply", "mailto:amanda@example.com?subject=Re: Help needed"),
            ("View Profile", "https://linkedin.com/in/janesmith"),
            ("Join Meeting", "https://teams.microsoft.com/meet/contoso-pitch-review"),
            ("Read More", "https://news.example.com/breaking-news-today"),
            ("Install Update", "ms-settings:windowsupdate")
        };

        public Task<NotificationResponse> GetNotificationsAsync()
        {
            var notificationCount = _random.Next(3, 8);
            var notifications = new List<Notification>();

            for (int i = 0; i < notificationCount; i++)
            {
                var appIndex = _random.Next(_appNames.Count);
                var notification = new Notification
                {
                    Id = Guid.NewGuid().ToString(),
                    AppName = _appNames[appIndex],
                    Title = _sampleTitles[appIndex],
                    Message = _sampleMessages[appIndex],
                    Icon = _sampleIcons[appIndex],
                    Timestamp = DateTime.Now.AddMinutes(-_random.Next(1, 120))
                };
                
                // Randomly add action (70% chance)
                if (_random.Next(100) < 70 && appIndex < _sampleActions.Count)
                {
                    notification.Action = _sampleActions[appIndex].action;
                    notification.ActionUrl = _sampleActions[appIndex].url;
                }
                
                notifications.Add(notification);
            }

            return Task.FromResult(new NotificationResponse
            {
                Notifications = notifications.OrderByDescending(n => n.Timestamp).ToList()
            });
        }
    }
}