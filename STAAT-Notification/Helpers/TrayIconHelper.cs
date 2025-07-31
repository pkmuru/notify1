using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using STAAT_Notification.Models;

namespace STAAT_Notification.Helpers
{
    public static class TrayIconHelper
    {
        public static NotifyIcon CreateTrayIcon()
        {
            var trayIcon = new NotifyIcon
            {
                Text = "STAAT Notifications",
                Visible = true,
                Icon = CreateNotificationIcon(false, false)
            };

            var contextMenu = new ContextMenuStrip();
            
            // Show Notifications
            var showItem = new ToolStripMenuItem("Show Notifications");
            showItem.Font = new Font(showItem.Font, FontStyle.Bold);
            contextMenu.Items.Add(showItem);
            
            contextMenu.Items.Add(new ToolStripSeparator());
            
            // Mute options
            var muteMenu = new ToolStripMenuItem("Mute for");
            muteMenu.DropDownItems.Add(new ToolStripMenuItem("1 hour") { Tag = MuteDuration.OneHour });
            muteMenu.DropDownItems.Add(new ToolStripMenuItem("4 hours") { Tag = MuteDuration.FourHours });
            muteMenu.DropDownItems.Add(new ToolStripMenuItem("8 hours") { Tag = MuteDuration.EightHours });
            muteMenu.DropDownItems.Add(new ToolStripMenuItem("Until tomorrow (9:00 AM)") { Tag = MuteDuration.UntilTomorrow });
            contextMenu.Items.Add(muteMenu);
            
            contextMenu.Items.Add(new ToolStripSeparator());
            
            // Turn off notifications
            var turnOffItem = new ToolStripMenuItem("Turn off all notifications") { CheckOnClick = true };
            contextMenu.Items.Add(turnOffItem);
            
            contextMenu.Items.Add(new ToolStripSeparator());
            
            // Settings
            contextMenu.Items.Add(new ToolStripMenuItem("Settings"));
            
            // Exit
            contextMenu.Items.Add(new ToolStripMenuItem("Exit"));
            
            trayIcon.ContextMenuStrip = contextMenu;
            
            return trayIcon;
        }

        public static Icon CreateNotificationIcon(bool isMuted, bool hasNotifications)
        {
            using (var bitmap = new Bitmap(16, 16))
            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                // Draw bell icon
                using (var brush = new SolidBrush(isMuted ? Color.Gray : (hasNotifications ? Color.Blue : Color.Black)))
                using (var pen = new Pen(brush, 1.5f))
                {
                    // Bell shape
                    g.FillEllipse(brush, 5, 2, 6, 6);
                    g.FillRectangle(brush, 5, 5, 6, 6);
                    g.FillRectangle(brush, 3, 11, 10, 2);
                    
                    // Clapper
                    g.FillEllipse(brush, 7, 12, 2, 2);
                    
                    // Mute line if needed
                    if (isMuted)
                    {
                        using (var mutePen = new Pen(Color.Red, 2))
                        {
                            g.DrawLine(mutePen, 2, 2, 14, 14);
                        }
                    }
                    
                    // Notification dot if needed
                    if (hasNotifications && !isMuted)
                    {
                        using (var dotBrush = new SolidBrush(Color.Red))
                        {
                            g.FillEllipse(dotBrush, 10, 2, 4, 4);
                        }
                    }
                }

                return Icon.FromHandle(bitmap.GetHicon());
            }
        }

        public static void UpdateTrayIcon(NotifyIcon trayIcon, bool isMuted, bool hasNotifications, string statusText)
        {
            trayIcon.Icon?.Dispose();
            trayIcon.Icon = CreateNotificationIcon(isMuted, hasNotifications);
            trayIcon.Text = $"STAAT Notifications\n{statusText}";
        }
    }
}