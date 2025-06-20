using System.Collections.Generic;
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
    }
}