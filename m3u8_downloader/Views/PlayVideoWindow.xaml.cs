using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace m3u8_downloader.Views
{
    public partial class PlayVideoWindow
    {
        private bool _isControllerVisible = true;
        private DispatcherTimer _controllerTimer;
        private bool _isDraggingProgress;

        public PlayVideoWindow(string videoPath)
        {
            InitializeComponent();
            if (FindResource("ShowControllerAnimation") is Storyboard showStoryboard)
            {
                showStoryboard.Begin();
                _controllerTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                _controllerTimer.Tick += Timer_Tick;
                _controllerTimer.Start();
            }

            PlayerRootWindow.MouseLeftButtonDown += PlayerRootWindow_MouseDown;

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

        private void Timer_Tick(object sender, EventArgs e)
        {
            _controllerTimer.Tick -= Timer_Tick;
            _controllerTimer.Stop();
            if (_isControllerVisible && FindResource("HideControllerAnimation") is Storyboard hideStoryboard)
            {
                hideStoryboard.Begin();
                _isControllerVisible = false;
            }
        }

        private void PlayerRootWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isControllerVisible)
            {
                _controllerTimer.Tick -= Timer_Tick;
                _controllerTimer.Stop();
                if (FindResource("HideControllerAnimation") is Storyboard hideStoryboard)
                {
                    hideStoryboard.Begin();
                }
            }
            else
            {
                if (FindResource("ShowControllerAnimation") is Storyboard showStoryboard)
                {
                    showStoryboard.Begin();
                    _controllerTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                    _controllerTimer.Tick += Timer_Tick;
                    _controllerTimer.Start();
                }
            }

            _isControllerVisible = !_isControllerVisible;
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