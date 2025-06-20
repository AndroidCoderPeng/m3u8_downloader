﻿using System.Collections.Generic;
using m3u8_downloader.Models;

namespace m3u8_downloader.Service
{
    public interface IAppDataService
    {
        List<NavigationMenu> GetNavigationMenu();
        void PutValue(string key, object value);
        object GetValue(string key);
    }
}