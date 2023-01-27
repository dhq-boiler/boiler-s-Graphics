using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using NLog;

namespace boilersGraphics.Helpers;

public static class Renderer
{
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    public static RenderTargetBitmap Render(Rect? sliceRect, DesignerCanvas designerCanvas,
        DiagramViewModel diagramViewModel, BackgroundViewModel backgroundItem, DesignerItemViewModelBase mosaic = null)
    {
        var size = GetRenderSize(sliceRect, diagramViewModel);

        s_logger.Debug($"SliceRect size:{size}");

        var width = (int)size.Width;
        var height = (int)size.Height;
        if (width <= 0) width = 1;
        if (height <= 0) height = 1;

        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

        var renderedCount = 0;
        Application.Current.Dispatcher.Invoke(() =>
        {
            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var allViews = designerCanvas.GetDescendantsViews<FrameworkElement>().ToList();
                //背景を描画
                if (RenderBackgroundViewModel(sliceRect, designerCanvas, context, backgroundItem, allViews))
                    renderedCount++;
                //前景を描画
                renderedCount += RenderForeground(sliceRect, diagramViewModel, designerCanvas, context, backgroundItem,
                    allViews, mosaic);
            }

            rtb.Render(visual);
            rtb.Freeze();
        });

        if (renderedCount == 0)
            s_logger.Warn("レンダリングが試みられましたが、レンダリングされませんでした。");
        else
            s_logger.Debug("レンダリングされました。");

        return rtb;
    }

    public static async Task<RenderTargetBitmap> RenderAsync(Rect? sliceRect, DesignerCanvas designerCanvas,
        DiagramViewModel diagramViewModel, BackgroundViewModel backgroundItem, DesignerItemViewModelBase mosaic = null)
    {
        var size = GetRenderSize(sliceRect, diagramViewModel);

        s_logger.Debug($"SliceRect size:{size}");

        var rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        var renderedCount = 0;
        await Application.Current.Dispatcher.Invoke(async () =>
        {
            using (var context = visual.RenderOpen())
            {
                //背景を描画
                if (await RenderBackgroundViewModelAsync(sliceRect, designerCanvas, context, backgroundItem))
                    renderedCount++;
                //前景を描画
                renderedCount += await RenderForegroundAsync(sliceRect, diagramViewModel, designerCanvas, context,
                    backgroundItem, mosaic);
            }

            rtb.Render(visual);
            rtb.Freeze();
        });

        if (renderedCount == 0)
            s_logger.Warn("レンダリングが試みられましたが、レンダリングされませんでした。");
        else
            s_logger.Debug("レンダリングされました。");

        return rtb;
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

    private static int RenderForeground(Rect? sliceRect, DiagramViewModel diagramViewModel,
        DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background,
        List<FrameworkElement> allViews, DesignerItemViewModelBase mosaic = null)
    {
        var renderedCount = 0;
        var except = new SelectableDesignerItemViewModelBase[] { background, mosaic }.Where(x => x is not null);
        foreach (var item in diagramViewModel.AllItems.Value.Except(except).Where(x => x.IsVisible.Value))
        {
            var view = default(FrameworkElement);
            view = allViews.FirstOrDefault(x => x.DataContext == item && x.GetType() == item.GetViewType());
            if (view is null)
                continue;
            view.SnapsToDevicePixels = true;
            var brush = new VisualBrush(view)
            {
                Stretch = Stretch.None,
                TileMode = TileMode.None
            };
            var rect = new Rect();
            if (item is DesignerItemViewModelBase designerItem)
            {
                if (item is PictureDesignerItemViewModel picture)
                {
                    var bounds = VisualTreeHelper.GetDescendantBounds(view);
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

                context.PushTransform(new RotateTransform(item.RotationAngle.Value,
                    (item as DesignerItemViewModelBase).CenterX.Value,
                    (item as DesignerItemViewModelBase).CenterY.Value));
                context.DrawRectangle(brush, null, rect);
                context.Pop();
                renderedCount++;
            }
            else if (item is ConnectorBaseViewModel connector)
            {
                var bounds = VisualTreeHelper.GetDescendantBounds(view);
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
            }

            //var size = GetRenderSize(diagramViewModel);
            //var rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            //DrawingVisual visual = new DrawingVisual();
            //using (DrawingContext context2 = visual.RenderOpen())
            //{
            //    context2.DrawRectangle(new SolidColorBrush(Colors.Green), null, new Rect(new Point(0, 0), size));
            //    context2.DrawRectangle(brush, null, rect);
            //}
            //rtb.Render(visual);
            //OpenCvSharpHelper.ImShow("Foreground", rtb);
        }

        return renderedCount;
    }

    private static async Task<int> RenderForegroundAsync(Rect? sliceRect, DiagramViewModel diagramViewModel,
        DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background,
        DesignerItemViewModelBase mosaic = null)
    {
        var renderedCount = 0;
        var except = new SelectableDesignerItemViewModelBase[] { background, mosaic }.Where(x => x is not null);
        foreach (var item in diagramViewModel.AllItems.Value.Except(except).Where(x => x.IsVisible.Value))
        {
            var view = default(FrameworkElement);
            var views = await designerCanvas.GetCorrespondingViewsAsync<FrameworkElement>(item).ToListAsync();
            view = views.FirstOrDefault(x => x.GetType() == item.GetViewType());
            if (view is null)
                continue;
            view.SnapsToDevicePixels = true;
            var brush = new VisualBrush(view);
            brush.Stretch = Stretch.None;
            brush.TileMode = TileMode.None;
            var rect = new Rect();
            if (item is DesignerItemViewModelBase designerItem)
            {
                if (item is PictureDesignerItemViewModel picture)
                {
                    var bounds = VisualTreeHelper.GetDescendantBounds(view);
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

                context.PushTransform(new RotateTransform(item.RotationAngle.Value,
                    (item as DesignerItemViewModelBase).CenterX.Value,
                    (item as DesignerItemViewModelBase).CenterY.Value));
                context.DrawRectangle(brush, null, rect);
                context.Pop();
                renderedCount++;
            }
            else if (item is ConnectorBaseViewModel connector)
            {
                var bounds = VisualTreeHelper.GetDescendantBounds(view);
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
            }

            //var size = GetRenderSize(diagramViewModel);
            //var rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);
            //DrawingVisual visual = new DrawingVisual();
            //using (DrawingContext context2 = visual.RenderOpen())
            //{
            //    context2.DrawRectangle(new SolidColorBrush(Colors.Green), null, new Rect(new Point(0, 0), size));
            //    context2.DrawRectangle(brush, null, rect);
            //}
            //rtb.Render(visual);
            //OpenCvSharpHelper.ImShow("Foreground", rtb);
        }

        return renderedCount;
    }

    private static bool RenderBackgroundViewModel(Rect? sliceRect, DesignerCanvas designerCanvas,
        DrawingContext context, BackgroundViewModel background, List<FrameworkElement> allViews)
    {
        var view = default(FrameworkElement);
        var result = Application.Current.Dispatcher.Invoke(() =>
        {
            view = allViews.FirstOrDefault(x => x.DataContext == background && x.GetType() == background.GetViewType());
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

        Rect rect;
        if (sliceRect.HasValue)
            rect = sliceRect.Value;
        else
            rect = bounds;

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

    private static async Task<bool> RenderBackgroundViewModelAsync(Rect? sliceRect, DesignerCanvas designerCanvas,
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

        Rect rect;
        if (sliceRect.HasValue)
            rect = sliceRect.Value;
        else
            rect = bounds;

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