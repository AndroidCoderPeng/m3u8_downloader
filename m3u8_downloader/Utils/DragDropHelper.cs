using System.Windows;

namespace m3u8_downloader.Utils
{
    public class DragDropHelper
    {
        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.RegisterAttached(
            "IsDragging", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false)
        );

        public static void SetIsDragging(DependencyObject element, bool value)
        {
            element.SetValue(IsDraggingProperty, value);
        }

        public static bool GetIsDragging(DependencyObject element)
        {
            return (bool)element.GetValue(IsDraggingProperty);
        }
    }
}