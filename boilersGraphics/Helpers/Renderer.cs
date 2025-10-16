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
    // DrawingVisualキャッシュ用フィールド
    private readonly Dictionary<object, CachedDrawingVisual> _drawingVisualCache = new();
    private readonly HashSet<object> _dirtyItems = new();
    private object _lastModifiedItem;

    // キャッシュ用フィールド
    private List<FrameworkElement> _cachedAllViews;
    private Dictionary<object, FrameworkElement> _dataContextToViewCache = new();
    private bool _viewCacheInvalidated = true;
    private DesignerCanvas _lastDesignerCanvas;
    private bool _disposed = false;

    protected readonly RendererCache _cache;

    protected Renderer(IVisualTreeHelper visualTreeHelper, RendererCache cache)
    {
        VisualTreeHelper = visualTreeHelper;
        _cache = cache;
    }

    public Renderer(IVisualTreeHelper visualTreeHelper) : this(visualTreeHelper, new RendererCache())
    {
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
            //view = allViews.AsValueEnumerable().FirstOrDefault(x => 
            //    x.DataContext == dataContext && x.FindName("PART_ContentPresenter") is not null);

            view = allViews.AsValueEnumerable().FirstOrDefault(x => x.DataContext == dataContext);
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

        var width = (int)size.Width;
        var height = (int)size.Height;
        if (width <= 0) width = 1;
        if (height <= 0) height = 1;

        var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);

        var renderedCount = 0;
        if (!App.IsTest)
        {
            // UI要素のアクセスのみをDispatcher内で実行
            var renderData = Application.Current.Dispatcher.Invoke(() =>
                PrepareRenderData(sliceRect, designerCanvas, diagramViewModel, backgroundItem, minZIndex, maxZIndex, caller));

            // 実際の描画処理はDispatcher外で実行
            renderedCount = RenderWithPreparedData(renderData, rtb);
        }
        else
        {
            renderedCount = RenderInternal(sliceRect, designerCanvas, diagramViewModel, backgroundItem, minZIndex, maxZIndex, renderedCount, rtb, caller);
        }

        return rtb;
    }

    private RenderData PrepareRenderData(Rect? sliceRect, DesignerCanvas designerCanvas,
        DiagramViewModel diagramViewModel, BackgroundViewModel backgroundItem,
        int minZIndex, int maxZIndex, SelectableDesignerItemViewModelBase caller)
    {
        var allViews = GetCachedAllViews(designerCanvas);

        var renderItems = new List<RenderItemData>();
        var except = new SelectableDesignerItemViewModelBase[] { backgroundItem }.AsValueEnumerable().Where(x => x is not null);

        var itemsToRender = diagramViewModel.AllItems.Value.AsValueEnumerable()
            .Except(except)
            .Where(x => x.IsVisible.Value && x.ZIndex.Value >= minZIndex && x.ZIndex.Value <= maxZIndex)
            .OrderBy(x => x.ZIndex.Value)
            .OfType<DesignerItemViewModelBase>();

        foreach (var item in itemsToRender)
        {
            var view = FindViewForDataContext(item, allViews);
            if (view != null)
            {
                // UI要素の情報を取得してキャッシュ
                var itemData = CreateRenderItemData(view, item);
                renderItems.Add(itemData);
            }
        }

        var backgroundData = CreateBackgroundRenderData(backgroundItem, allViews);

        return new RenderData
        {
            SliceRect = sliceRect,
            RenderItems = renderItems,
            BackgroundData = backgroundData,
            BackgroundItem = backgroundItem
        };
    }

    private int RenderWithPreparedData(RenderData renderData, RenderTargetBitmap rtb)
    {
        var visual = new DrawingVisual();
        var renderedCount = 0;

        using (var context = visual.RenderOpen())
        {
            // 描画領域外の要素を事前に除外
            var visibleItems = FilterVisibleItems(renderData.RenderItems, renderData.SliceRect);

            // 背景を描画
            if (RenderBackgroundWithData(renderData.SliceRect, context, renderData.BackgroundData, renderData.BackgroundItem))
            {
                renderedCount++;
            }

            // 可視要素のみを描画
            foreach (var itemData in visibleItems)
            {
                if (RenderItemWithDataCached(renderData.SliceRect, context, itemData, renderData.BackgroundItem))
                {
                    renderedCount++;
                }
            }
        }

        rtb.Render(visual);
        rtb.Freeze();

        return renderedCount;
    }

    private List<RenderItemData> FilterVisibleItems(List<RenderItemData> items, Rect? sliceRect)
    {
        if (!sliceRect.HasValue)
            return items;

        var visibleRect = sliceRect.Value;
        return items.Where(item =>
        {
            var itemRect = GetItemBounds(item);
            return visibleRect.IntersectsWith(itemRect);
        }).ToList();
    }

    private Rect GetItemBounds(RenderItemData item)
    {
        switch (item.Item)
        {
            case DesignerItemViewModelBase designerItem:
                return new Rect(item.Left, item.Top, item.Width, item.Height);
            case ConnectorBaseViewModel connector:
                return new Rect(item.LeftTop, item.Bounds.Size);
            default:
                return item.Bounds;
        }
    }

    private bool RenderItemWithData(Rect? sliceRect, DrawingContext context, RenderItemData itemData, BackgroundViewModel background)
    {
        // VisualBrushのキャッシュ
        var brush = GetOrCreateVisualBrush(itemData.View);

        var rect = CalculateRenderRect(sliceRect, itemData, background);
        if (rect == Rect.Empty) return false;

        switch (itemData.Item)
        {
            case DesignerItemViewModelBase designerItem:
                if (Math.Abs(designerItem.RotationAngle.Value) > 0.01) // 回転がある場合のみ変換を適用
                {
                    context.PushTransform(new RotateTransform(designerItem.RotationAngle.Value,
                        designerItem.CenterX.Value, designerItem.CenterY.Value));
                    context.DrawRectangle(brush, null, rect);
                    context.Pop();
                }
                else
                {
                    context.DrawRectangle(brush, null, rect);
                }
                return true;

            case ConnectorBaseViewModel:
                context.DrawRectangle(brush, null, rect);
                return true;
        }

        return false;
    }

    private Rect CalculateRenderRect(Rect? sliceRect, RenderItemData itemData, BackgroundViewModel background)
    {
        switch (itemData.Item)
        {
            case DesignerItemViewModelBase designerItem:
                return CalculateDesignerItemRectWithData(sliceRect, itemData, designerItem, background);

            case ConnectorBaseViewModel connector:
                return CalculateConnectorRectWithData(sliceRect, itemData, connector, background);

            default:
                // フォールバック処理
                return CalculateDefaultRectWithData(sliceRect, itemData, background);
        }
    }

    private Rect CalculateDefaultRectWithData(Rect? sliceRect, RenderItemData itemData, BackgroundViewModel background)
    {
        var bounds = itemData.Bounds;
        if (bounds.IsEmpty) return Rect.Empty;

        Rect rect;
        if (sliceRect.HasValue)
        {
            rect = sliceRect.Value;
            var intersectSrc = new Rect(itemData.Left, itemData.Top, bounds.Width, bounds.Height);
            rect = Rect.Intersect(rect, intersectSrc);
            if (rect != Rect.Empty)
            {
                rect.X -= sliceRect.Value.X;
                rect.Y -= sliceRect.Value.Y;
            }
        }
        else
        {
            rect = new Rect(itemData.Left, itemData.Top, bounds.Width, bounds.Height);
        }

        if (rect != Rect.Empty)
        {
            rect.X -= background.Left.Value;
            rect.Y -= background.Top.Value;
        }

        return rect;
    }

    private Rect CalculateDesignerItemRectWithData(Rect? sliceRect, RenderItemData itemData,
    DesignerItemViewModelBase designerItem, BackgroundViewModel background)
    {
        var bounds = itemData.Bounds;
        if (bounds.IsEmpty) return Rect.Empty;

        Rect rect;
        if (designerItem is PictureDesignerItemViewModel)
        {
            if (sliceRect.HasValue)
            {
                rect = sliceRect.Value;
                var intersectSrc = new Rect(itemData.Left, itemData.Top, bounds.Width, bounds.Height);
                rect = Rect.Intersect(rect, intersectSrc);
                if (rect != Rect.Empty)
                {
                    rect.X -= sliceRect.Value.X;
                    rect.Y -= sliceRect.Value.Y;
                }
            }
            else
            {
                rect = new Rect(itemData.Left, itemData.Top, itemData.Width, itemData.Height);
            }
        }
        else
        {
            if (sliceRect.HasValue)
            {
                rect = sliceRect.Value;
                var intersectSrc = new Rect(itemData.Left, itemData.Top, bounds.Width, bounds.Height);
                rect = Rect.Intersect(rect, intersectSrc);
                if (rect != Rect.Empty)
                {
                    rect.X -= sliceRect.Value.X;
                    rect.Y -= sliceRect.Value.Y;
                }
            }
            else
            {
                rect = new Rect(itemData.Left, itemData.Top, itemData.Width, itemData.Height);
            }
        }

        if (rect != Rect.Empty)
        {
            rect.X -= background.Left.Value;
            rect.Y -= background.Top.Value;
        }

        return rect;
    }

    private Rect CalculateConnectorRectWithData(Rect? sliceRect, RenderItemData itemData,
        ConnectorBaseViewModel connector, BackgroundViewModel background)
    {
        var bounds = itemData.Bounds;
        if (bounds.IsEmpty) return Rect.Empty;

        Rect rect;
        if (sliceRect.HasValue)
        {
            rect = sliceRect.Value;
            var intersectSrc = new Rect(itemData.LeftTop, bounds.Size);
            rect = Rect.Intersect(rect, intersectSrc);
            if (rect != Rect.Empty)
            {
                rect.X -= sliceRect.Value.X;
                rect.Y -= sliceRect.Value.Y;
            }
        }
        else
        {
            rect = new Rect(itemData.LeftTop, bounds.Size);
        }

        rect.X -= background.Left.Value;
        rect.Y -= background.Top.Value;
        return rect;
    }

    private BackgroundRenderData CreateBackgroundRenderData(BackgroundViewModel background, List<FrameworkElement> allViews)
    {
        var view = FindViewForDataContext(background, allViews);
        if (view == null)
        {
            s_logger.Warn($"背景ビューが見つかりません: {background}");
            return null;
        }

        // 背景ビューの準備
        view.Measure(new Size(background.Width.Value, background.Height.Value));
        view.Arrange(background.Rect.Value);
        view.UpdateLayout();
        view.SnapsToDevicePixels = true;

        var bounds = VisualTreeHelper.GetDescendantBounds(view);

        return new BackgroundRenderData
        {
            View = view,
            Bounds = bounds,
            Width = background.Width.Value,
            Height = background.Height.Value,
            Left = background.Left.Value,
            Top = background.Top.Value,
            Rect = background.Rect.Value
        };
    }

    private bool RenderBackgroundWithData(Rect? sliceRect, DrawingContext context, BackgroundRenderData backgroundData, BackgroundViewModel background)
    {
        if (backgroundData?.View == null)
        {
            s_logger.Warn($"背景ビューが見つかりません: {background}");
            return false;
        }

        var rect = sliceRect ?? backgroundData.Bounds;

        var brush = new VisualBrush(backgroundData.View)
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

    private RenderItemData CreateRenderItemData(FrameworkElement view, DesignerItemViewModelBase item)
    {
        var PART_ContentPresenter = view.FindName("PART_ContentPresenter") as ContentPresenter;
        var actualView = PART_ContentPresenter ?? view;

        // ビューの現在の状態をキャプチャ
        PrepareViewForRendering(actualView, item);

        var bounds = VisualTreeHelper.GetDescendantBounds(actualView);
        return new RenderItemData
        {
            View = actualView,
            Item = item,
            Bounds = bounds,
            Left = item.Left.Value,
            Top = item.Top.Value,
            Width = item.Width.Value,
            Height = item.Height.Value,
            LeftTop = new Point(item.Left.Value, item.Top.Value)
        };
    }

    public class RenderData
    {
        public Rect? SliceRect { get; set; }
        public List<RenderItemData> RenderItems { get; set; }
        public BackgroundRenderData BackgroundData { get; set; }
        public BackgroundViewModel BackgroundItem { get; set; }
    }

    public class BackgroundRenderData
    {
        public FrameworkElement View { get; set; }
        public Rect Bounds { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public Rect Rect { get; set; }
    }

    public class RenderItemData
    {
        public FrameworkElement View { get; set; }
        public SelectableDesignerItemViewModelBase Item { get; set; }
        public Rect Bounds { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Point LeftTop { get; set; } // Connector用
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

    /// <summary>
    /// キャッシュインスタンスを取得（EffectRendererなどが共有するため）
    /// </summary>
    public RendererCache GetCache()
    {
        return _cache;
    }

    /// <summary>
    /// DrawingVisualキャッシュの無効化
    /// </summary>
    /// <param name="item">無効化する特定のアイテム。nullの場合は全てクリア</param>
    public void InvalidateDrawingVisualCache(object item = null)
    {
        _cache.InvalidateDrawingVisualCache(item);

        if (item == null)
        {
            s_logger.Debug("DrawingVisualキャッシュを全てクリアしました");
        }
        else
        {
            s_logger.Debug($"アイテムをダーティマーク: {item.GetType().Name}");
        }
    }

    /// <summary>
    /// 特定のアイテムとその親をダーティマークする
    /// </summary>
    public void MarkItemDirty(object item)
    {
        _cache.MarkItemDirty(item);
        s_logger.Debug($"アイテムとその親をダーティマーク: {item?.GetType().Name}");
    }

    /// <summary>
    /// キャッシュを使用してアイテムを描画
    /// </summary>
    private bool RenderItemWithDataCached(Rect? sliceRect, DrawingContext context, RenderItemData itemData, BackgroundViewModel background)
    {
        var itemKey = itemData.Item;
        var isDirty = _cache.IsDirty(itemKey);

        // キャッシュの取得または生成
        if (!isDirty && _cache.TryGetDrawingVisual(itemKey, out var cachedVisual))
        {
            // キャッシュが有効な場合、キャッシュされたDrawingVisualを使用
            if (cachedVisual.IsValid(itemData))
            {
                var cachedRect = CalculateRenderRect(sliceRect, itemData, background);
                if (cachedRect == Rect.Empty) return false;

                // キャッシュされたVisualBrushを描画
                context.DrawRectangle(cachedVisual.Brush, null, cachedRect);
                s_logger.Trace($"キャッシュから描画: {itemData.Item.GetType().Name}");
                return true;
            }
            else
            {
                // キャッシュが無効になった場合は削除
                _cache.RemoveDrawingVisual(itemKey);
                s_logger.Debug($"キャッシュが無効: {itemData.Item.GetType().Name}");
            }
        }

        // キャッシュがない、またはダーティな場合は新規に描画してキャッシュ
        var drawingVisual = new DrawingVisual();
        using (var drawingContext = drawingVisual.RenderOpen())
        {
            var brush = GetOrCreateVisualBrush(itemData.View);
            var rect = new Rect(itemData.Left, itemData.Top, itemData.Width, itemData.Height);

            switch (itemData.Item)
            {
                case DesignerItemViewModelBase designerItem:
                    if (Math.Abs(designerItem.RotationAngle.Value) > 0.01)
                    {
                        drawingContext.PushTransform(new RotateTransform(designerItem.RotationAngle.Value,
                            designerItem.CenterX.Value, designerItem.CenterY.Value));
                        drawingContext.DrawRectangle(brush, null, rect);
                        drawingContext.Pop();
                    }
                    else
                    {
                        drawingContext.DrawRectangle(brush, null, rect);
                    }
                    break;

                case ConnectorBaseViewModel:
                    drawingContext.DrawRectangle(brush, null, rect);
                    break;

                default:
                    return false;
            }
        }

        // DrawingVisualをキャッシュに保存
        var visualBrush = new VisualBrush(drawingVisual)
        {
            Stretch = Stretch.None,
            TileMode = TileMode.None
        };

        var cached = new CachedDrawingVisual
        {
            Visual = drawingVisual,
            Brush = visualBrush,
            ItemHash = itemData.GetHashCode(),
            Bounds = itemData.Bounds,
            LastUpdateTime = DateTime.UtcNow
        };

        _cache.AddDrawingVisual(itemKey, cached);

        // ダーティフラグをクリア
        _cache.ClearDirtyFlag(itemKey);

        s_logger.Debug($"新規キャッシュ作成: {itemData.Item.GetType().Name}");

        // 実際の描画
        var renderRect = CalculateRenderRect(sliceRect, itemData, background);
        if (renderRect == Rect.Empty) return false;

        context.DrawRectangle(visualBrush, null, renderRect);
        return true;
    }

    /// <summary>
    /// VisualBrushのキャッシュ取得
    /// </summary>
    private VisualBrush GetOrCreateVisualBrush(FrameworkElement view)
    {
        return _cache.GetOrCreateVisualBrush(view);
    }

    /// <summary>
    /// キャッシュ統計情報の取得（デバッグ用）
    /// </summary>
    public CacheStatistics GetCacheStatistics()
    {
        return _cache.GetStatistics(_dataContextToViewCache.Count);
    }

    /// <summary>
    /// 全てのキャッシュをクリア（拡張版）
    /// </summary>
    public void ClearAllCaches()
    {
        ClearVisualBrushCache();
        InvalidateDrawingVisualCache();
        InvalidateViewCache();

        s_logger.Info("全てのキャッシュをクリアしました");
    }

    /// <summary>
    /// VisualBrushキャッシュをクリア
    /// </summary>
    public void ClearVisualBrushCache()
    {
        _cache.ClearVisualBrushCache();
    }
}