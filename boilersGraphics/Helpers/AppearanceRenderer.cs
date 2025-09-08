using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZLinq;

namespace boilersGraphics.Helpers
{
    public class AppearanceRenderer : Renderer
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        public AppearanceRenderer(IVisualTreeHelper visualTreeHelper) : base(visualTreeHelper)
        {
        }

        protected override Size GetRenderSize(Rect? sliceRect, DiagramViewModel diagramViewModel, int minZIndex, int maxZIndex)
        {
            var rect = base.GetRenderSize(sliceRect, diagramViewModel, minZIndex, maxZIndex);
            var except = new SelectableDesignerItemViewModelBase[] { diagramViewModel.BackgroundItem.Value }.AsValueEnumerable().Where(x => x is not null);
            var rects = diagramViewModel.AllItems.Value.AsValueEnumerable().Except(except).Where(x => x.IsVisible.Value && x.ZIndex.Value >= minZIndex && x.ZIndex.Value <= maxZIndex).OrderBy(x => x.ZIndex.Value).OfType<DesignerItemViewModelBase>().Select(x =>
            {
                var matrix = new Matrix();
                matrix.RotateAt(x.RotationAngle.Value, x.CenterX.Value, x.CenterY.Value);
                return Rect.Transform(x.Rect.Value, matrix);
            });
            var boundingRect = Rect.Empty;
            foreach (var _rect in rects)
            {
                boundingRect.Union(_rect.TopLeft);
                boundingRect.Union(_rect.TopRight);
                boundingRect.Union(_rect.BottomLeft);
                boundingRect.Union(_rect.BottomRight);
            }
            return boundingRect.Size;
        }

