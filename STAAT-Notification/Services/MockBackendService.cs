using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using STAAT_Notification.Models;

namespace STAAT_Notification.Services
{
    public class MockBackendService
    {
        private static readonly List<Notification> _staticNotifications = new List<Notification>
        {
            new Notification
            {
                Id = "1",
                AppName = "WhatsApp",
                Title = "Amanda Brady",
                Message = "Hey! Can you help me out with this problem really quick?",
                Icon = "https://i.pravatar.cc/150?img=1",
                Timestamp = DateTime.Now.AddMinutes(-5),
                Action = "Reply",
                ActionUrl = "mailto:amanda@example.com?subject=Re: Help needed"
            },
            new Notification
            {
                Id = "2",
                AppName = "Microsoft Teams",
                Title = "Jane Smith",
                Message = "Great job at the pitch review today! Let's catch up later.",
                Icon = "https://i.pravatar.cc/150?img=2",
                Timestamp = DateTime.Now.AddMinutes(-15),
                Action = "View Profile",
                ActionUrl = "https://linkedin.com/in/janesmith"
            },
            new Notification
            {
                Id = "3",
                AppName = "Calendar",
                Title = "Contoso Pitch Review",
                Message = "Meeting starts in 15 minutes",
                Icon = "https://i.pravatar.cc/150?img=3",
                Timestamp = DateTime.Now.AddMinutes(-30),
                Action = "Join Meeting",
                ActionUrl = "https://teams.microsoft.com/meet/contoso-pitch-review"
            },
            new Notification
            {
                Id = "4",
                AppName = "News",
                Title = "Breaking News",
                Message = "Markets are showing significant changes today",
                Icon = "https://i.pravatar.cc/150?img=4",
                Timestamp = DateTime.Now.AddMinutes(-45)
            },
            new Notification
            {
                Id = "5",
                AppName = "System",
                Title = "System Update",
                Message = "A new update is available for your system",
                Icon = "https://i.pravatar.cc/150?img=5",
                Timestamp = DateTime.Now.AddMinutes(-60),
                Action = "Install Update",
                ActionUrl = "ms-settings:windowsupdate"
            }
        };

        public Task<NotificationResponse> GetNotificationsAsync()
        {
            // Return a copy of the static notifications to prevent modification
            var notifications = _staticNotifications.Select(n => new Notification
            {
                Id = n.Id,
                AppName = n.AppName,
                Title = n.Title,
                Message = n.Message,
                Icon = n.Icon,
                Timestamp = n.Timestamp,
                Action = n.Action,
                ActionUrl = n.ActionUrl
            }).ToList();

            return Task.FromResult(new NotificationResponse
            {
                Notifications = notifications
            });
        }
    }
}