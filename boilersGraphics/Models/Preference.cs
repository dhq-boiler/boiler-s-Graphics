using boilersGraphics.Helpers;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Windows.Media;

namespace boilersGraphics.Models
{
    class Preference : BindableBase
    {
        public ReactivePropertySlim<int> Width { get; set; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> Height { get; set; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<Brush> CanvasBackground { get; set; } = new ReactivePropertySlim<Brush>();
        public ReactivePropertySlim<bool> EnablePointSnap { get; set; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<double> SnapPower { get; set; } = new ReactivePropertySlim<double>();
        public ReactivePropertySlim<bool> EnableAutoSave { get; set; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<AutoSaveType> AutoSaveType { get; set; } = new ReactivePropertySlim<AutoSaveType>();
        public ReactivePropertySlim<TimeSpan> AutoSaveInterval { get; set; } = new ReactivePropertySlim<TimeSpan>();
        public ReactivePropertySlim<AngleType> AngleType { get; set; } = new ReactivePropertySlim<AngleType>();
        public ReactivePropertySlim<bool> EnableImageEmbedding { get; set; } = new ReactivePropertySlim<bool>();
    }

    public enum AutoSaveType
    {
        EveryTimeCampusChanges,
        SetInterval,
    }
}