        public override RenderTargetBitmap Render(Rect? sliceRect, DesignerCanvas designerCanvas, DiagramViewModel diagramViewModel, BackgroundViewModel backgroundItem, SelectableDesignerItemViewModelBase caller, int minZIndex = 0, int maxZIndex = int.MaxValue)
        {
            var size = GetRenderSize(sliceRect, diagramViewModel, minZIndex, maxZIndex);

            s_logger.Debug($"SliceRect size:{size}");

            var width = (int)size.Width;
            var height = (int)size.Height;
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

            var renderedCount = 0;
            if (!App.IsTest)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    renderedCount = RenderInternal(sliceRect, designerCanvas, diagramViewModel, backgroundItem, minZIndex, maxZIndex, renderedCount, rtb, caller);
                });
            }
            else
            {
                renderedCount = RenderInternal(sliceRect, designerCanvas, diagramViewModel, backgroundItem, minZIndex, maxZIndex, renderedCount, rtb, caller);
            }

            if (renderedCount == 0)
                s_logger.Warn("レンダリングが試みられましたが、レンダリングされませんでした。");
            else
                s_logger.Debug("レンダリングされました。");

            return rtb;
        }

        public override int RenderForeground(Rect? sliceRect, DiagramViewModel diagramViewModel, DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background, List<FrameworkElement> allViews, int minZIndex, int maxZIndex, SelectableDesignerItemViewModelBase caller)
        {
            var renderedCount = 0;
            var except = new SelectableDesignerItemViewModelBase[] { background }.AsValueEnumerable().Where(x => x is not null);
            foreach (var item in diagramViewModel.AllItems.Value.AsValueEnumerable().Except(except).Where(x => x.IsVisible.Value && x.ZIndex.Value >= minZIndex && x.ZIndex.Value <= maxZIndex).OrderBy(x => x.ZIndex.Value))
            {
                if (item is null)
                {
                    continue;
                }

                var view = default(FrameworkElement);
                if (App.IsTest)
                {
                    view = allViews.AsValueEnumerable().FirstOrDefault(x => x.DataContext == item);
                }
                else
                {
                    view = allViews.AsValueEnumerable().FirstOrDefault(x => x.DataContext == item && x.FindName("PART_ContentPresenter") is not null);
                }

                if (view is null)
                    continue;

                var PART_ContentPresenter = view.FindName("PART_ContentPresenter") as ContentPresenter;
                if (PART_ContentPresenter is not null)
                {
                    view = PART_ContentPresenter;
                }

                Size renderSize;
                if (item is ISizeRps size1)
                {
                    view.Measure(new Size(size1.Width.Value, size1.Height.Value));
                    if (App.IsTest)
                    {
                        view.Arrange(new Rect(0, 0, size1.Width.Value, size1.Height.Value));
                    }
                    else
                    {
                        view.InvalidateArrange();
                    }
                }
                else if (item is ISizeReadOnlyRps size2)
                {
                    view.Measure(new Size(size2.Width.Value, size2.Height.Value));
                    view.Arrange(new Rect(new Point(), new Size(size2.Width.Value, size2.Height.Value)));
                }
                view.UpdateLayout();
                view.SnapsToDevicePixels = true;
                var brush = new VisualBrush(view)
                {
                    Stretch = Stretch.None,
                    TileMode = TileMode.None
                };
                var rect = new Rect();
                switch (item)
                {
                    case DesignerItemViewModelBase designerItem:
                        {
                            if (designerItem is PictureDesignerItemViewModel picture)
                            {
                                var bounds = VisualTreeHelper.GetDescendantBounds(view);
                                if (bounds.IsEmpty)
                                {
                                    continue;
                                }
                                if (sliceRect.HasValue)
                                {
                                    rect = sliceRect.Value;
                                    var intersectSrc = new Rect(designerItem.Left.Value, designerItem.Top.Value, bounds.Width,
                                        bounds.Height);
                                    rect = Rect.Union(rect, intersectSrc);
                                    rect = Rect.Intersect(rect, designerItem.Rect.Value);
                                    if (background is not null)
                                    {
                                        rect = Rect.Intersect(rect, background.Rect.Value);
                                    }

                                    if (rect != Rect.Empty)
                                    {
                                        rect.X -= sliceRect.Value.X;
                                        rect.Y -= sliceRect.Value.Y;
                                    }
                                }
                                else
                                {
                                    rect = new Rect(designerItem.Left.Value, designerItem.Top.Value, designerItem.Width.Value,
                                        designerItem.Height.Value);
                                }
                            }
                            else
                            {
                                var bounds = VisualTreeHelper.GetDescendantBounds(view);
                                if (bounds.IsEmpty)
                                {
                                    continue;
                                }
                                if (sliceRect.HasValue)
                                {
                                    rect = sliceRect.Value;
                                    var intersectSrc = new Rect(designerItem.Left.Value, designerItem.Top.Value, bounds.Width,
                                        bounds.Height);
                                    rect = Rect.Intersect(rect, intersectSrc);
                                    if (rect != Rect.Empty)
                                    {
                                        rect.X -= sliceRect.Value.X;
                                        rect.Y -= sliceRect.Value.Y;
                                    }
                                }
                                else
                                {
                                    rect = new Rect(designerItem.Left.Value, designerItem.Top.Value, designerItem.Width.Value,
                                        designerItem.Height.Value);
                                }
                            }

                            if (background is not null && rect != Rect.Empty)
                            {
                                rect.X -= background.Left.Value;
                                rect.Y -= background.Top.Value;
                            }

                            if (item is DesignerItemViewModelBase id)
                            {
                                var bounds = VisualTreeHelper.GetDescendantBounds(view);
                                var baseMatrix = new Matrix();

                                //rotateBounds計算
                                baseMatrix.RotateAt(id.RotationAngle.Value, id.CenterPoint.Value.X, id.CenterPoint.Value.Y);
                                var leftTop = new Vector(0, 0);
                                var rotatedLeftTop = baseMatrix.Transform(new Point(id.Left.Value, id.Top.Value)) + leftTop;
                                var rotatedLeftBottom = baseMatrix.Transform(new Point(id.Left.Value, id.Bottom.Value)) + leftTop;
                                var rotatedRightTop = baseMatrix.Transform(new Point(id.Right.Value, id.Top.Value)) + leftTop;
                                var rotatedRightBottom = baseMatrix.Transform(new Point(id.Right.Value, id.Bottom.Value)) + leftTop;
                                var rotatedBounds = new Rect(Math.Min(Math.Min(rotatedLeftTop.X, rotatedLeftBottom.X), Math.Min(rotatedRightTop.X, rotatedRightBottom.X)),
                                                             Math.Min(Math.Min(rotatedLeftTop.Y, rotatedLeftBottom.Y), Math.Min(rotatedRightTop.Y, rotatedRightBottom.Y)),
                                                             Math.Max(Math.Max(rotatedLeftTop.X, rotatedLeftBottom.X), Math.Max(rotatedRightTop.X, rotatedRightBottom.X)) - Math.Min(Math.Min(rotatedLeftTop.X, rotatedLeftBottom.X), Math.Min(rotatedRightTop.X, rotatedRightBottom.X)),
                                                             Math.Max(Math.Max(rotatedLeftTop.Y, rotatedLeftBottom.Y), Math.Max(rotatedRightTop.Y, rotatedRightBottom.Y)) - Math.Min(Math.Min(rotatedLeftTop.Y, rotatedLeftBottom.Y), Math.Min(rotatedRightTop.Y, rotatedRightBottom.Y)));
                                baseMatrix.Translate(-rotatedBounds.X + id.Left.Value, -rotatedBounds.Y + id.Top.Value);

                                //PushTransform用にMatrixを初期化
                                baseMatrix = new Matrix();
                                baseMatrix.RotateAt(id.RotationAngle.Value, id.CenterPoint.Value.X - id.Left.Value, id.CenterPoint.Value.Y - id.Top.Value);
                                baseMatrix.Translate(-rotatedBounds.X + id.Left.Value, -rotatedBounds.Y + id.Top.Value);
                                context.PushTransform(new MatrixTransform(baseMatrix));
                            }

                            context.DrawRectangle(brush, null, rect);

                            if (item is DesignerItemViewModelBase)
                            {
                                context.Pop();
                            }

                            renderedCount++;
                            break;
                        }
                    case ConnectorBaseViewModel connector:
                        {
                            var bounds = VisualTreeHelper.GetDescendantBounds(view);
                            if (bounds.IsEmpty)
                            {
                                continue;
                            }
                            if (sliceRect.HasValue)
                            {
                                rect = sliceRect.Value;
                                var intersectSrc = new Rect(connector.LeftTop.Value, bounds.Size);
                                rect = Rect.Intersect(rect, intersectSrc);
                                if (rect != Rect.Empty)
                                {
                                    rect.X -= sliceRect.Value.X;
                                    rect.Y -= sliceRect.Value.Y;
                                }
                            }
                            else
                            {
                                rect = new Rect(connector.LeftTop.Value, bounds.Size);
                            }

                            rect.X -= background.Left.Value;
                            rect.Y -= background.Top.Value;
                            context.DrawRectangle(brush, null, rect);
                            renderedCount++;
                            break;
                        }
                }
            }

            return renderedCount;
        }

        public override bool RenderBackgroundViewModel(Rect? sliceRect, DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background, List<FrameworkElement> allViews, SelectableDesignerItemViewModelBase caller)
        {
            var grid = new Grid();
            grid.Background = Brushes.Transparent;

            var _brush = new VisualBrush(grid)
            {
                Stretch = Stretch.None,
            };
            context.DrawRectangle(_brush, null, sliceRect.Value);

            if (background is null)
            {
                return false;
            }

            var view = default(FrameworkElement);
            if (!boilersGraphics.App.IsTest)
            {
                var result = Application.Current.Dispatcher.Invoke(() =>
                {
                    view = allViews.AsValueEnumerable().FirstOrDefault(x =>
                        x.DataContext == background);
                    if (view is null)
                    {
                        s_logger.Warn($"Not Found: view of {background}");
                        return false;
                    }

                    return true;
                });
                if (!result)
                    return false;
            }
            else
            {
                view = allViews.AsValueEnumerable().FirstOrDefault(x =>
                    x.DataContext == background);
                if (view is null)
                {
                    s_logger.Warn($"Not Found: view of {background}");
                    return false;
                }
            }

            view.Measure(new Size(background.Width.Value, background.Height.Value));
            view.Arrange(background.Rect.Value);
            view.UpdateLayout();

            var bounds = VisualTreeHelper.GetDescendantBounds(view);

            var rect = sliceRect ?? bounds;

            var brush = new VisualBrush(view)
            {
                Stretch = Stretch.None
            };
            if (sliceRect.HasValue)
            {
                rect.X = 0;
                rect.Y = 0;
            }

            if (caller is DesignerItemViewModelBase designer)
            {
                var baseMatrix = new Matrix();
                baseMatrix.Translate(-designer.Left.Value, -designer.Top.Value);
                baseMatrix.RotateAt(-designer.RotationAngle.Value, designer.CenterPoint.Value.X - designer.Left.Value, designer.CenterPoint.Value.Y - designer.Top.Value);
                context.PushTransform(new MatrixTransform(baseMatrix));
            }
            context.DrawRectangle(brush, null, background.Rect.Value);
            if (caller is DesignerItemViewModelBase)
            {
                context.Pop();
            }

            return true;
        }
    }
}
