using System;
using System.IO;
using System.Windows.Forms;
using m3u8_downloader.Events;
using m3u8_downloader.Service;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using DialogResult = System.Windows.Forms.DialogResult;

namespace m3u8_downloader.ViewModels
{
    public class SoftwareSettingPageViewModel : BindableBase
    {
        private string _selectedOutputPath = "未设置";

        public string SelectedOutputPath
        {
            set
            {
                _selectedOutputPath = value;
                RaisePropertyChanged();
            }
            get => _selectedOutputPath;
        }

        private string _cacheSize = "0.00 B";

        public string CacheSize
        {
            set
            {
                _cacheSize = value;
                RaisePropertyChanged();
            }
            get => _cacheSize;
        }

        public DelegateCommand SelectFolderCommand { set; get; }

        public SoftwareSettingPageViewModel(IAppDataService dataService, IEventAggregator eventAggregator)
        {
            SelectedOutputPath = dataService.GetValue("VideoFolder") as string;

            eventAggregator.GetEvent<UpdateCacheSizeEvent>().Subscribe(() =>
            {
                var cacheFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache");
                var files = Directory.GetFiles(cacheFolderPath);
                long totalSize = 0;
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }
                CacheSize = $"{totalSize / 1024} KB";
            });

            SelectFolderCommand = new DelegateCommand(() =>
            {
                var folderDialog = new FolderBrowserDialog
                {
                    Description = @"选择视频文件保存目录",
                    RootFolder = Environment.SpecialFolder.Desktop
                };

                if (folderDialog.ShowDialog() != DialogResult.OK) return;
                SelectedOutputPath = folderDialog.SelectedPath;
                dataService.PutValue("VideoFolder", folderDialog.SelectedPath);
            });
        }
    }
}