using System;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace m3u8_downloader.ViewModels
{
    public class MergeProgressDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "片段合并进度";
        
        public event Action<IDialogResult> RequestClose;

        public MergeProgressDialogViewModel()
        {
            
        }

        public void UpdateProgress(double value)
        {
            // ProgressTextBlock.Text = $@"{value:F2}%";
            // var angle = 360 * (value / 100);
            // Console.WriteLine(angle);
            // var x = 1 * Math.Cos(Math.PI * (angle / 180 - 90) / 180 * Math.PI);
            // var y = 1 * Math.Sin(Math.PI * (angle / 180 - 90) / 180 * Math.PI);
            // ArcSegment.Point = new Point(x, y);
            // ArcSegment.IsLargeArc = angle > 180;
            // if (value >= 100)
            // {
            //     Close();
            // }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}