﻿using Prism.Mvvm;

namespace boilersGraphics.Models;

public class DraggingItem : BindableBase
{
    private object _Item;
    private double _XOffset;
    private double _YOffset;

    public object Item
    {
        get => _Item;
        set => SetProperty(ref _Item, value);
    }

    /// <summary>
    ///     AssociatedObjectを基準とした相対座標のX値
    /// </summary>
    public double XOffset
    {
        get => _XOffset;
        set => SetProperty(ref _XOffset, value);
    }

    /// <summary>
    ///     AssociatedObjectを基準とした相対座標のY値
    /// </summary>
    public double YOffset
    {
        get => _YOffset;
        set => SetProperty(ref _YOffset, value);
    }
}