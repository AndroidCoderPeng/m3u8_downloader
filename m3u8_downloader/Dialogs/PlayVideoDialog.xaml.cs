namespace m3u8_downloader.Dialogs
{
    public partial class PlayVideoDialog
    {
        private bool _isDraggingProgress;
        private readonly string _currentFilePath;

        public PlayVideoDialog(string videoPath)
        {
            InitializeComponent();
            _currentFilePath = videoPath;
        }
    }
}