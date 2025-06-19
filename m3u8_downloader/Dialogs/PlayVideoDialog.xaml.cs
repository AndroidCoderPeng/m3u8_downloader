using System;

namespace m3u8_downloader.Dialogs
{
    public partial class PlayVideoDialog
    {
        public PlayVideoDialog(string videoPath)
        {
            InitializeComponent();
            Console.WriteLine(videoPath);
        }
    }
}