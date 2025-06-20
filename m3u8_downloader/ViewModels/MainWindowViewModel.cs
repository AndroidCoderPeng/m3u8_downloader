using System;
using System.Collections.Generic;
using System.Configuration;
using System.Windows.Forms;
using HandyControl.Controls;
using m3u8_downloader.Models;
using m3u8_downloader.Service;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using DialogResult = System.Windows.Forms.DialogResult;
using ListBox = System.Windows.Controls.ListBox;

namespace m3u8_downloader.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public List<NavigationMenu> NavigationMenuItems { get; }

        public DelegateCommand<ListBox> ItemSelectionChangedCommand { set; get; }
        public DelegateCommand SelectFolderCommand { set; get; }

        public MainWindowViewModel(IRegionManager regionManager, IAppDataService dataService)
        {
            NavigationMenuItems = dataService.GetNavigationMenu();

            ItemSelectionChangedCommand = new DelegateCommand<ListBox>(listBox =>
            {
                var region = regionManager.Regions["ContentRegion"];
                switch (listBox.SelectedIndex)
                {
                    case 0:
                        region.RequestNavigate("DownloadTaskPage");
                        break;
                    case 1:
                        region.RequestNavigate("FinishedTaskPage");
                        break;
                    case 2:
                        // region.RequestNavigate("SoftwareSettingsPage");
                        break;
                    case 3:
                        region.RequestNavigate("AboutSoftwarePage");
                        break;
                }
            });
            ///////////////////////////////////////////////////////////////////////////////
            SelectFolderCommand = new DelegateCommand(() =>
            {
                var folderDialog = new FolderBrowserDialog
                {
                    Description = @"选择视频文件保存目录",
                    RootFolder = Environment.SpecialFolder.Desktop
                };

                if (folderDialog.ShowDialog() != DialogResult.OK) return;
                var folderPath = folderDialog.SelectedPath;
                if (string.IsNullOrEmpty(folderPath)) return;
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["VideoFolder"].Value = folderPath;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                Growl.Success("路径设置成功");
            });
        }
    }
}