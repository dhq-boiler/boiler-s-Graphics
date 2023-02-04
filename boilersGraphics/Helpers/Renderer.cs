using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Helpers;

public class Renderer
{
    public IVisualTreeHelper VisualTreeHelper { get; private set; }
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    public Renderer(IVisualTreeHelper visualTreeHelper)
    {
        VisualTreeHelper = visualTreeHelper;
    }

    public RenderTargetBitmap Render(Rect? sliceRect, DesignerCanvas designerCanvas,
        DiagramViewModel diagramViewModel, BackgroundViewModel backgroundItem, int maxZIndex = int.MaxValue)
    {
        var size = GetRenderSize(sliceRect, diagramViewModel);

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
                renderedCount = RenderInternal(sliceRect, designerCanvas, diagramViewModel, backgroundItem, maxZIndex, renderedCount, rtb);
            });
        }
        else
        {
            renderedCount = RenderInternal(sliceRect, designerCanvas, diagramViewModel, backgroundItem, maxZIndex, renderedCount, rtb);
        }

        if (renderedCount == 0)
            s_logger.Warn("レンダリングが試みられましたが、レンダリングされませんでした。");
        else
            s_logger.Debug("レンダリングされました。");

        return rtb;
    }

    private int RenderInternal(Rect? sliceRect, DesignerCanvas designerCanvas, DiagramViewModel diagramViewModel,
        BackgroundViewModel backgroundItem, int maxZIndex, int renderedCount, RenderTargetBitmap rtb)
    {
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            var allViews = designerCanvas.EnumVisualChildren<FrameworkElement>()
                .Where(x => x.DataContext is not null).ToList();
            //背景を描画
            if (RenderBackgroundViewModel(sliceRect, designerCanvas, context, backgroundItem, allViews))
                renderedCount++;
            //前景を描画
            renderedCount += RenderForeground(sliceRect, diagramViewModel, designerCanvas, context,
                backgroundItem,
                allViews, maxZIndex);
        }

        rtb.Render(visual);
        rtb.Freeze();
        return renderedCount;
    }

    private static Size GetRenderSize(Rect? sliceRect, DiagramViewModel diagramViewModel)
    {
        Size size;
        if (sliceRect.HasValue)
            size = sliceRect.Value.Size;
        else
            size = new Size(diagramViewModel.BackgroundItem.Value.Width.Value,
                diagramViewModel.BackgroundItem.Value.Height.Value);
        return size;
    }

    private int RenderForeground(Rect? sliceRect, DiagramViewModel diagramViewModel,
        DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background,
        List<FrameworkElement> allViews, int maxZIndex)
    {
        var renderedCount = 0;
        var except = new SelectableDesignerItemViewModelBase[] { background }.Where(x => x is not null);
        foreach (var item in diagramViewModel.AllItems.Value.Except(except).Where(x => x.IsVisible.Value && x.ZIndex.Value <= maxZIndex))
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

                    context.PushTransform(new RotateTransform(designerItem.RotationAngle.Value,
                        designerItem.CenterX.Value,
                        designerItem.CenterY.Value));
                    context.DrawRectangle(brush, null, rect);
                    context.Pop();
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
    
    private bool RenderBackgroundViewModel(Rect? sliceRect, DesignerCanvas designerCanvas,
        DrawingContext context, BackgroundViewModel background, List<FrameworkElement> allViews)
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

        context.DrawRectangle(brush, null, rect);

        return true;
    }

    private async Task<bool> RenderBackgroundViewModelAsync(Rect? sliceRect, DesignerCanvas designerCanvas,
        DrawingContext context, BackgroundViewModel background)
    {
        var view = default(FrameworkElement);
        var result = await Application.Current.Dispatcher.Invoke(async () =>
        {
            var views = await designerCanvas.GetCorrespondingViewsAsync<FrameworkElement>(background).ToListAsync();
            view = views.FirstOrDefault(x => x.GetType() == background.GetViewType());
            if (view is null)
            {
                s_logger.Warn($"Not Found: view of {background}");
                return false;
            }

            return true;
        });
        if (!result)
            return false;
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
            view.UpdateLayout();
            context.DrawRectangle(brush, null, rect);
        }
        else
        {
            view.UpdateLayout();
            context.DrawRectangle(brush, null, rect);
        }

        return true;
    }
}