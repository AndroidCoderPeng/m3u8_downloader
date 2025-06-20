using System;
using System.Collections.ObjectModel;
using System.Configuration;
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

        private bool _isLoadingCompleted;

        public bool IsLoadingCompleted
        {
            get => _isLoadingCompleted;
            set
            {
                _isLoadingCompleted = value;
                RaisePropertyChanged();
            }
        }

        private readonly VideoManager _videoManager;
        
        public FinishedTaskPageViewModel()
        {
            var folder = ConfigurationManager.AppSettings["VideoFolder"];
            if (string.IsNullOrEmpty(folder))
            {
                IsLoadingCompleted = true;
                return;
            }
            
            _videoManager = new VideoManager(folder);
            LoadVideosAsync();
        }
        
        private async void LoadVideosAsync()
        {
            var videos = await _videoManager.GetVideosAsync();
            foreach (var video in videos)
            {
                Console.WriteLine(JsonConvert.SerializeObject(video));
                Videos.Add(video);
            }

            IsLoadingCompleted = true;
        }
    }
}