using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using NLog;
using System;
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

public class Renderer : IDisposable
{
    public IVisualTreeHelper VisualTreeHelper { get; private set; }
    private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

    // キャッシュ用フィールド
    private List<FrameworkElement> _cachedAllViews;
    private Dictionary<object, FrameworkElement> _dataContextToViewCache = new();
    private bool _viewCacheInvalidated = true;
    private DesignerCanvas _lastDesignerCanvas;
    private bool _disposed = false;

    public Renderer(IVisualTreeHelper visualTreeHelper)
    {
        VisualTreeHelper = visualTreeHelper;
    }

    // 公開キャッシュ無効化メソッド
    public void InvalidateViewCache()
    {
        _viewCacheInvalidated = true;
        _cachedAllViews = null;
        _dataContextToViewCache.Clear();
    }

    // リソースの解放
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _cachedAllViews?.Clear();
                _dataContextToViewCache?.Clear();
                _cachedAllViews = null;
                _dataContextToViewCache = null;
            }
            _disposed = true;
        }
    }

    // キャッシュされたビューを取得
    private List<FrameworkElement> GetCachedAllViews(DesignerCanvas designerCanvas)
    {
        if (_viewCacheInvalidated || _cachedAllViews == null || _lastDesignerCanvas != designerCanvas)
        {
            _cachedAllViews = designerCanvas.EnumerateChildOfType<FrameworkElement>()
                .AsValueEnumerable()
                .Where(x => x.DataContext is not null)
                .ToList();
            
            // DataContextからViewへのマッピングキャッシュを構築
            _dataContextToViewCache.Clear();
            foreach (var view in _cachedAllViews)
            {
                if (view.DataContext != null)
                {
                    _dataContextToViewCache[view.DataContext] = view;
                }
            }
            
            _lastDesignerCanvas = designerCanvas;
            _viewCacheInvalidated = false;
        }
        return _cachedAllViews;
    }

    // 効率的なビュー検索
    protected FrameworkElement FindViewForDataContext(object dataContext, List<FrameworkElement> allViews)
    {
        // キャッシュから高速検索
        if (_dataContextToViewCache.TryGetValue(dataContext, out var cachedView))
        {
            return cachedView;
        }

        // キャッシュにない場合はフォールバック検索
        FrameworkElement view = null;
        if (App.IsTest)
        {
            view = allViews.AsValueEnumerable().FirstOrDefault(x => x.DataContext == dataContext);
        }
        else
        {
            view = allViews.AsValueEnumerable().FirstOrDefault(x => 
                x.DataContext == dataContext && x.FindName("PART_ContentPresenter") is not null);
        }

        // 見つかった場合はキャッシュに追加
        if (view != null)
        {
            _dataContextToViewCache[dataContext] = view;
        }

        return view;
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
            // キャッシュされたビューを取得（パフォーマンス改善）
            var allViews = GetCachedAllViews(designerCanvas);
            
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
        
        // 描画対象アイテムを事前にフィルタリングして並べ替え（パフォーマンス改善）
        var itemsToRender = diagramViewModel.AllItems.Value.AsValueEnumerable()
            .Except(except)
            .Where(x => x.IsVisible.Value && x.ZIndex.Value >= minZIndex && x.ZIndex.Value <= maxZIndex)
            .OrderBy(x => x.ZIndex.Value)
            .ToList(); // ToList()でキャッシュ化

        // バッチでレイアウト更新を処理するためのリスト
        var viewsNeedingLayout = new List<(FrameworkElement view, SelectableDesignerItemViewModelBase item)>();

        foreach (var item in itemsToRender)
        {
            // 効率的なビュー検索を使用
            var view = FindViewForDataContext(item, allViews);
            if (view is null)
                continue;

            var PART_ContentPresenter = view.FindName("PART_ContentPresenter") as ContentPresenter;
            if (PART_ContentPresenter is not null)
            {
                view = PART_ContentPresenter;
            }

            // レイアウト更新が必要なビューをリストに追加
            viewsNeedingLayout.Add((view, item));
        }

        // バッチでレイアウト更新を実行（パフォーマンス改善）
        foreach (var (view, item) in viewsNeedingLayout)
        {
            PrepareViewForRendering(view, item);
        }

        // 実際の描画処理
        foreach (var (view, item) in viewsNeedingLayout)
        {
            if (RenderSingleItem(sliceRect, context, background, view, item))
            {
                renderedCount++;
            }
        }

        return renderedCount;
    }

    // ビューのレンダリング準備を行う（分離してパフォーマンス改善）
    protected void PrepareViewForRendering(FrameworkElement view, SelectableDesignerItemViewModelBase item)
    {
        if (item is DesignerItemViewModelBase des)
        {
            Canvas.SetLeft(view, des.Left.Value);
            Canvas.SetTop(view, des.Top.Value);
        }

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
    }

    // 単一アイテムの描画処理（分離してパフォーマンス改善）
    private bool RenderSingleItem(Rect? sliceRect, DrawingContext context, BackgroundViewModel background, 
        FrameworkElement view, SelectableDesignerItemViewModelBase item)
    {
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
                    rect = CalculateDesignerItemRect(sliceRect, view, designerItem, background);
                    if (rect == Rect.Empty) return false;

                    context.PushTransform(new RotateTransform(designerItem.RotationAngle.Value,
                        designerItem.CenterX.Value, designerItem.CenterY.Value));
                    context.DrawRectangle(brush, null, rect);
                    context.Pop();
                    return true;
                }
            case ConnectorBaseViewModel connector:
                {
                    rect = CalculateConnectorRect(sliceRect, view, connector, background);
                    if (rect == Rect.Empty) return false;

                    context.DrawRectangle(brush, null, rect);
                    return true;
                }
        }
        
        return false;
    }

    // DesignerItemの矩形計算（分離してコードの可読性向上）
    private Rect CalculateDesignerItemRect(Rect? sliceRect, FrameworkElement view, 
        DesignerItemViewModelBase designerItem, BackgroundViewModel background)
    {
        var bounds = VisualTreeHelper.GetDescendantBounds(view);
        if (bounds.IsEmpty) return Rect.Empty;

        Rect rect;
        if (designerItem is PictureDesignerItemViewModel)
        {
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
        else
        {
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

        return rect;
    }

    // Connectorの矩形計算（分離してコードの可読性向上）
    private Rect CalculateConnectorRect(Rect? sliceRect, FrameworkElement view, 
        ConnectorBaseViewModel connector, BackgroundViewModel background)
    {
        var bounds = VisualTreeHelper.GetDescendantBounds(view);
        if (bounds.IsEmpty) return Rect.Empty;

        Rect rect;
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
        return rect;
    }
    
    public virtual bool RenderBackgroundViewModel(Rect? sliceRect, DesignerCanvas designerCanvas,
        DrawingContext context, BackgroundViewModel background, List<FrameworkElement> allViews, SelectableDesignerItemViewModelBase caller)
    {
        FrameworkElement view = null;
        
        if (!boilersGraphics.App.IsTest)
        {
            var result = Application.Current.Dispatcher.Invoke(() =>
            {
                // 効率的なビュー検索を使用
                view = FindViewForDataContext(background, allViews);
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
            // 効率的なビュー検索を使用
            view = FindViewForDataContext(background, allViews);
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