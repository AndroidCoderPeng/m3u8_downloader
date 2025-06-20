using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using m3u8_downloader.Models;
using m3u8_downloader.Utils;
using Newtonsoft.Json;
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

        private readonly VideoManager _videoManager;

        public FinishedTaskPageViewModel()
        {
            var folder = @"C:\Users\Administrator\Desktop\文件\temp";
            // var folder = ConfigurationManager.AppSettings["VideoFolder"];
            if (string.IsNullOrEmpty(folder))
            {
                IsLoadingVisible = Visibility.Collapsed;
                return;
            }

            _videoManager = new VideoManager(folder);
            LoadVideosAsync();
        }

        private async void LoadVideosAsync()
        {
            var videos = await _videoManager.GetVideosAsync();
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

            foreach (var video in videos)
            {
                Console.WriteLine(JsonConvert.SerializeObject(video));
                Videos.Add(video);
            }
        }
    }
}