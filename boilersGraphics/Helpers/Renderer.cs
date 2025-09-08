using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZLinq;

namespace boilersGraphics.Helpers;

public class Renderer
{
    public IVisualTreeHelper VisualTreeHelper { get; private set; }
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    public Renderer(IVisualTreeHelper visualTreeHelper)
    {
        VisualTreeHelper = visualTreeHelper;
    }

    public virtual RenderTargetBitmap Render(Rect? sliceRect, DesignerCanvas designerCanvas,
        DiagramViewModel diagramViewModel, BackgroundViewModel backgroundItem, SelectableDesignerItemViewModelBase caller, int minZIndex = 0, int maxZIndex = int.MaxValue)
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

    protected int RenderInternal(Rect? sliceRect, DesignerCanvas designerCanvas, DiagramViewModel diagramViewModel,
        BackgroundViewModel backgroundItem, int minZIndex, int maxZIndex, int renderedCount, RenderTargetBitmap rtb, SelectableDesignerItemViewModelBase caller)
    {
        var visual = new DrawingVisual();
        using (var context = visual.RenderOpen())
        {
            var allViews = designerCanvas.EnumVisualChildren<FrameworkElement>().AsValueEnumerable()
                .Where(x => x.DataContext is not null).ToList();
            //背景を描画
            if (RenderBackgroundViewModel(sliceRect, designerCanvas, context, backgroundItem, allViews, caller))
                renderedCount++;
            //前景を描画
            renderedCount += RenderForeground(sliceRect, diagramViewModel, designerCanvas, context,
                DiagramViewModel.Instance.BackgroundItem.Value,
                allViews, minZIndex, maxZIndex, caller);
        }

        rtb.Render(visual);
        rtb.Freeze();
        return renderedCount;
    }

    protected virtual Size GetRenderSize(Rect? sliceRect, DiagramViewModel diagramViewModel, int minZIndex, int maxZIndex)
    {
        Size size;
        if (sliceRect.HasValue)
            size = sliceRect.Value.Size;
        else
            size = new Size(diagramViewModel.BackgroundItem.Value.Width.Value,
                diagramViewModel.BackgroundItem.Value.Height.Value);
        return size;
    }

    public virtual int RenderForeground(Rect? sliceRect, DiagramViewModel diagramViewModel,
        DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background,
        List<FrameworkElement> allViews, int minZIndex, int maxZIndex, SelectableDesignerItemViewModelBase caller)
    {
        var renderedCount = 0;
        var except = new SelectableDesignerItemViewModelBase[] { background }.AsValueEnumerable().Where(x => x is not null);
        foreach (var item in diagramViewModel.AllItems.Value.AsValueEnumerable().Except(except).Where(x => x.IsVisible.Value && x.ZIndex.Value >= minZIndex && x.ZIndex.Value <= maxZIndex).OrderBy(x => x.ZIndex.Value))
        {
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

            if (item is DesignerItemViewModelBase des)
            {
                Canvas.SetLeft(view, des.Left.Value);
                Canvas.SetTop(view, des.Top.Value);
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
                view.Measure(new Size(size2.Width.CurrentValue, size2.Height.CurrentValue));
                view.Arrange(new Rect(new Point(), new Size(size2.Width.CurrentValue, size2.Height.CurrentValue)));
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
    
    public virtual bool RenderBackgroundViewModel(Rect? sliceRect, DesignerCanvas designerCanvas,
        DrawingContext context, BackgroundViewModel background, List<FrameworkElement> allViews, SelectableDesignerItemViewModelBase caller)
    {
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
        view.Arrange(background.Rect.CurrentValue);
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

    public static System.Drawing.Bitmap ConvertToBitmap(RenderTargetBitmap renderTargetBitmap)
    {
        var bitmapImage = new BitmapImage();

        // RenderTargetBitmapをBitmapImageに変換
        var bitmapEncoder = new BmpBitmapEncoder();
        bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
        using (var ms = new MemoryStream())
        {
            bitmapEncoder.Save(ms);
            ms.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
        }

        // BitmapImageをBitmapに変換
        var bitmap = new System.Drawing.Bitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
        bitmapImage.CopyPixels(Int32Rect.Empty, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride, bitmapData.Stride);
        bitmap.UnlockBits(bitmapData);
        return bitmap;
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