using System;
using System.Collections.Generic;
using System.Windows;
using m3u8_downloader.Models;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace m3u8_downloader.ViewModels
{
    public class MergeProgressDialogViewModel : BindableBase, IDialogAware
    {
        public string Title => "片段合并进度";

        public event Action<IDialogResult> RequestClose;

        public List<ArcLineSegment> Segments { get; set; }

        public MergeProgressDialogViewModel()
        {
            Segments = new List<ArcLineSegment>();
            // 每个分段的角度（360度 / 10 = 36度），减去2度作为间隙
            const int segmentAngle = 34;
            const int gapAngle = 2;

            for (var i = 0; i < 10; i++)
            {
                // 计算起始角度和结束角度
                var startAngle = i * (segmentAngle + gapAngle);
                var endAngle = startAngle + segmentAngle;

                // 转换为弧度
                var startRadians = (startAngle - 90) * Math.PI / 180; // 减去90度使起始点在顶部
                var endRadians = (endAngle - 90) * Math.PI / 180;

                // 计算起始点和结束点坐标（圆心为100,100，半径为100）
                var startPoint = new Point(
                    100 + 100 * Math.Cos(startRadians), 100 + 100 * Math.Sin(startRadians)
                );

                var endPoint = new Point(
                    100 + 100 * Math.Cos(endRadians), 100 + 100 * Math.Sin(endRadians)
                );
                Segments.Add(new ArcLineSegment
                {
                    StartPoint = startPoint,
                    EndPoint = endPoint,
                    IsLargeArc = segmentAngle > 180
                });
            }
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