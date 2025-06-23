using System;
using System.Collections.Generic;
using System.Windows.Forms;
using m3u8_downloader.Events;
using m3u8_downloader.Models;
using m3u8_downloader.Service;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using DialogResult = System.Windows.Forms.DialogResult;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.Forms.MessageBox;

namespace m3u8_downloader.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public List<NavigationMenu> NavigationMenuItems { get; }

        public DelegateCommand<ListBox> ItemSelectionChangedCommand { set; get; }
        public DelegateCommand SelectFolderCommand { set; get; }

        public MainWindowViewModel(IRegionManager regionManager, IAppDataService dataService,
            IEventAggregator eventAggregator)
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
                        // 更新视频列表
                        var folder = dataService.GetValue("VideoFolder") as string;
                        eventAggregator.GetEvent<UpdateVideoResourceEvent>().Publish(folder);
                        break;
                    case 2:
                        region.RequestNavigate("AboutSoftwarePage");
                        break;
                }
            });
            
            SelectFolderCommand = new DelegateCommand(() =>
            {
                var folderDialog = new FolderBrowserDialog
                {
                    Description = @"选择视频文件保存目录",
                    RootFolder = Environment.SpecialFolder.Desktop
                };

                if (folderDialog.ShowDialog() != DialogResult.OK) return;
                dataService.PutValue("VideoFolder", folderDialog.SelectedPath);
                MessageBox.Show(@"目录设置成功", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
        }
    }
}