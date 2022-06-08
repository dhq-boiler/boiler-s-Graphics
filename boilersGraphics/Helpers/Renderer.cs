using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Helpers
{
    public static class Renderer
    {
        public static RenderTargetBitmap Render(Rect? sliceRect, DesignerCanvas designerCanvas, DiagramViewModel diagramViewModel, BackgroundViewModel backgroundItem)
        {
            Size size = GetRenderSize(sliceRect, diagramViewModel);

            LogManager.GetCurrentClassLogger().Info($"SliceRect size:{size}");

            var rtb = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                //背景を描画
                RenderBackgroundViewModel(sliceRect, designerCanvas, context, backgroundItem);
            }
            rtb.Render(visual);

            //OpenCvSharpHelper.ImShow("step1. render background", rtb);

            using (DrawingContext context = visual.RenderOpen())
            {
                RenderForeground(sliceRect, diagramViewModel, designerCanvas, context, backgroundItem);
            }
            rtb.Render(visual);

            //OpenCvSharpHelper.ImShow("step2. render foreground", rtb);

            rtb.Freeze();

            return rtb;
        }

        private static Size GetRenderSize(Rect? sliceRect, DiagramViewModel diagramViewModel)
        {
            Size size;
            if (sliceRect.HasValue)
            {
                size = sliceRect.Value.Size;
            }
            else
            {
                size = new Size(diagramViewModel.BackgroundItem.Value.Width.Value, diagramViewModel.BackgroundItem.Value.Height.Value);
            }
            return size;
        }

        private static void RenderForeground(Rect? sliceRect, DiagramViewModel diagramViewModel, DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background)
        {
            foreach (var item in diagramViewModel.AllItems.Value.Except(new SelectableDesignerItemViewModelBase[] { background }))
            {
                var views = designerCanvas.GetCorrespondingViews<FrameworkElement>(item);
                var view = views.First(x => x.GetType() == item.GetViewType());
                view.SnapsToDevicePixels = true;
                VisualBrush brush = new VisualBrush(view);
                brush.Stretch = Stretch.None;
                brush.TileMode = TileMode.None;
                Rect rect = new Rect();
                if (item is DesignerItemViewModelBase designerItem)
                {
                    if (item is PictureDesignerItemViewModel picture)
                    {
                        var bounds = VisualTreeHelper.GetDescendantBounds(view);
                        if (sliceRect.HasValue)
                        {
                            rect = sliceRect.Value;
                            var intersectSrc = new Rect(designerItem.Left.Value, designerItem.Top.Value, bounds.Width, bounds.Height);
                            rect = Rect.Union(rect, intersectSrc);
                            if (rect != Rect.Empty)
                            {
                                rect.X -= sliceRect.Value.X;
                                rect.Y -= sliceRect.Value.Y;
                            }
                        }
                        else
                        {
                            rect = new Rect(designerItem.Left.Value, designerItem.Top.Value, designerItem.Width.Value, designerItem.Height.Value);
                        }
                    }
                    else
                    {
                        var bounds = VisualTreeHelper.GetDescendantBounds(view);
                        if (sliceRect.HasValue)
                        {
                            rect = sliceRect.Value;
                            var intersectSrc = new Rect(designerItem.Left.Value, designerItem.Top.Value, bounds.Width, bounds.Height);
                            rect = Rect.Intersect(rect, intersectSrc);
                            if (rect != Rect.Empty)
                            {
                                rect.X -= sliceRect.Value.X;
                                rect.Y -= sliceRect.Value.Y;
                            }
                        }
                        else
                        {
                            rect = new Rect(designerItem.Left.Value, designerItem.Top.Value, designerItem.Width.Value, designerItem.Height.Value);
                        }
                    }
                    if (rect != Rect.Empty)
                    {
                        rect.X -= background.Left.Value;
                        rect.Y -= background.Top.Value;
                    }
                    context.PushTransform(new RotateTransform(item.RotationAngle.Value, (item as DesignerItemViewModelBase).CenterX.Value, (item as DesignerItemViewModelBase).CenterY.Value));
                    context.DrawRectangle(brush, null, rect);
                    context.Pop();
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
        }

        private static void RenderBackgroundViewModel(Rect? sliceRect, DesignerCanvas designerCanvas, DrawingContext context, BackgroundViewModel background)
        {
            var views = designerCanvas.GetCorrespondingViews<FrameworkElement>(background);
            var view = views.First(x => x.GetType() == background.GetViewType());
            var bounds = VisualTreeHelper.GetDescendantBounds(view);

            Rect rect;
            if (sliceRect.HasValue)
            {
                rect = sliceRect.Value;
            }
            else
            {
                rect = bounds;
            }

            VisualBrush brush = new VisualBrush(view);
            brush.Stretch = Stretch.None;
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
        }
    }
}
