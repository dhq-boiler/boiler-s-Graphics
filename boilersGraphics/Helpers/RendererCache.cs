using boilersGraphics.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers;

/// <summary>
/// レンダリングキャッシュを管理するクラス
/// Renderer間で共有可能
/// </summary>
public class RendererCache
{
    // DrawingVisualキャッシュ用フィールド
    private readonly Dictionary<object, CachedDrawingVisual> _drawingVisualCache = new();
    private readonly HashSet<object> _dirtyItems = new();
    private object _lastModifiedItem;
    private readonly Dictionary<FrameworkElement, VisualBrush> _visualBrushCache = new();

    /// <summary>
    /// VisualBrushキャッシュを取得または作成
    /// </summary>
    public VisualBrush GetOrCreateVisualBrush(FrameworkElement view)
    {
        if (_visualBrushCache.TryGetValue(view, out var cachedBrush))
        {
            return cachedBrush;
        }

        var brush = new VisualBrush(view)
        {
            Stretch = Stretch.None,
            TileMode = TileMode.None
        };

        _visualBrushCache[view] = brush;
        return brush;
    }

    /// <summary>
    /// VisualBrushキャッシュをクリア
    /// </summary>
    public void ClearVisualBrushCache()
    {
        _visualBrushCache.Clear();
    }

    /// <summary>
    /// DrawingVisualキャッシュを取得
    /// </summary>
    public bool TryGetDrawingVisual(object key, out CachedDrawingVisual cached)
    {
        return _drawingVisualCache.TryGetValue(key, out cached);
    }

    /// <summary>
    /// DrawingVisualキャッシュに追加
    /// </summary>
    public void AddDrawingVisual(object key, CachedDrawingVisual cached)
    {
        _drawingVisualCache[key] = cached;
    }

    /// <summary>
    /// DrawingVisualキャッシュから削除
    /// </summary>
    public bool RemoveDrawingVisual(object key)
    {
        if (_drawingVisualCache.TryGetValue(key, out var cached))
        {
            cached?.Dispose();
            _drawingVisualCache.Remove(key);
            return true;
        }
        return false;
    }

    /// <summary>
    /// アイテムがダーティかどうかを確認
    /// </summary>
    public bool IsDirty(object item)
    {
        return _dirtyItems.Contains(item);
    }

    /// <summary>
    /// ダーティフラグをクリア
    /// </summary>
    public void ClearDirtyFlag(object item)
    {
        _dirtyItems.Remove(item);
    }

    /// <summary>
    /// アイテムをダーティマーク
    /// </summary>
    public void MarkItemDirty(object item)
    {
        if (item == null) return;

        _dirtyItems.Add(item);
        _lastModifiedItem = item;

        // ViewModelの親階層もダーティマークする
        if (item is SelectableDesignerItemViewModelBase designerItem)
        {
            var parent = designerItem.GetParent();
            if (parent != null)
            {
                _dirtyItems.Add(parent);
            }
        }
    }

    /// <summary>
    /// DrawingVisualキャッシュを無効化
    /// </summary>
    public void InvalidateDrawingVisualCache(object item = null)
    {
        if (item == null)
        {
            foreach (var cached in _drawingVisualCache.Values)
            {
                cached.Dispose();
            }
            _drawingVisualCache.Clear();
            _dirtyItems.Clear();
        }
        else
        {
            MarkItemDirty(item);
        }
    }

    /// <summary>
    /// すべてのキャッシュをクリア
    /// </summary>
    public void ClearAll()
    {
        foreach (var cached in _drawingVisualCache.Values)
        {
            cached.Dispose();
        }
        _drawingVisualCache.Clear();
        _visualBrushCache.Clear();
        _dirtyItems.Clear();
        _lastModifiedItem = null;
    }

    /// <summary>
    /// キャッシュ統計情報を取得
    /// </summary>
    public CacheStatistics GetStatistics(int viewCacheCount)
    {
        return new CacheStatistics
        {
            DrawingVisualCacheCount = _drawingVisualCache.Count,
            VisualBrushCacheCount = _visualBrushCache.Count,
            DirtyItemsCount = _dirtyItems.Count,
            ViewCacheCount = viewCacheCount,
            LastModifiedItem = _lastModifiedItem?.GetType().Name
        };
    }
}
