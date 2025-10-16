using boilersGraphics.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers;

/// <summary>
/// �����_�����O�L���b�V�����Ǘ�����N���X
/// Renderer�Ԃŋ��L�\
/// </summary>
public class RendererCache
{
    // DrawingVisual�L���b�V���p�t�B�[���h
    private readonly Dictionary<object, CachedDrawingVisual> _drawingVisualCache = new();
    private readonly HashSet<object> _dirtyItems = new();
    private object _lastModifiedItem;
    private readonly Dictionary<FrameworkElement, VisualBrush> _visualBrushCache = new();

    /// <summary>
    /// VisualBrush�L���b�V�����擾�܂��͍쐬
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
    /// VisualBrush�L���b�V�����N���A
    /// </summary>
    public void ClearVisualBrushCache()
    {
        _visualBrushCache.Clear();
    }

    /// <summary>
    /// DrawingVisual�L���b�V�����擾
    /// </summary>
    public bool TryGetDrawingVisual(object key, out CachedDrawingVisual cached)
    {
        return _drawingVisualCache.TryGetValue(key, out cached);
    }

    /// <summary>
    /// DrawingVisual�L���b�V���ɒǉ�
    /// </summary>
    public void AddDrawingVisual(object key, CachedDrawingVisual cached)
    {
        _drawingVisualCache[key] = cached;
    }

    /// <summary>
    /// DrawingVisual�L���b�V������폜
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
    /// �A�C�e�����_�[�e�B���ǂ������m�F
    /// </summary>
    public bool IsDirty(object item)
    {
        return _dirtyItems.Contains(item);
    }

    /// <summary>
    /// �_�[�e�B�t���O���N���A
    /// </summary>
    public void ClearDirtyFlag(object item)
    {
        _dirtyItems.Remove(item);
    }

    /// <summary>
    /// �A�C�e�����_�[�e�B�}�[�N
    /// </summary>
    public void MarkItemDirty(object item)
    {
        if (item == null) return;

        _dirtyItems.Add(item);
        _lastModifiedItem = item;

        // ViewModel�̐e�K�w���_�[�e�B�}�[�N����
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
    /// DrawingVisual�L���b�V���𖳌���
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
    /// ���ׂẴL���b�V�����N���A
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
    /// �L���b�V�����v�����擾
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
