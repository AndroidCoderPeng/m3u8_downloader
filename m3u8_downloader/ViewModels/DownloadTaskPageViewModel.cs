using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using m3u8_downloader.Models;
using m3u8_downloader.Service;
using m3u8_downloader.Utils;
using m3u8_downloader.Views;
using Prism.Commands;
using Prism.Mvvm;
using Application = System.Windows.Application;
using DialogResult = System.Windows.Forms.DialogResult;
using MessageBox = System.Windows.Forms.MessageBox;

namespace m3u8_downloader.ViewModels
{
    public class DownloadTaskPageViewModel : BindableBase
    {
        public DelegateCommand ParseUrlCommand { set; get; }
        public DelegateCommand<string> MouseDoubleClickCommand { set; get; }
        public DelegateCommand EditTaskCommand { set; get; }
        public DelegateCommand<string> DeleteTaskCommand { set; get; }

        private string _m3u8Url = "https://t30.cdn2020.com/video/m3u8/2025/06/10/5b80adba/index.m3u8";
        // private string _m3u8Url = "https://m.dongludi.cc/x/0-1225190.html";
        // private string _m3u8Url = "https://m.dongludi.cc/x/0-1225140.html";

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

        private readonly IAppDataService _dataService;

        public DownloadTaskPageViewModel(IAppDataService dataService)
        {
            _dataService = dataService;
            ParseUrlCommand = new DelegateCommand(ParseUrl);

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

            EditTaskCommand = new DelegateCommand(() => { Console.WriteLine("EditTaskCommand"); });

            DeleteTaskCommand = new DelegateCommand<string>(url =>
            {
                var dialogResult = MessageBox.Show(
                    @"要删除该任务以及下载的资源吗？", @"提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question
                );
                if (dialogResult == DialogResult.No) return;
                var task = _downloadTaskSource.FirstOrDefault(x => x.Url.Equals(url));
                if (task == null) return;
                DownloadTaskSource.Remove(task);
                
                var folder = _dataService.GetValue("VideoFolder") as string;
                if (string.IsNullOrEmpty(folder))
                {
                    MessageBox.Show(@"请先设置保存目录", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var filePath = Path.Combine(folder, $"{task.TaskName}.mp4");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            });
        }

        private async void ParseUrl()
        {
            if (_m3u8Url.EndsWith(".html"))
            {
                // 如果是 HTML 页面，尝试提取 M3U8 资源
                var urls = await _m3u8Url.ExtractM3U8Resource();
                if (!urls.Any())
                {
                    MessageBox.Show(@"无法从 HTML 页面中提取 M3U8 资源", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                foreach (var url in urls)
                {
                    Console.WriteLine(url);
                }
            }

            if (!_m3u8Url.EndsWith(".m3u8"))
            {
                MessageBox.Show(@"请输入正确的 M3U8 文件地址", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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

            // 解析m3u8片段
            var (segments, duration) = await _m3u8Url.ParseVideoResourceAsync();
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

            await segments.DownloadTsSegmentsAsync(folder, new Progress<TaskProgress>(progress =>
                {
                    task.TotalSize = $"{progress.TotalBytes / 1024.0 / 1024.0:N2} MB";
                    task.DownloadedSegments = progress.DownloadedSegments;
                    task.PercentComplete = progress.PercentComplete;
                }
            ));

            task.TaskState = "合并中";
            await folder.MergeTsSegmentsAsync(task.TaskName);
            await folder.DeleteTsSegments();
            task.TaskState = "下载完成";
        }
    }
}