using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLViewer
{
    class ConfigHelper
    {

        /// <summary>
        /// 根据 Key 读取配置值
        /// </summary>
        public static string Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// 写入或更新配置值（自动保存）
        /// </summary>
        public static void Set(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // 如果 key 已存在，先删除
            if (config.AppSettings.Settings[key] != null)
            {
                config.AppSettings.Settings[key].Value = value;
            }
            else
            {
                config.AppSettings.Settings.Add(key, value);
            }

            // 保存配置文件
            config.Save(ConfigurationSaveMode.Modified);

            // 刷新缓存
            ConfigurationManager.RefreshSection("appSettings");
        }

    }
}
