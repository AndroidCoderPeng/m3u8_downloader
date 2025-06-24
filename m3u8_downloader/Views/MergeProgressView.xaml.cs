using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace m3u8_downloader.Views
{
    public partial class MergeProgressView 
    {
        private const int SegmentCount = 36;
        private const double TotalAngle = 360;
        private const double GapAngle = 1;
        private readonly Path _progressPath = new Path();
        
        public MergeProgressView()
        {
            InitializeComponent();

            var radius = CirclePathCanvas.Width / 2;
            var centerX = CirclePathCanvas.Width / 2;
            var centerY = CirclePathCanvas.Width / 2;

            //实际的圆弧段对应的的圆心角角度
            const double segmentAngle = TotalAngle / SegmentCount - GapAngle;
            for (var i = 0; i < SegmentCount; i++)
            {
                var startAngle = i * (TotalAngle / SegmentCount) + i * GapAngle;
                var endAngle = startAngle + segmentAngle;

                var path = CreateArcSegment(centerX, centerY, radius, startAngle, endAngle);
                CirclePathCanvas.Children.Add(path);
            }
            
            _progressPath.Stroke = (Brush)FindResource("AppThemeBrush");
            _progressPath.StrokeThickness = 15;
        }

        private Path CreateArcSegment(double centerX, double centerY, double radius, double startAngle, double endAngle)
        {
            var path = new Path();
            var pathGeometry = new PathGeometry();
            var pathFigure = new PathFigure();
            pathGeometry.Figures.Add(pathFigure);

            // 起始点
            var startPoint = GetPointOnCircle(centerX, centerY, radius, startAngle);
            pathFigure.StartPoint = startPoint;

            // 圆弧段
            var arcSegment = new ArcSegment
            {
                Point = GetPointOnCircle(centerX, centerY, radius, endAngle),
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = endAngle - startAngle > 180
            };
            pathFigure.Segments.Add(arcSegment);

            // 设置路径的样式
            path.Stroke = (Brush)FindResource("AppBorderBrush");
            path.StrokeThickness = 15;
            path.Data = pathGeometry;

            return path;
        }

        private static Point GetPointOnCircle(double centerX, double centerY, double radius, double angle)
        {
            var angleInRadians = angle * Math.PI / 180.0;
            var x = centerX + radius * Math.Cos(angleInRadians);
            var y = centerY + radius * Math.Sin(angleInRadians);
            return new Point(x, y);
        }
        
        public void UpdateProgress(double value)
        {
            ProgressTextBlock.Text = $@"{value:F2}%";
            
            //更新弧度
            
            
            if (value >= 100)
            {
                Close();
            }
        }
    }
}