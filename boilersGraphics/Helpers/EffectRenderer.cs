using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace boilersGraphics.Helpers
{
    public class EffectRenderer : Renderer
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        public EffectRenderer(IVisualTreeHelper visualTreeHelper) : base(visualTreeHelper)
        {
        }

        public override int RenderForeground(Rect? sliceRect, DiagramViewModel diagramViewModel, DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background, List<FrameworkElement> allViews, int maxZIndex, SelectableDesignerItemViewModelBase caller)
        {
            var renderedCount = 0;
            var except = new SelectableDesignerItemViewModelBase[] { background }.Where(x => x is not null);
            foreach (var item in diagramViewModel.AllItems.Value.Except(except).Where(x => x.IsVisible.Value && x.ZIndex.Value <= maxZIndex).OrderBy(x => x.ZIndex.Value))
            {
                var view = default(FrameworkElement);
                if (App.IsTest)
                {
                    view = allViews.FirstOrDefault(x => x.DataContext == item);
                }
                else
                {
                    view = allViews.FirstOrDefault(x => x.DataContext == item && x.FindName("PART_ContentPresenter") is not null);
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
                                    rect = Rect.Intersect(rect, background.Rect.Value);

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

                            if (rect != Rect.Empty)
                            {
                                rect.X -= background.Left.Value;
                                rect.Y -= background.Top.Value;
                            }

                            if (item is DesignerItemViewModelBase id && caller is DesignerItemViewModelBase des)
                            {
                                var baseMatrix = new Matrix();
                                baseMatrix.Translate((des.CenterPoint.Value.X - rect.Width / 2) * 1, (des.CenterPoint.Value.Y - rect.Height / 2) * 1);
                                baseMatrix.Translate(-des.Left.Value, -des.Top.Value);
                                baseMatrix.RotateAt(item.RotationAngle.Value, id.CenterPoint.Value.X - des.Left.Value, id.CenterPoint.Value.Y - des.Top.Value);
                                baseMatrix.RotateAt(-des.RotationAngle.Value, des.CenterPoint.Value.X - des.Left.Value, des.CenterPoint.Value.Y - des.Top.Value);
                                context.PushTransform(new MatrixTransform(baseMatrix));
                            }

                            context.DrawRectangle(brush, null, rect);
                            if (caller is DesignerItemViewModelBase)
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
            var view = default(FrameworkElement);
            if (!boilersGraphics.App.IsTest)
            {
                var result = Application.Current.Dispatcher.Invoke(() =>
                {
                    view = allViews.FirstOrDefault(x =>
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
                view = allViews.FirstOrDefault(x =>
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
