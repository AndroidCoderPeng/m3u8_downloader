using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using m3u8_downloader.Models;
using m3u8_downloader.ViewModels;

namespace m3u8_downloader.Pages
{
    public partial class MergeSegmentPage : UserControl
    {
        private Point _startPoint;
        private bool _isDragging;

        public MergeSegmentPage()
        {
            InitializeComponent();
        }

        private void RootPathTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (RootPathTextBox.Text.Length != 0) return;
            var vm = DataContext as MergeSegmentPageViewModel;
            vm?.RootPathClearedCommand.Execute();
        }

        private void SegmentsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = false;
        }

        private void SegmentsListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || _isDragging) return;

            var mousePos = e.GetPosition(null);
            var diff = _startPoint - mousePos;

            // 当鼠标移动距离足够大时，开始拖拽
            if (!(Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance) &&
                !(Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)) return;

            // 找到鼠标下的 ListBoxItem
            var listBoxItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (listBoxItem.DataContext is SegmentFile segment)
            {
                try
                {
                    // 设置鼠标光标为手型
                    SegmentsListBox.Cursor = Cursors.Hand;
                    _isDragging = true;
                    DragDrop.DoDragDrop(listBoxItem, segment, DragDropEffects.Move);
                }
                finally
                {
                    // 恢复鼠标光标为默认形状
                    SegmentsListBox.Cursor = Cursors.Arrow;
                    _isDragging = false;
                }
            }
        }

        private void SegmentsListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(SegmentFile)))
            {
                var draggedItem = e.Data.GetData(typeof(SegmentFile)) as SegmentFile;
                if (draggedItem == null) return;

                var vm = DataContext as MergeSegmentPageViewModel;
                if (vm == null) return;

                // 获取拖拽源项目和目标项目的索引
                var sourceIndex = vm.ResourceSegments.IndexOf(draggedItem);
                if (sourceIndex < 0) return;

                // 获取鼠标在 ListBox 中的位置
                var dropPosition = e.GetPosition(SegmentsListBox);
                var droppedOnItem = GetItemContainerAt(SegmentsListBox, dropPosition);

                var targetIndex = -1;

                if (droppedOnItem != null)
                {
                    if (droppedOnItem is ListBoxItem targetItem)
                    {
                        targetIndex = SegmentsListBox.ItemContainerGenerator.IndexFromContainer(targetItem);

                        // 根据鼠标位置调整目标索引（上半部分或下半部分）
                        var positionInItem = e.GetPosition(targetItem);
                        if (positionInItem.Y < targetItem.ActualHeight / 2)
                        {
                            // 放在项目上方
                            if (targetIndex > sourceIndex)
                            {
                                targetIndex--;
                            }
                        }
                        else
                        {
                            // 放在项目下方
                            if (targetIndex <= sourceIndex)
                            {
                                targetIndex++;
                            }
                        }
                    }
                }
                else
                {
                    // 如果没找到具体项目，拖到了列表末尾
                    targetIndex = vm.ResourceSegments.Count;
                }

                // 验证目标索引是否在有效范围内
                targetIndex = Math.Max(0, Math.Min(targetIndex, vm.ResourceSegments.Count));

                // 调整项目位置
                if (vm.ResourceSegments.Count > 0 && sourceIndex != targetIndex)
                {
                    // 防止移动后索引无效
                    if (sourceIndex < targetIndex)
                    {
                        targetIndex--;
                    }

                    // 再次验证索引
                    if (targetIndex < vm.ResourceSegments.Count)
                    {
                        vm.ResourceSegments.Move(sourceIndex, targetIndex);
                    }
                    else
                    {
                        // 如果索引仍然无效，改为添加到末尾
                        vm.ResourceSegments.RemoveAt(sourceIndex);
                        vm.ResourceSegments.Add(draggedItem);
                    }
                }
            }
        }

        private void SegmentsListBox_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(SegmentFile)) || sender != e.Source)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            if (sender == e.Source)
            {
                e.Effects = DragDropEffects.Move;
                // 确保拖拽过程中鼠标光标保持为手型
                if (SegmentsListBox.Cursor != Cursors.Hand)
                {
                    SegmentsListBox.Cursor = Cursors.Hand;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void SegmentsListBox_DragLeave(object sender, DragEventArgs e)
        {
            if (!SegmentsListBox.IsMouseOver)
            {
                SegmentsListBox.Cursor = Cursors.Arrow;
            }
        }

        private void SegmentsListBox_MouseLeave(object sender, MouseEventArgs e)
        {
            // 如果不是在拖拽过程中，恢复鼠标光标
            if (!_isDragging)
            {
                SegmentsListBox.Cursor = Cursors.Arrow;
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(current);
            if (parent == null) return null;

            if (parent is T)
            {
                return (T)parent;
            }

            return FindAncestor<T>(parent);
        }

        private UIElement GetItemContainerAt(ListBox listBox, Point position)
        {
            var hitTestResult = VisualTreeHelper.HitTest(listBox, position);
            if (hitTestResult == null)
                return null;

            var hitObject = hitTestResult.VisualHit;
            while (hitObject != null && !(hitObject is ListBoxItem))
            {
                hitObject = VisualTreeHelper.GetParent(hitObject);
            }

            return hitObject as UIElement;
        }
    }
}