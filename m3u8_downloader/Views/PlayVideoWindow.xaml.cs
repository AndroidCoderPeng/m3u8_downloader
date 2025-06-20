using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
                _controllerTimer.Tick += ControllerTimer_Tick;
                _controllerTimer.Start();
            }

            PlayerRootWindow.MouseLeftButtonDown += PlayerRootWindow_MouseDown;

            try
            {
                VideoPlayerElement.Source = new Uri(videoPath);
                Title = $"{Path.GetFileName(videoPath)}";

                var durationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                durationTimer.Tick += DurationTimer_Tick;
                durationTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法加载视频: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ControllerTimer_Tick(object sender, EventArgs e)
        {
            _controllerTimer.Tick -= ControllerTimer_Tick;
            _controllerTimer.Stop();
            if (_isControllerVisible && FindResource("HideControllerAnimation") is Storyboard hideStoryboard)
            {
                hideStoryboard.Begin();
                _isControllerVisible = false;
            }
        }

        private void DurationTimer_Tick(object sender, EventArgs e)
        {
            if (VideoPlayerElement.NaturalDuration.HasTimeSpan && !_isDraggingProgress)
            {
                DurationSlider.Maximum = VideoPlayerElement.NaturalDuration.TimeSpan.TotalSeconds;

                var position = VideoPlayerElement.Position;
                PositionTextBlock.Text = $"{position.Hours:D2}:{position.Minutes:D2}:{position.Seconds:D2}";
                DurationSlider.Value = position.TotalSeconds;
            }
        }

        private void PlayerRootWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isControllerVisible)
            {
                _controllerTimer.Tick -= ControllerTimer_Tick;
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
                    _controllerTimer.Tick += ControllerTimer_Tick;
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

            if (VideoPlayerElement.NaturalDuration.HasTimeSpan)
            {
                PlayButton.Content = new TextBlock { Text = "\ue6fc" };
                DurationTextBlock.Text = $"{VideoPlayerElement.NaturalDuration}";
            }
        }

        private void VideoPlayerElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"视频播放结束");
        }
    }
}