using System.Windows;

namespace m3u8_downloader.Models
{
    public class ArcLineSegment
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }
        public bool IsLargeArc { get; set; }
    }
}