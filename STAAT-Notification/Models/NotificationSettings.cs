using System;

namespace STAAT_Notification.Models
{
    public class NotificationSettings
    {
        public bool IsNotificationsEnabled { get; set; } = true;
        public bool IsMuted { get; set; } = false;
        public DateTime? MuteUntil { get; set; }
        public MuteDuration SelectedMuteDuration { get; set; } = MuteDuration.None;
        
        public bool IsCurrentlyMuted
        {
            get
            {
                if (!IsMuted) return false;
                if (MuteUntil == null) return false;
                return DateTime.Now < MuteUntil.Value;
            }
        }

        public string MuteStatusText
        {
            get
            {
                if (!IsCurrentlyMuted) return string.Empty;
                if (MuteUntil == null) return string.Empty;
                
                var timeLeft = MuteUntil.Value - DateTime.Now;
                if (timeLeft.TotalHours > 1)
                    return $"Muted for {(int)timeLeft.TotalHours} hours";
                else if (timeLeft.TotalMinutes > 1)
                    return $"Muted for {(int)timeLeft.TotalMinutes} minutes";
                else
                    return "Muted for less than a minute";
            }
        }
    }

    public enum MuteDuration
    {
        None,
        OneHour,
        FourHours,
        EightHours,
        UntilTomorrow
    }
}