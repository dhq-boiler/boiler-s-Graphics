using System;
using System.Windows.Media;
using boilersGraphics.Helpers;
using Prism.Mvvm;
using Reactive.Bindings;

namespace boilersGraphics.Models;

internal class Preference : BindableBase
{
    public ReactivePropertySlim<int> Width { get; set; } = new();
    public ReactivePropertySlim<int> Height { get; set; } = new();
    public ReactivePropertySlim<Brush> CanvasBackground { get; set; } = new();
    public ReactivePropertySlim<double> CanvasEdgeThickness { get; set; } = new();
    public ReactivePropertySlim<bool> EnablePointSnap { get; set; } = new();
    public ReactivePropertySlim<double> SnapPower { get; set; } = new();
    public ReactivePropertySlim<bool> EnableAutoSave { get; set; } = new();
    public ReactivePropertySlim<AutoSaveType> AutoSaveType { get; set; } = new();
    public ReactivePropertySlim<TimeSpan> AutoSaveInterval { get; set; } = new();
    public ReactivePropertySlim<AngleType> AngleType { get; set; } = new();
    public ReactivePropertySlim<bool> EnableImageEmbedding { get; set; } = new();

    public ReactiveCollection<double> EdgeThicknessOptions { get; } = new ReactiveCollection<double>()
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