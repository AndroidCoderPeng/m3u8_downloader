using System.Collections.Generic;
using System.Windows.Controls;
using m3u8_downloader.Events;
using m3u8_downloader.Models;
using m3u8_downloader.Service;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace m3u8_downloader.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public List<NavigationMenu> NavigationMenuItems { get; }

        public DelegateCommand<ListBox> ItemSelectionChangedCommand { set; get; }

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
                        region.RequestNavigate("MergeSegmentPage");
                        break;
                    case 3:
                        region.RequestNavigate("SoftwareSettingPage");
                        //刷新缓存大小
                        eventAggregator.GetEvent<UpdateCacheSizeEvent>().Publish();
                        break;
                    case 4:
                        region.RequestNavigate("AboutSoftwarePage");
                        break;
                }
            });
        }
    }
}