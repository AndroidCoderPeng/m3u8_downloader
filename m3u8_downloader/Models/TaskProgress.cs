namespace m3u8_downloader.Models
{
    public class TaskProgress
    {
        public int TotalSegments { get; set; }
        public int DownloadedSegments { get; set; }
        public long TotalBytes { get; set; }
        public int PercentComplete { get; set; }
    }
}