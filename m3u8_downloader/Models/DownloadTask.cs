using Prism.Mvvm;

namespace m3u8_downloader.Models
{
    public class DownloadTask : BindableBase
    {
        /// <summary>
        /// 下载视频的名字
        /// </summary>
        private string _taskName;

        public string TaskName
        {
            get => _taskName;
            set => SetProperty(ref _taskName, value);
        }

        /// <summary>
        /// m3u8地址
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// ts片段数量
        /// </summary>
        private int _totalSegments;

        public int TotalSegments
        {
            get => _totalSegments;
            set => SetProperty(ref _totalSegments, value);
        }

        /// <summary>
        /// 已下载ts片段数量
        /// </summary>
        private int _downloadedSegments;

        public int DownloadedSegments
        {
            get => _downloadedSegments;
            set => SetProperty(ref _downloadedSegments, value);
        }

        /// <summary>
        /// 视频时长
        /// </summary>
        private string _duration;

        public string Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        /// <summary>
        /// 视频大小
        /// </summary>
        private string _totalSize;

        public string TotalSize
        {
            get => _totalSize;
            set => SetProperty(ref _totalSize, value);
        }

        /// <summary>
        /// 下载进度
        /// </summary>
        private double _percentComplete;

        public double PercentComplete
        {
            get => _percentComplete;
            set => SetProperty(ref _percentComplete, value);
        }

        /// <summary>
        /// 任务状态
        /// </summary>
        private string _taskState;

        public string TaskState
        {
            get => _taskState;
            set => SetProperty(ref _taskState, value);
        }
    }
}