using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HandyControl.Controls;
using m3u8_downloader.Dialogs;
using m3u8_downloader.Models;
using m3u8_downloader.Utils;
using m3u8_downloader.Views;
using Prism.Commands;
using Prism.Mvvm;
using Application = System.Windows.Application;
using DialogResult = System.Windows.Forms.DialogResult;
using MessageBox = System.Windows.Forms.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace m3u8_downloader.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public DelegateCommand OpenFileCommand { set; get; }
        public DelegateCommand SelectFolderCommand { set; get; }
        public DelegateCommand ExitApplicationCommand { set; get; }
        public DelegateCommand ShowDocumentCommand { set; get; }
        public DelegateCommand ShowAboutCommand { set; get; }
        public DelegateCommand ParseUrlCommand { set; get; }
        public DelegateCommand<string> PlayVideoCommand { set; get; }
        public DelegateCommand<string> DeleteTaskCommand { set; get; }

        private string _m3u8Url = "https://t30.cdn2020.com/video/m3u8/2025/06/10/5b80adba/index.m3u8";

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

        public MainWindowViewModel()
        {
            OpenFileCommand = new DelegateCommand(() =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "打开 M3U8 文件",
                    Filter = "M3U8 文件 (*.m3u8)|*.m3u8"
                };

                if (openFileDialog.ShowDialog() != true) return;
                var fileName = openFileDialog.FileName;
                // TODO: 打开文件逻辑
            });

            SelectFolderCommand = new DelegateCommand(() =>
            {
                var folderDialog = new FolderBrowserDialog
                {
                    Description = @"选择视频文件保存目录",
                    RootFolder = Environment.SpecialFolder.Desktop
                };

                if (folderDialog.ShowDialog() != DialogResult.OK) return;
                var folderPath = folderDialog.SelectedPath;
                if (string.IsNullOrEmpty(folderPath)) return;
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["VideoFolder"].Value = folderPath;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                Growl.Success("路径设置成功");
            });

            ExitApplicationCommand = new DelegateCommand(() => { Application.Current.Shutdown(); });

            ShowDocumentCommand = new DelegateCommand(() => { });

            ShowAboutCommand = new DelegateCommand(() =>
            {
                new AboutSoftwareDialog { Owner = Application.Current.MainWindow }.ShowDialog();
            });

            ParseUrlCommand = new DelegateCommand(() =>
            {
                var filePath = "三文鱼".IsVideoExists();
                if (filePath == string.Empty)
                {
                    MessageBox.Show(@"视频文件不存在，请先下载", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                new PlayVideoWindow(filePath){ Owner = Application.Current.MainWindow }.ShowDialog();
            });

            PlayVideoCommand = new DelegateCommand<string>(name =>
            {
                var filePath = name.IsVideoExists();
                if (filePath == string.Empty)
                {
                    MessageBox.Show(@"视频文件不存在，请先下载", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                new PlayVideoWindow(filePath){ Owner = Application.Current.MainWindow }.ShowDialog();
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
                var filePath = task.TaskName.IsVideoExists();
                if (filePath != string.Empty)
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
                _m3u8Url = _m3u8Url.ExtractM3U8Resource();
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
                PercentComplete = "0%",
                TaskState = "连接中"
            };
            DownloadTaskSource.Add(task);

            // 解析m3u8片段
            var (segments, duration) = await _m3u8Url.ParseVideoResourceAsync();
            var durationTime = TimeSpan.FromSeconds(duration).ToString(@"hh\:mm\:ss");
            task.TotalSegments = segments.Count;
            task.Duration = durationTime;
            task.TaskState = "下载中";

            var outputFolder = ConfigurationManager.AppSettings["VideoFolder"];
            if (string.IsNullOrEmpty(outputFolder))
            {
                MessageBox.Show(@"请先设置保存目录", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            await segments.DownloadTsSegmentsAsync(outputFolder, new Progress<TaskProgress>(progress =>
                {
                    task.TotalSize = $"{progress.TotalBytes / 1024.0 / 1024.0:N2} MB";
                    task.PercentComplete = $"{progress.PercentComplete}%";
                }
            ));
            
            task.TaskState = "合并中";
            await outputFolder.MergeTsSegmentsAsync(task.TaskName);
            await outputFolder.DeleteTsSegments();
            task.TaskState = "下载完成";
        }
    }
}