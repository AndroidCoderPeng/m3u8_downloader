using System;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
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
        private bool _isPlaying;
        private bool _isCompleted;
        private double _lastVolume = 0.5;

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
                Title = $"{Path.GetFileName(videoPath)}";
                VideoPlayerElement.Source = new Uri(videoPath);
                VideoPlayerElement.Play();
                _isPlaying = true;

                var durationTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                durationTimer.Tick += DurationTimer_Tick;
                durationTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法加载视频: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            RewindButton.Click += delegate { VideoPlayerElement.Position -= TimeSpan.FromSeconds(5); };
            PlayButton.Click += delegate { ControlVideoState(); };
            ForwardButton.Click += delegate { VideoPlayerElement.Position += TimeSpan.FromSeconds(5); };
            VoiceButton.Click += delegate
            {
                if (VideoPlayerElement.Volume == 0)
                {
                    VoiceSlider.Value = _lastVolume;
                }
                else
                {
                    _lastVolume = VideoPlayerElement.Volume;
                    VoiceSlider.Value = 0;
                }
            };
            VoiceSlider.ValueChanged += VoiceSlider_ValueChanged;
            ExpendButton.Click += delegate
            {
                WindowState = WindowState.Maximized;
                WindowStyle = WindowStyle.None;
                ExpendButton.Content = "\ue683";
            };
        }

        private void ControlVideoState()
        {
            if (_isCompleted)
            {
                VideoPlayerElement.Position = TimeSpan.Zero;
                VideoPlayerElement.Play();
                PlayButton.Content = "\ue6fc";
                _isPlaying = true;
                _isCompleted = false;
            }
            else
            {
                if (_isPlaying)
                {
                    VideoPlayerElement.Pause();
                    PlayButton.Content = "\ue6c2";
                    _isPlaying = false;
                }
                else
                {
                    VideoPlayerElement.Play();
                    PlayButton.Content = "\ue6fc";
                    _isPlaying = true;
                }
            }
        }

        private void VoiceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue == 0)
            {
                VoiceButton.Content = "\ue6d8";
            }
            else if (e.NewValue > 0 && e.NewValue < 0.5)
            {
                VoiceButton.Content = "\ue71d";
            }
            else
            {
                VoiceButton.Content = "\ue71e";
            }

            VideoPlayerElement.Volume = e.NewValue;
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
                PlayButton.Content = "\ue6fc";
                DurationTextBlock.Text = $"{VideoPlayerElement.NaturalDuration}";
                VoiceSlider.Value = VideoPlayerElement.Volume;
            }
        }

        private void DurationSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            _isDraggingProgress = true;
        }

        private void DurationSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            _isDraggingProgress = false;
            VideoPlayerElement.Position = TimeSpan.FromSeconds(DurationSlider.Value);
        }

        private void VideoPlayerElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"视频播放结束");
            PlayButton.Content = "\ue6c2";
            _isPlaying = false;
            _isCompleted = true;
        }

        private void PlayVideoWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    VideoPlayerElement.Position -= TimeSpan.FromSeconds(5);
                    break;
                case Key.Right:
                    VideoPlayerElement.Position += TimeSpan.FromSeconds(5);
                    break;
                case Key.Space:
                    ControlVideoState();
                    break;
                case Key.Escape:
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                        WindowStyle = WindowStyle.SingleBorderWindow;
                        ExpendButton.Content = "\ue60c";
                    }
                    else
                    {
                        Close();
                    }
                    break;
                case Key.F11:
                    WindowState = WindowState.Maximized;
                    WindowStyle = WindowStyle.None;
                    ExpendButton.Content = "\ue683";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}