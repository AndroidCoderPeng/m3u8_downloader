using System;

namespace m3u8_downloader.Models
{
    public class VideoFile
    {
        /// <summary>
        /// 封面
        /// </summary>
        public string CoverImage { set; get; }

        /// <summary>
        /// 视频名
        /// </summary>
        public string VideoName { set; get; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { set; get; }

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