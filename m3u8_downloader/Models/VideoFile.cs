using System;
using Prism.Mvvm;

namespace m3u8_downloader.Models
{
    public class VideoFile : BindableBase
    {
        /// <summary>
        /// 封面
        /// </summary>
        public string CoverImage { set; get; }

        /// <summary>
        /// 视频名
        /// </summary>
        private string _videoName;

        public string VideoName
        {
            get => _videoName;
            set => SetProperty(ref _videoName, value);
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        private string _filePath;

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        /// <summary>
        /// 分辨率
        /// </summary>
        public string Resolution { set; get; }

        /// <summary>
        /// 视频大小
        /// </summary>
        public string VideoSize { set; get; }

        /// <summary>
        /// 时长
        /// </summary>
        public string Duration { set; get; }

        /// <summary>
        /// 文件修改时间，用于缓存有效性检查
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}