using boilersGraphics.Exceptions;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Microsoft.Xaml.Behaviors;
using NLog;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Views.Behaviors
{
    public class DropArguments
    {
        public LayerTreeViewItemBase Source { get; set; }
        public LayerTreeViewItemBase Target { get; set; }
        public MoveableTreeViewBehavior.InsertType Type { get; set; }
    }

    public class MoveableTreeViewBehavior : Behavior<TreeView>
    {

        public enum InsertType
        {
            After,
            Before,
            Children
        }

        private readonly HashSet<LayerTreeViewItemBase> _changedBlocks = new HashSet<LayerTreeViewItemBase>();
        private InsertType _insertType;
        private Point? _startPos;

        #region DropCommand
        public ICommand DropCommand
        {
            get => (ICommand)GetValue(DropCommandProperty);
            set => SetValue(DropCommandProperty, value);
        }

        public static readonly DependencyProperty DropCommandProperty = DependencyProperty.Register(
            "DropCommand",
            typeof(ICommand),
            typeof(MoveableTreeViewBehavior),
            new UIPropertyMetadata(null));
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AllowDrop = true;
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
            AssociatedObject.Drop += OnDrop;
            AssociatedObject.DragOver += OnDragOver;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.AllowDrop = false;
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
            AssociatedObject.Drop -= OnDrop;
            AssociatedObject.DragOver -= OnDragOver;
        }

        private LayerTreeViewItemBase EitherNotNull(LayerTreeViewItemBase left, LayerTreeViewItemBase right)
        {
            if (left != null && right != null)
                throw new UnexpectedException("must not be left != null && right != null");
            else if (left != null)
                return left;
            else if (right != null)
                return right;
            else
                throw new UnexpectedException("must be left != null || right != null");
        }

        private bool isDragOver = false;

        private void OnDragOver(object sender, DragEventArgs e)
        {
            ResetSeparator(_changedBlocks);

            if (!(sender is ItemsControl itemsControl) || !(e.Data.GetDataPresent(typeof(LayerItem)) || e.Data.GetDataPresent(typeof(Layer))))
                return;

            DragScroll(itemsControl, e);

            LogManager.GetCurrentClassLogger().Trace($"Begin OnDragOver()");

            var sourceItemLayerItem = (LayerTreeViewItemBase)e.Data.GetData(typeof(LayerItem));
            var sourceItemLayer = (LayerTreeViewItemBase)e.Data.GetData(typeof(Layer));
            var sourceItem = EitherNotNull(sourceItemLayer, sourceItemLayerItem);
            var targetElement = HitTest<FrameworkElement>(itemsControl, e.GetPosition);

            var parentGrid = targetElement?.GetParent<Grid>();
            if (parentGrid == null || !(targetElement.DataContext is LayerTreeViewItemBase targetElementInfo) || targetElementInfo == sourceItem)
            {
                LogManager.GetCurrentClassLogger().Trace("OnDragOver() canceled.");
                return;
            }

            if (targetElementInfo.ContainsParent(sourceItem))
                return;

            e.Effects = DragDropEffects.Move;

            var targetParentLast = GetParentLastChild(targetElementInfo);

            const int boundary = 10;
            var pos = e.GetPosition(parentGrid);
            if (pos.Y > 0 && pos.Y < boundary)
            {
                _insertType = InsertType.Before;
                targetElementInfo.BeforeSeparatorVisibility.Value = Visibility.Visible;
            }
            else if (targetParentLast == targetElementInfo
                     && pos.Y < parentGrid.ActualHeight && pos.Y > parentGrid.ActualHeight - boundary)
            {
                _insertType = InsertType.After;
                targetElementInfo.AfterSeparatorVisibility.Value = Visibility.Visible;
            }
            //else if (targetElementInfo is LayerItem && sourceItem is LayerItem)
            //{
            //    targetElementInfo = targetElementInfo.Parent.Value;
            //    _insertType = InsertType.Children;
            //    targetElementInfo.Background.Value = Brushes.Gray;
            //}
            else if (targetElementInfo is Layer && sourceItem is LayerItem)
            {
                _insertType = InsertType.Children;
                targetElementInfo.Background.Value = Brushes.Gray;
            }

            if (!_changedBlocks.Contains(targetElementInfo))
                _changedBlocks.Add(targetElementInfo);

            isDragOver = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            ResetSeparator(_changedBlocks);

            if (!isDragOver)
                return;

            if (!(sender is ItemsControl itemsControl))
                return;

            var mainWindowViewModel = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            mainWindowViewModel.Recorder.BeginRecode();

            var sourceItemLayerItem = (LayerTreeViewItemBase)e.Data.GetData(typeof(LayerItem));
            var sourceItemLayer = (LayerTreeViewItemBase)e.Data.GetData(typeof(Layer));
            var sourceItem = EitherNotNull(sourceItemLayer, sourceItemLayerItem);
            var targetItem = HitTest<FrameworkElement>(itemsControl, e.GetPosition)?.DataContext as LayerTreeViewItemBase;

            if (targetItem == null || sourceItem == null || sourceItem == targetItem)
                return;

            if (targetItem.ContainsParent(sourceItem))
                return;

            var sourceItemParent = sourceItem.Parent.Value;
            var targetItemParent = targetItem.Parent.Value;
            var diagramVM = (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel;
            ObservableCollection<LayerTreeViewItemBase> children = sourceItemParent.Children.Value;
            if (sourceItemParent == diagramVM.RootLayer.Value)
            {
                children = diagramVM.Layers;
            }
            mainWindowViewModel.Recorder.Current.ExecuteRemove(children, sourceItem);
            switch (_insertType)
            {
                case InsertType.Before:
                    if (sourceItemParent != diagramVM.RootLayer.Value)
                    {
                        children = targetItemParent.Children.Value;
                    }
                    LayerTreeViewItemCollection.InsertBeforeChildren(mainWindowViewModel.Recorder, diagramVM.Layers, children, sourceItem, targetItem);
                    sourceItem.Parent.Value = targetItemParent;
                    sourceItem.IsSelected.Value = true;
                    break;
                case InsertType.After:
                    if (sourceItemParent != diagramVM.RootLayer.Value)
                    {
                        children = targetItemParent.Children.Value;
                    }
                    LayerTreeViewItemCollection.InsertAfterChildren(mainWindowViewModel.Recorder, diagramVM.Layers, children, sourceItem, targetItem);
                    sourceItem.Parent.Value = targetItemParent;
                    sourceItem.IsSelected.Value = true;
                    break;
                case InsertType.Children:
                    children = targetItem.Children.Value;
                    LayerTreeViewItemCollection.AddChildren(mainWindowViewModel.Recorder, diagramVM.Layers, children, sourceItem);
                    targetItem.IsExpanded.Value = true;
                    sourceItem.IsSelected.Value = true;
                    sourceItem.Parent.Value = targetItem;
                    break;
            }

            mainWindowViewModel.Recorder.EndRecode();

            DropCommand?.Execute(new DropArguments
            {
                Source = sourceItem,
                Target = targetItem,
                Type = _insertType
            });
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is TreeView treeView) || treeView.SelectedItem == null || _startPos == null)
                return;

            var cursorPoint = treeView.PointToScreen(e.GetPosition(treeView));
            var diff = cursorPoint - (Point)_startPos;
            if (!CanDrag(diff))
                return;

            DragDrop.DoDragDrop(treeView, treeView.SelectedItem, DragDropEffects.Move);
            LogManager.GetCurrentClassLogger().Trace($"Done DragDrop.DoDragDrop() in OnPreviewMouseMove()");

            _startPos = null;
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _startPos = null;
            isDragOver = false;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ItemsControl itemsControl))
            {
                LogManager.GetCurrentClassLogger().Trace("OnPreviewMouseLeftButtonDown() canceled.");
                return;
            }

            var pos = e.GetPosition(itemsControl);
            var hit = HitTest<FrameworkElement>(itemsControl, e.GetPosition);
            if (hit != null && hit.DataContext is LayerTreeViewItemBase)
                _startPos = itemsControl.PointToScreen(pos);
            else
                _startPos = null;
        }




        private static LayerTreeViewItemBase GetParentLastChild(LayerTreeViewItemBase infoBase)
        {
            var targetParent = infoBase.Parent.Value;
            var last = targetParent?.Children.Value.LastOrDefault();
            return last;
        }

        private static void ResetSeparator(ICollection<LayerTreeViewItemBase> collection)
        {
            var list = collection.ToList();
            foreach (var pair in list)
            {
                ResetSeparator(pair);
                collection.Remove(pair);
            }
        }

        private static void ResetSeparator(LayerTreeViewItemBase infoBase)
        {
            infoBase.Background.Value = Brushes.Transparent;
            infoBase.BeforeSeparatorVisibility.Value = Visibility.Hidden;
            infoBase.AfterSeparatorVisibility.Value = Visibility.Hidden;
        }

        private static void DragScroll(FrameworkElement itemsControl, DragEventArgs e)
        {
            var scrollViewer = itemsControl.Descendants<ScrollViewer>().FirstOrDefault();
            const double tolerance = 10d;
            const double offset = 3d;
            var verticalPos = e.GetPosition(itemsControl).Y;
            if (verticalPos < tolerance)
                scrollViewer?.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offset);
            else if (verticalPos > itemsControl.ActualHeight - tolerance)
                scrollViewer?.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offset);
        }

        private static T HitTest<T>(UIElement itemsControl, Func<IInputElement, Point> getPosition) where T : class
        {
            var pt = getPosition(itemsControl);
            var result = itemsControl.InputHitTest(pt);
            if (result is T ret)
                return ret;
            return null;
        }

        private static bool CanDrag(Vector delta)
        {
            return (SystemParameters.MinimumHorizontalDragDistance < Math.Abs(delta.X)) ||
                   (SystemParameters.MinimumVerticalDragDistance < Math.Abs(delta.Y));
        }
    }
}
