﻿using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Windows.Media;

namespace boilersGraphics.Models
{
    class Preference : BindableBase
    {
        public ReactivePropertySlim<int> Width { get; set; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> Height { get; set; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<Color> CanvasBackground { get; set; } = new ReactivePropertySlim<Color>();
        public ReactivePropertySlim<bool> EnablePointSnap { get; set; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<double> SnapPower { get; set; } = new ReactivePropertySlim<double>();
        public ReactivePropertySlim<bool> EnableAutoSave { get; set; } = new ReactivePropertySlim<bool>();
        public ReactivePropertySlim<AutoSaveType> AutoSaveType { get; set; } = new ReactivePropertySlim<AutoSaveType>();
        public ReactivePropertySlim<TimeSpan> AutoSaveInterval { get; set; } = new ReactivePropertySlim<TimeSpan>();
    }

    public enum AutoSaveType
    {
        EveryTimeCampusChanges,
        SetInterval,
    }
}