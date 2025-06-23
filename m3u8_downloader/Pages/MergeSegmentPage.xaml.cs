using System.Windows.Controls;
using m3u8_downloader.ViewModels;

namespace m3u8_downloader.Pages
{
    public partial class MergeSegmentPage : UserControl
    {
        public MergeSegmentPage()
        {
            InitializeComponent();
        }

        private void RootPathTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (RootPathTextBox.Text.Length == 0)
            {
                var vm = DataContext as MergeSegmentPageViewModel;
                vm?.RootPathClearedCommand.Execute();
            }
        }
    }
}