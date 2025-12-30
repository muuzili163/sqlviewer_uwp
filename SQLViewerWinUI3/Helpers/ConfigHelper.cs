using Windows.Storage;

namespace SQLViewerWinUI3.Helpers
{
    public static class ConfigHelper
    {
        private static ApplicationDataContainer LocalSettings => ApplicationData.Current.LocalSettings;

        /// <summary>
        /// 根据 Key 读取配置值
        /// </summary>
        public static string? Get(string key)
        {
            if (LocalSettings.Values.ContainsKey(key))
            {
                return LocalSettings.Values[key]?.ToString();
            }
            return null;
        }

        /// <summary>
        /// 写入或更新配置值（自动保存）
        /// </summary>
        public static void Set(string key, string value)
        {
            LocalSettings.Values[key] = value;
        }
    }
}
