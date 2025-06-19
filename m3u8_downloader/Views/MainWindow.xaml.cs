using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace m3u8_downloader.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileMenuItem_OnSubmenuOpened(object sender, RoutedEventArgs e)
        {
            ResetPopupPosition(sender);
        }

        private void HelpMenuItem_OnSubmenuOpened(object sender, RoutedEventArgs e)
        {
            ResetPopupPosition(sender);
        }

        private void ResetPopupPosition(object sender)
        {
            if (sender is MenuItem menuItem && menuItem.Template != null)
            {
                if (menuItem.Template.FindName("PART_Popup", menuItem) is Popup popup)
                {
                    popup.Placement = PlacementMode.Left;
                    popup.HorizontalOffset = -menuItem.ActualWidth;
                    popup.VerticalOffset = menuItem.ActualHeight;
                }
            }
        }
    }
}