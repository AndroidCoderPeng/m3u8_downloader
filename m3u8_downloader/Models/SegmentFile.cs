using System;

namespace m3u8_downloader.Models
{
    public class SegmentFile
    {
        public string CoverImage { set; get; }
        public string SegmentName { set; get; }
        public string FilePath { set; get; }
        public string Duration { set; get; }
        public string SegmentSize { set; get; }

        /// <summary>
        /// 文件修改时间，用于缓存有效性检查
        /// </summary>
        public DateTime LastModified { get; set; }
    }
}