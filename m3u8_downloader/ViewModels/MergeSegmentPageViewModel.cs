using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using m3u8_downloader.Models;
using m3u8_downloader.Utils;
using Prism.Commands;
using Prism.Mvvm;

namespace m3u8_downloader.ViewModels
{
    public class MergeSegmentPageViewModel : BindableBase
    {
        private string _segmentsRootPath = string.Empty;

        public string SegmentsRootPath
        {
            set
            {
                _segmentsRootPath = value;
                RaisePropertyChanged();
            }
            get => _segmentsRootPath;
        }

        private ObservableCollection<SegmentFile> _resourceSegments = new ObservableCollection<SegmentFile>();

        public ObservableCollection<SegmentFile> ResourceSegments
        {
            get => _resourceSegments;
            set
            {
                _resourceSegments = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _isLoadingVisible = Visibility.Collapsed;

        public Visibility IsLoadingVisible
        {
            get => _isLoadingVisible;
            set
            {
                _isLoadingVisible = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _isSegmentsVisible = Visibility.Collapsed;

        public Visibility IsSegmentsVisible
        {
            get => _isSegmentsVisible;
            set
            {
                _isSegmentsVisible = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _isEmptyImageVisible = Visibility.Visible;

        public Visibility IsEmptyImageVisible
        {
            get => _isEmptyImageVisible;
            set
            {
                _isEmptyImageVisible = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand SelectSegmentsCommand { set; get; }
        public DelegateCommand RootPathClearedCommand { set; get; }

        public MergeSegmentPageViewModel()
        {
            RootPathClearedCommand = new DelegateCommand(delegate
            {
                if (ResourceSegments.Any())
                {
                    ResourceSegments.Clear();
                }

                IsEmptyImageVisible = Visibility.Visible;
                IsSegmentsVisible = Visibility.Collapsed;
            });

            SelectSegmentsCommand = new DelegateCommand(delegate
            {
                var folderDialog = new FolderBrowserDialog
                {
                    Description = @"选择片段根目录",
                    RootFolder = Environment.SpecialFolder.Desktop
                };

                if (folderDialog.ShowDialog() != DialogResult.OK) return;
                SegmentsRootPath = folderDialog.SelectedPath;
                IsLoadingVisible = Visibility.Visible;
                IsSegmentsVisible = Visibility.Collapsed;
                IsEmptyImageVisible = Visibility.Collapsed;
                LoadSegmentsAsync();
            });
        }

        private async void LoadSegmentsAsync()
        {
            if (ResourceSegments.Any())
            {
                ResourceSegments.Clear();
            }

            // 获取片段
            var segmentManager = new SegmentManager(_segmentsRootPath);
            var segments = await segmentManager.GetSegmentsAsync();
            foreach (var segment in segments)
            {
                ResourceSegments.Add(segment);
            }

            IsLoadingVisible = Visibility.Collapsed;
            if (_resourceSegments.Any())
            {
                IsEmptyImageVisible = Visibility.Collapsed;
                IsSegmentsVisible = Visibility.Visible;
            }
            else
            {
                IsEmptyImageVisible = Visibility.Visible;
                IsSegmentsVisible = Visibility.Collapsed;
            }
        }
    }
}