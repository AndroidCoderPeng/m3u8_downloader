using System;
using System.Collections.Generic;
using System.IO;
using m3u8_downloader.Models;
using Newtonsoft.Json;

namespace m3u8_downloader.Service
{
    public class AppDataServiceImpl : IAppDataService
    {
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "application.json");
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public AppDataServiceImpl() {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
        }

        public List<NavigationMenu> GetNavigationMenu()
        {
            return new List<NavigationMenu>
            {
                new NavigationMenu { Icon = "\ue60c", Title = "下载中" },
                new NavigationMenu { Icon = "\ue652", Title = "已完成" },
                new NavigationMenu { Icon = "\ue646", Title = "关于" }
            };
        }

        public void PutValue(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            _data[key] = value;
            File.WriteAllText(_filePath, JsonConvert.SerializeObject(_data));
        }

        public object GetValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            _data.TryGetValue(key, out var value);
            return value;
        }
    }
}