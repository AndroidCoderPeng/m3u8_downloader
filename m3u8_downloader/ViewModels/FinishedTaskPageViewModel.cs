using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using ImTools;
using m3u8_downloader.Events;
using m3u8_downloader.Models;
using m3u8_downloader.Utils;
using m3u8_downloader.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace m3u8_downloader.ViewModels
{
    public class FinishedTaskPageViewModel : BindableBase
    {
        private ObservableCollection<VideoFile> _videos = new ObservableCollection<VideoFile>();

        public ObservableCollection<VideoFile> Videos
        {
            get => _videos;
            set
            {
                _videos = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _isLoadingVisible = Visibility.Visible;

        public Visibility IsLoadingVisible
        {
            get => _isLoadingVisible;
            set
            {
                _isLoadingVisible = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _isVideoListBoxVisible = Visibility.Collapsed;

        public Visibility IsVideoListBoxVisible
        {
            get => _isVideoListBoxVisible;
            set
            {
                _isVideoListBoxVisible = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _isEmptyImageVisible = Visibility.Collapsed;

        public Visibility IsEmptyImageVisible
        {
            get => _isEmptyImageVisible;
            set
            {
                _isEmptyImageVisible = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand<string> MouseDoubleClickCommand { set; get; }
        public DelegateCommand<string> DeleteTaskCommand { set; get; }

        public FinishedTaskPageViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<UpdateVideoResourceEvent>().Subscribe(LoadVideosAsync);

            MouseDoubleClickCommand = new DelegateCommand<string>(filePath =>
            {
                if (!File.Exists(filePath)) return;
                new PlayVideoWindow(filePath) { Owner = Application.Current.MainWindow }.ShowDialog();
            });

            DeleteTaskCommand = new DelegateCommand<string>(filePath =>
            {
                if (!File.Exists(filePath)) return;
                File.Delete(filePath);
                var videoFile = _videos.FindFirst(x => x.FilePath == filePath);
                Videos.Remove(videoFile);
                if (_videos.Any())
                {
                    IsEmptyImageVisible = Visibility.Collapsed;
                    IsVideoListBoxVisible = Visibility.Visible;
                }
                else
                {
                    IsEmptyImageVisible = Visibility.Visible;
                    IsVideoListBoxVisible = Visibility.Collapsed;
                }
            });
        }

        private async void LoadVideosAsync(string folder)
        {
            if (_videos.Any())
            {
                Videos.Clear();
            }

            if (string.IsNullOrEmpty(folder))
            {
                IsLoadingVisible = Visibility.Collapsed;
                IsEmptyImageVisible = Visibility.Visible;
                IsVideoListBoxVisible = Visibility.Collapsed;
                return;
            }

            var videoManager = new VideoManager(folder);
            var videos = await videoManager.GetVideosAsync();
            foreach (var video in videos)
            {
                Videos.Add(video);
            }

            IsLoadingVisible = Visibility.Collapsed;
            if (videos.Any())
            {
                IsVideoListBoxVisible = Visibility.Visible;
                IsEmptyImageVisible = Visibility.Collapsed;
            }
            else
            {
                IsEmptyImageVisible = Visibility.Visible;
                IsVideoListBoxVisible = Visibility.Collapsed;
            }

            Console.WriteLine($@"数据加载完成，一共加载 {videos.Count} 个视频");
        }
    }
}