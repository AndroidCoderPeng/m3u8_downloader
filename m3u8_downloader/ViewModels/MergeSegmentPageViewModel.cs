using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using m3u8_downloader.Models;
using m3u8_downloader.Utils;
using Prism.Commands;
using Prism.Mvvm;
using DialogResult = System.Windows.Forms.DialogResult;
using MessageBox = System.Windows.MessageBox;

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

        private double _mergeProgressValue;

        public double MergeProgressValue
        {
            get => _mergeProgressValue;
            set
            {
                _mergeProgressValue = value;
                RaisePropertyChanged();
            }
        }

        public DelegateCommand SelectSegmentsCommand { set; get; }
        public DelegateCommand MergeSegmentsCommand { set; get; }
        public DelegateCommand RootPathClearedCommand { set; get; }
        public DelegateCommand<string> DeleteSegmentCommand { set; get; }

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

            MergeSegmentsCommand = new DelegateCommand(delegate
            {
                if (!_resourceSegments.Any())
                {
                    MessageBox.Show("请先选择要合并的片段根目录", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                MergeProgressValue = 0;
                var indexedSegments = new ConcurrentDictionary<int, string>();
                for (var i = 0; i < _resourceSegments.Count; i++)
                {
                    indexedSegments.TryAdd(i, _resourceSegments[i].FilePath);
                }

                MergeTsSegmentsAsync(indexedSegments);
            });

            DeleteSegmentCommand = new DelegateCommand<string>(segmentName =>
            {
                var segment = _resourceSegments.First(x => x.SegmentName == segmentName);
                ResourceSegments.Remove(segment);
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

        private async void MergeTsSegmentsAsync(ConcurrentDictionary<int, string> indexedSegments)
        {
            // 计算总时长
            long totalDuration = 0;
            await Task.Run(() =>
            {
                Parallel.ForEach(indexedSegments.Values, file =>
                {
                    var duration = file.GetMediaDuration();
                    var timeSpan = TimeSpan.Parse(duration);
                    Interlocked.Add(ref totalDuration, (long)timeSpan.TotalSeconds);
                });
            });

            await indexedSegments.MergeTsSegmentsAsync(
                _segmentsRootPath, Guid.NewGuid().ToString("N"), totalDuration,
                new Progress<double>(progress => { MergeProgressValue = progress; })
            );
        }
    }
}