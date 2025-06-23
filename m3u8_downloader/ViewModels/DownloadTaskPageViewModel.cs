using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using m3u8_downloader.Models;
using m3u8_downloader.Service;
using m3u8_downloader.Utils;
using m3u8_downloader.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Application = System.Windows.Application;
using DialogResult = System.Windows.Forms.DialogResult;
using MessageBox = System.Windows.Forms.MessageBox;

namespace m3u8_downloader.ViewModels
{
    public class DownloadTaskPageViewModel : BindableBase
    {
        public DelegateCommand ParseUrlCommand { set; get; }
        public DelegateCommand<string> MouseDoubleClickCommand { set; get; }
        public DelegateCommand<DownloadTask> EditTaskCommand { set; get; }
        public DelegateCommand<string> DeleteTaskCommand { set; get; }

        // private string _m3u8Url = "https://t30.cdn2020.com/video/m3u8/2025/06/20/be401fc5/index.m3u8";
        private string _m3u8Url = "https://vip5.lbb2025.com/20250618/11qELL1u/2000kb/hls/index.m3u8";
        // private string _m3u8Url = "https://hls.qzkj.tech/videos5/190685a0ddb687c902cd8307afbddfc1/190685a0ddb687c902cd8307afbddfc1.m3u8?auth_key=1750497321-685678291b7fd-0-6d345de8ebc2336083b3dc50fc316dec&v=3&time=0";

        public string M3U8Url
        {
            set
            {
                _m3u8Url = value;
                RaisePropertyChanged();
            }
            get => _m3u8Url;
        }

        private ObservableCollection<DownloadTask> _downloadTaskSource = new ObservableCollection<DownloadTask>();

        public ObservableCollection<DownloadTask> DownloadTaskSource
        {
            set
            {
                _downloadTaskSource = value;
                RaisePropertyChanged();
            }
            get => _downloadTaskSource;
        }

        private Visibility _isDownloadTaskVisible = Visibility.Collapsed;

        public Visibility IsDownloadTaskVisible
        {
            get => _isDownloadTaskVisible;
            set
            {
                _isDownloadTaskVisible = value;
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

        private readonly IAppDataService _dataService;

        public DownloadTaskPageViewModel(IAppDataService dataService, IDialogService dialogService)
        {
            _dataService = dataService;
            ParseUrlCommand = new DelegateCommand(delegate
            {
                if (_downloadTaskSource.Any(x => x.Url.Equals(_m3u8Url)))
                {
                    MessageBox.Show(@"该任务已存在", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var task = new DownloadTask
                {
                    TaskName = Guid.NewGuid().ToString("N"),
                    Url = _m3u8Url,
                    TotalSegments = 0,
                    Duration = "00:00:00",
                    TotalSize = "0 MB",
                    PercentComplete = 0.0,
                    TaskState = "连接中"
                };
                DownloadTaskSource.Add(task);
                IsEmptyImageVisible = Visibility.Collapsed;
                IsDownloadTaskVisible = Visibility.Visible;

                // 解析m3u8片段
                ParseResourceAsync(task);
            });

            MouseDoubleClickCommand = new DelegateCommand<string>(name =>
            {
                var folder = _dataService.GetValue("VideoFolder") as string;
                if (string.IsNullOrEmpty(folder))
                {
                    MessageBox.Show(@"请先设置保存目录", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var filePath = Path.Combine(folder, $"{name}.mp4");
                Console.WriteLine(filePath);
                if (File.Exists(filePath))
                {
                    MessageBox.Show(@"视频文件不存在，请先下载", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                new PlayVideoWindow(filePath) { Owner = Application.Current.MainWindow }.ShowDialog();
            });

            EditTaskCommand = new DelegateCommand<DownloadTask>(it =>
            {
                var dialogParameters = new DialogParameters
                {
                    { "TaskName", it.TaskName }
                };
                dialogService.ShowDialog("EditTaskNameDialog", dialogParameters, delegate(IDialogResult result)
                {
                    if (result.Result != ButtonResult.OK) return;
                    it.TaskName = result.Parameters.GetValue<string>("TaskName");
                });
            });

            DeleteTaskCommand = new DelegateCommand<string>(url =>
            {
                var dialogResult = MessageBox.Show(
                    @"要删除该任务以及下载的资源吗？", @"提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question
                );
                if (dialogResult == DialogResult.No) return;
                var task = _downloadTaskSource.FirstOrDefault(x => x.Url.Equals(url));
                if (task == null) return;
                DownloadTaskSource.Remove(task);
                if (_downloadTaskSource.Any())
                {
                    IsEmptyImageVisible = Visibility.Collapsed;
                    IsDownloadTaskVisible = Visibility.Visible;
                }
                else
                {
                    IsEmptyImageVisible = Visibility.Visible;
                    IsDownloadTaskVisible = Visibility.Collapsed;
                }

                var folder = _dataService.GetValue("VideoFolder") as string;
                if (string.IsNullOrEmpty(folder))
                {
                    MessageBox.Show(@"请先设置保存目录", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var filePath = Path.Combine(folder, $"{task.TaskName}.mp4");
                if (!File.Exists(filePath)) return;
                File.Delete(filePath);
            });
        }

        private async void ParseResourceAsync(DownloadTask task)
        {
            var (segments, duration, dictionary) = await _m3u8Url.ParseVideoResourceAsync();
            var durationTime = TimeSpan.FromSeconds(duration).ToString(@"hh\:mm\:ss");
            task.TotalSegments = segments.Count;
            task.Duration = durationTime;
            task.TaskState = "下载中";

            var folder = _dataService.GetValue("VideoFolder") as string;
            if (string.IsNullOrEmpty(folder))
            {
                MessageBox.Show(@"请先设置保存目录", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ConcurrentDictionary<int, string> indexedFiles;
            if (!dictionary.Any())
            {
                indexedFiles = await segments.DownloadTsSegmentsAsync(folder, new Progress<TaskProgress>(progress =>
                    {
                        task.TotalSize = $"{progress.TotalBytes / 1024.0 / 1024.0:N2} MB";
                        task.DownloadedSegments = progress.DownloadedSegments;
                        task.PercentComplete = progress.PercentComplete;
                    }
                ));
            }
            else
            {
                var key = dictionary["URI"];
                var iv = dictionary["IV"];
                indexedFiles = await segments.DownloadAndDecryptTsSegmentAsync(key.GetByteArray(), iv.ToByteArray(),
                    folder, new Progress<TaskProgress>(progress =>
                        {
                            task.TotalSize = $"{progress.TotalBytes / 1024.0 / 1024.0:N2} MB";
                            task.DownloadedSegments = progress.DownloadedSegments;
                            task.PercentComplete = progress.PercentComplete;
                        }
                    )
                );
            }

            task.TaskState = "合并中";
            await indexedFiles.MergeTsSegmentsAsync(folder, task.TaskName);
            await folder.DeleteTsSegments();
            task.TaskState = "下载完成";
        }
    }
}