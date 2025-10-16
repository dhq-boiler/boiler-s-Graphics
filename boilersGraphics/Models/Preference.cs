using boilersGraphics.Helpers;
using Prism.Mvvm;
using R3;
using System;
using System.Windows.Media;
using ObservableCollections;

namespace boilersGraphics.Models;

internal class Preference : BindableBase
{
    public BindableReactiveProperty<int> Width { get; set; } = new();
    public BindableReactiveProperty<int> Height { get; set; } = new();
    public BindableReactiveProperty<Brush> CanvasFillBrush { get; set; } = new();
    public BindableReactiveProperty<double> CanvasEdgeThickness { get; set; } = new();
    public BindableReactiveProperty<Brush> CanvasEdgeBrush { get; set; } = new();
    public BindableReactiveProperty<bool> EnablePointSnap { get; set; } = new();
    public BindableReactiveProperty<double> SnapPower { get; set; } = new();
    public BindableReactiveProperty<bool> EnableAutoSave { get; set; } = new();
    public BindableReactiveProperty<AutoSaveType> AutoSaveType { get; set; } = new();
    public BindableReactiveProperty<TimeSpan> AutoSaveInterval { get; set; } = new();
    public BindableReactiveProperty<AngleType> AngleType { get; set; } = new();
    public BindableReactiveProperty<bool> EnableImageEmbedding { get; set; } = new();

    public ObservableList<double> EdgeThicknessOptions { get; } = new ObservableList<double>()
    {
        0.0,
        1.0,
        2.0,
        3.0,
        4.0,
        5.0,
        10.0,
        15.0,
        20.0,
        25.0,
        30.0,
        35.0,
        40.0,
        45.0,
        50.0,
        100.0
    };
}

public enum AutoSaveType
{
    EveryTimeCampusChanges,
    SetInterval
}