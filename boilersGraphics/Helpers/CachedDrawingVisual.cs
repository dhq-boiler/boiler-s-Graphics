using System;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers;

/// <summary>
/// DrawingVisualとそのブラシをキャッシュするクラス
/// </summary>
public class CachedDrawingVisual : IDisposable
{
    public DrawingVisual Visual { get; set; }
    public VisualBrush Brush { get; set; }
    public int ItemHash { get; set; }
    public Rect Bounds { get; set; }
    public DateTime LastUpdateTime { get; set; }
    
    private bool _disposed = false;

    /// <summary>
    /// キャッシュが有効かどうかを判定
    /// </summary>
    public bool IsValid(Renderer.RenderItemData itemData)
    {
        // ハッシュ値が一致し、サイズが変わっていなければ有効
        return ItemHash == itemData.GetHashCode() && 
               Bounds == itemData.Bounds;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Visual = null;
            Brush = null;
            _disposed = true;
        }
    }
}

/// <summary>
/// キャッシュ統計情報
/// </summary>
public class CacheStatistics
{
    public int DrawingVisualCacheCount { get; set; }
    public int VisualBrushCacheCount { get; set; }
    public int DirtyItemsCount { get; set; }
    public int ViewCacheCount { get; set; }
    public string LastModifiedItem { get; set; }

    public override string ToString()
    {
        return $"DrawingVisuals: {DrawingVisualCacheCount}, VisualBrushes: {VisualBrushCacheCount}, " +
               $"Dirty: {DirtyItemsCount}, Views: {ViewCacheCount}, Last Modified: {LastModifiedItem ?? "None"}";
    }
}
