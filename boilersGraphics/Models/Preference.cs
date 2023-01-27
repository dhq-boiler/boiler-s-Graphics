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
    public ReactivePropertySlim<bool> EnablePointSnap { get; set; } = new();
    public ReactivePropertySlim<double> SnapPower { get; set; } = new();
    public ReactivePropertySlim<bool> EnableAutoSave { get; set; } = new();
    public ReactivePropertySlim<AutoSaveType> AutoSaveType { get; set; } = new();
    public ReactivePropertySlim<TimeSpan> AutoSaveInterval { get; set; } = new();
    public ReactivePropertySlim<AngleType> AngleType { get; set; } = new();
    public ReactivePropertySlim<bool> EnableImageEmbedding { get; set; } = new();
}

public enum AutoSaveType
{
    EveryTimeCampusChanges,
    SetInterval
}