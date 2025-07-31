using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using STAAT_Notification.Models;

namespace STAAT_Notification.Services
{
    public static class ApiSyncService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string API_BASE_URL = "https://api.example.com"; // Replace with actual API URL

        public static async Task SyncSettingsAsync(NotificationSettings settings)
        {
            try
            {
                var settingsData = new
                {
                    isEnabled = settings.IsNotificationsEnabled,
                    isMuted = settings.IsMuted,
                    muteUntil = settings.MuteUntil?.ToString("O")
                };

                var json = JsonConvert.SerializeObject(settingsData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // In production, add authentication headers here
                // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync($"{API_BASE_URL}/api/user/notification-settings", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to sync settings: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't crash the app
                Console.WriteLine($"Error syncing settings to API: {ex.Message}");
            }
        }

        public static async Task<NotificationSettings> GetSettingsAsync()
        {
            try
            {
                // In production, add authentication headers here
                var response = await _httpClient.GetAsync($"{API_BASE_URL}/api/user/notification-settings");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(json);
                    
                    return new NotificationSettings
                    {
                        IsNotificationsEnabled = data.isEnabled,
                        IsMuted = data.isMuted,
                        MuteUntil = data.muteUntil != null ? DateTime.Parse(data.muteUntil.ToString()) : (DateTime?)null
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting settings from API: {ex.Message}");
            }
            
            return null;
        }
    }
}