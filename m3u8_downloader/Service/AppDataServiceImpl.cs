using System.Collections.Generic;
using System.Windows;
using m3u8_downloader.Models;

namespace m3u8_downloader.Service
{
    public class AppDataServiceImpl : IAppDataService
    {
        public List<NavigationMenu> GetNavigationMenu()
        {
            return new List<NavigationMenu>
            {
                new NavigationMenu { Icon = "\ue60c", Title = "下载中" },
                new NavigationMenu { Icon = "\ue652", Title = "已完成" },
                new NavigationMenu { Icon = "\ue6cd", Title = "设置" },
                new NavigationMenu { Icon = "\ue646", Title = "关于" }
            };
        }

        public void PutValue(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            Application.Current.Properties[key] = value;
        }

        public object GetValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;
            return Application.Current.Properties[key];
        }
    }
}