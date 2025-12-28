using System;
using System.IO;
using System.Text.Json;

namespace SQLViewerUWP
{
    /// <summary>
    /// Configuration helper for storing app settings
    /// WPF version using local JSON file storage
    /// </summary>
    class ConfigHelper
    {
        private static readonly string ConfigFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SQLViewerUWP",
            "config.json"
        );

        private static Dictionary<string, string>? _cache;

        private static Dictionary<string, string> LoadConfig()
        {
            if (_cache != null)
                return _cache;

            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    _cache = JsonSerializer.Deserialize<Dictionary<string, string>>(json) 
                             ?? new Dictionary<string, string>();
                }
                else
                {
                    _cache = new Dictionary<string, string>();
                }
            }
            catch
            {
                _cache = new Dictionary<string, string>();
            }

            return _cache;
        }

        private static void SaveConfig()
        {
            try
            {
                string dir = Path.GetDirectoryName(ConfigFilePath)!;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string json = JsonSerializer.Serialize(_cache, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                File.WriteAllText(ConfigFilePath, json);
            }
            catch
            {
                // Silently fail for now
            }
        }

        /// <summary>
        /// Get configuration value by key
        /// </summary>
        public static string? Get(string key)
        {
            var config = LoadConfig();
            return config.ContainsKey(key) ? config[key] : null;
        }

        /// <summary>
        /// Set configuration value by key
        /// </summary>
        public static void Set(string key, string value)
        {
            var config = LoadConfig();
            config[key] = value;
            SaveConfig();
        }
    }
}
