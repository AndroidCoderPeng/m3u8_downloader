using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using m3u8_downloader.Models;
using m3u8_downloader.Utils;
using m3u8_downloader.ViewModels;

namespace m3u8_downloader.Pages
{
    public partial class MergeSegmentPage : UserControl
    {
        private Point _dragStartPoint;
        private ListBoxItem _draggingItem;
        private AdornerLayer _adornerLayer;
        private DragAdorner _dragAdorner;

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

        /// <summary>
        /// 鼠标按下，确定要拖动的ListBoxItem，准备拖动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SegmentsListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(null);
            _draggingItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
        }

        /// <summary>
        /// 确定是否开始拖动
        /// </summary>
        private bool IsDraggingItem(MouseEventArgs e)
        {
            var mousePos = e.GetPosition(null);
            var diff = _dragStartPoint - mousePos;
            return Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                   Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance;
        }

        /// <summary>
        /// 拖动过程中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SegmentsListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggingItem == null) return;
            if (e.LeftButton == MouseButtonState.Pressed && IsDraggingItem(e))
            {
                if (!(sender is ItemsControl itemsControl)) return;
                // 创建拖动装饰器
                _adornerLayer = AdornerLayer.GetAdornerLayer(itemsControl);
                if (_adornerLayer != null)
                {
                    _dragAdorner = new DragAdorner(itemsControl, _draggingItem);
                    _adornerLayer.Add(_dragAdorner);
                }

                DragDropHelper.SetIsDragging(_draggingItem, true);
                DragDrop.DoDragDrop(_draggingItem, _draggingItem.DataContext, DragDropEffects.Move);

                // 清理
                if (_dragAdorner != null && _adornerLayer != null)
                {
                    _adornerLayer.Remove(_dragAdorner);
                    _dragAdorner = null;
                }

                if (_draggingItem == null) return;
                DragDropHelper.SetIsDragging(_draggingItem, false);
                _draggingItem = null;
            }
        }

        /// <summary>
        /// 处理拖动过程中的效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SegmentsListBox_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(SegmentFile)) || sender != e.Source)
            {
                e.Effects = DragDropEffects.None;
                return;
            }

            // 更新装饰器位置
            if (_dragAdorner != null)
            {
                var currentPosition = e.GetPosition(_adornerLayer);
                _dragAdorner.SetPosition(currentPosition.X, currentPosition.Y);
            }

            // 获取当前鼠标位置下的ListBoxItem
            var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (targetItem == null) return;

            // 获取目标项的位置和大小
            var itemRect = targetItem.TransformToAncestor(SegmentsListBox).TransformBounds(
                new Rect(0, 0, targetItem.ActualWidth, targetItem.ActualHeight)
            );

            // 计算插入位置
            if (e.GetPosition(SegmentsListBox).Y < itemRect.Top + itemRect.Height / 2)
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.Move;
            }

            e.Handled = true;
        }

        /// <summary>
        /// 处理最终拖动位置，放置拖过来的ListBoxItem，更新数据集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SegmentsListBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(SegmentFile)))
            {
                var draggedItem = e.Data.GetData(typeof(SegmentFile)) as SegmentFile;

                var vm = DataContext as MergeSegmentPageViewModel;
                if (vm == null) return;

                // 获取目标ListBoxItem
                var targetItem = FindAncestor<ListBoxItem>((DependencyObject)e.OriginalSource);
                if (targetItem == null) return;

                // 获取源和目标索引
                var sourceIndex = vm.ResourceSegments.IndexOf(draggedItem);
                if (sourceIndex == -1) return;

                var targetIndex = SegmentsListBox.ItemContainerGenerator.IndexFromContainer(targetItem);

                // 调整目标索引（基于鼠标位置是在项目的上半部分还是下半部分）
                var itemRect = targetItem.TransformToAncestor(SegmentsListBox).TransformBounds(
                    new Rect(0, 0, targetItem.ActualWidth, targetItem.ActualHeight)
                );

                if (e.GetPosition(SegmentsListBox).Y > itemRect.Top + itemRect.Height / 2)
                {
                    targetIndex++;
                }

                if (targetIndex < 0)
                {
                    targetIndex = 0;
                }

                if (targetIndex > vm.ResourceSegments.Count)
                {
                    targetIndex = vm.ResourceSegments.Count;
                }

                // 移动项目
                if (sourceIndex != targetIndex)
                {
                    try
                    {
                        vm.ResourceSegments.Insert(targetIndex, draggedItem);
                        if (sourceIndex < targetIndex)
                        {
                            vm.ResourceSegments.RemoveAt(sourceIndex);
                        }
                        else
                        {
                            vm.ResourceSegments.RemoveAt(sourceIndex + 1);
                        }

                        SegmentsListBox.SelectedItem = draggedItem;
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        MessageBox.Show($"移动项目时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T dependencyObject)
                {
                    return dependencyObject;
                }

                current = VisualTreeHelper.GetParent(current);
            } while (current != null);

            return null;
        }
    }
}