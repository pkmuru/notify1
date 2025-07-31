using System;

namespace STAAT_Notification.Models
{
    public class Notification
    {
        public string Id { get; set; }
        public string AppName { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Icon { get; set; }
        public DateTime Timestamp { get; set; }
        public string TimestampDisplay => GetRelativeTime();
        public string Action { get; set; }
        public string ActionUrl { get; set; }
        
        public bool HasAction => !string.IsNullOrEmpty(Action) && !string.IsNullOrEmpty(ActionUrl);

        private string GetRelativeTime()
        {
            var diff = DateTime.Now - Timestamp;
            
            if (diff.TotalMinutes < 1)
                return "Just now";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} minute{((int)diff.TotalMinutes == 1 ? "" : "s")} ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} hour{((int)diff.TotalHours == 1 ? "" : "s")} ago";
            if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} day{((int)diff.TotalDays == 1 ? "" : "s")} ago";
            
            return Timestamp.ToString("MMM dd, yyyy");
        }
    }
}