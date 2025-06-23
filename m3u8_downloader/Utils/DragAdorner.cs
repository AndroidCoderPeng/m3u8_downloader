using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace m3u8_downloader.Utils
{
    public class DragAdorner : Adorner
    {
        private readonly UIElement _child;
        private double _offsetX;
        private double _offsetY;

        public DragAdorner(UIElement adornedElement, UIElement dragElement) : base(adornedElement)
        {
            var brush = new VisualBrush(dragElement);

            // 创建一个与被拖动项相同大小的矩形
            var rect = new Rectangle
            {
                Width = dragElement.RenderSize.Width,
                Height = dragElement.RenderSize.Height,
                Fill = brush,
                Opacity = 0.7,
                Effect = new DropShadowEffect
                {
                    BlurRadius = 15,
                    Color = Colors.Black,
                    Opacity = 0.3,
                    ShadowDepth = 0
                }
            };

            _child = rect;
        }

        public void SetPosition(double x, double y)
        {
            _offsetX = x;
            _offsetY = y;

            if (Parent is AdornerLayer adornerLayer)
            {
                adornerLayer.Update(AdornedElement);
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        protected override int VisualChildrenCount => 1;

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(_child.DesiredSize));
            return finalSize;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform) ?? throw new InvalidOperationException());
            result.Children.Add(
                new TranslateTransform(
                    _offsetX - _child.DesiredSize.Width,
                    _offsetY - _child.DesiredSize.Height
                )
            );
            return result;
        }
    }
}