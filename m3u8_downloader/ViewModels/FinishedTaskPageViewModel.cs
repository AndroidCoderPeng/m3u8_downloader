using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using m3u8_downloader.Models;
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
        
        public FinishedTaskPageViewModel()
        {
            var folder = ConfigurationManager.AppSettings["VideoFolder"];
            if (string.IsNullOrEmpty(folder))
            {
                IsLoadingCompleted = true;
                return;
            }

            var files = Directory.GetFiles(folder, "*.mp4");
            foreach (var file in files)
            {
                //采用三级缓存
                
                // Videos.Add(new FileInfo(file));
            }
            IsLoadingCompleted = true;
        }
    }
}