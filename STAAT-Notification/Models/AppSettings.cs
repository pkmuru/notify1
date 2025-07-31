namespace STAAT_Notification.Models
{
    public class AppSettings
    {
        public double PollingIntervalHours { get; set; } = 24;
        public string BackendUrl { get; set; } = "http://localhost:5000/api/notifications";
        public int MaxNotifications { get; set; } = 8;
        public bool EnableSounds { get; set; } = false;
        public int AutoHideSeconds { get; set; } = 0; // 0 means no auto-hide
    }
}