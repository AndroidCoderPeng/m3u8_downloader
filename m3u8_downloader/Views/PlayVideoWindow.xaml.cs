using System;
using System.IO;
using System.Windows;

namespace m3u8_downloader.Views
{
    public partial class PlayVideoWindow
    {
        private bool _isDraggingProgress;

        public PlayVideoWindow(string videoPath)
        {
            InitializeComponent();
            try
            {
                VideoPlayerElement.Source = new Uri(videoPath);
                Title = $"{Path.GetFileName(videoPath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法加载视频: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 视频加载完成时的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoPlayerElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            // 获取视频的原始宽高
            double videoWidth = VideoPlayerElement.NaturalVideoWidth;
            double videoHeight = VideoPlayerElement.NaturalVideoHeight;
            PlayerRootWindow.SizeToContent = videoWidth > videoHeight ? SizeToContent.Height : SizeToContent.Width;

            var duration = VideoPlayerElement.NaturalDuration;
            DurationSlider.Maximum = duration.TimeSpan.Seconds;
            DurationTextBlock.Text = $"{duration}";
        }

        private void VideoPlayerElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // 视频播放结束时的处理
            Console.WriteLine(@"视频播放结束");
        }
    }
}