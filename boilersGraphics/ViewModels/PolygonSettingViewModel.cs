﻿using boilersGraphics.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    class PolygonSettingViewModel : BindableBase, IDialogAware, IDisposable
    {
        private bool disposedValue;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public event Action<IDialogResult> RequestClose;

        public ReactivePropertySlim<string> Data { get; private set; } = new ReactivePropertySlim<string>();
        public ReactivePropertySlim<int> Angle { get; private set; } = new ReactivePropertySlim<int>();

        public ObservableCollection<Corner> Corners { get; private set; } = new ObservableCollection<Corner>();

        public ReactiveCommand AddCornerCommand { get; } = new ReactiveCommand();

        public ReactiveCommand<Corner> RemoveCornerCommand { get; } = new ReactiveCommand<Corner>();

        public ReactiveCommand DrawCommand { get; }

        public DelegateCommand<KeyEventArgs> KeyDownCommand { get; }

        public string Title => "多角形";

        public PolygonSettingViewModel()
        {
            AddCornerCommand.Subscribe(_ =>
            {
                AddCorner();
            })
            .AddTo(_disposables);
            RemoveCornerCommand.Subscribe(x =>
            {
                RemoveCorner(x);
            })
            .AddTo(_disposables);
            Angle.Subscribe(x =>
            {
                UpdateSegments();
            })
            .AddTo(_disposables);
            Corners.ObserveElementObservableProperty(x => x.Angle)
                   .Subscribe(x =>
                   {
                       UpdateSegments();
                   })
                   .AddTo(_disposables);
            Corners.ObserveElementObservableProperty(x => x.Radius)
                   .Subscribe(x =>
                   {
                       UpdateSegments();
                   })
                   .AddTo(_disposables);
            DrawCommand = new ReactiveCommand();
            DrawCommand.Subscribe(_ =>
            {
                var result = new DialogResult(ButtonResult.OK,
                                              new DialogParameters() 
                                              {
                                                  { "Corners", Corners },
                                                  { "Data", Data.Value },
                                              }
                                              );
                RequestClose.Invoke(result);
            })
            .AddTo(_disposables);
            KeyDownCommand = new DelegateCommand<KeyEventArgs>(args =>
            {
                if (args.Key == Key.Add)
                {
                    AddCorner();
                    args.Handled = true;
                }
            });

            var corner = new Corner();
            corner.Number.Value = 1;
            corner.Angle.Value = 36;
            corner.Radius.Value = 10;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 2;
            corner.Angle.Value = 36;
            corner.Radius.Value = 5;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 3;
            corner.Angle.Value = 36;
            corner.Radius.Value = 10;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 4;
            corner.Angle.Value = 36;
            corner.Radius.Value = 5;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 5;
            corner.Angle.Value = 36;
            corner.Radius.Value = 10;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 6;
            corner.Angle.Value = 36;
            corner.Radius.Value = 5;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 7;
            corner.Angle.Value = 36;
            corner.Radius.Value = 10;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 8;
            corner.Angle.Value = 36;
            corner.Radius.Value = 5;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 9;
            corner.Angle.Value = 36;
            corner.Radius.Value = 10;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 10;
            corner.Angle.Value = 36;
            corner.Radius.Value = 5;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 11;
            corner.Angle.Value = 36;
            corner.Radius.Value = 10;
            Corners.Add(corner);
            corner = new Corner();
            corner.Number.Value = 12;
            corner.Angle.Value = 36;
            corner.Radius.Value = 5;
            Corners.Add(corner);
            UpdateSegments();
        }

        private void RemoveCorner(Corner x)
        {
            int indexOf = Corners.IndexOf(x);
            Corners.Remove(x);
            Corners.Where(y => y.Number.Value - 1 > indexOf).ToList().ForEach(y => y.Number.Value -= 1);
            UpdateSegments();
        }

        private Corner AddCorner()
        {
            var corner = new Corner();
            corner.Number.Value = Corners.Count + 1;
            Corners.Add(corner);
            return corner;
        }

        private void UpdateSegments()
        {
            if (Corners.Count() <= 1) return;
            Data.Value = "";
            var x = Corners.Skip(1);
            var data = $"M {x.First().Point.Value}";
            var list = new List<Corner>();
            foreach (var corner in Corners.Skip(1))
            {
                var angle = list.Sum(x => x.Angle.Value);
                var θ = (angle - Angle.Value) * Math.PI / 180.0;
                var point = new System.Windows.Point(
                                Math.Round(corner.Radius.Value * Math.Cos(θ), 2, MidpointRounding.AwayFromZero),
                                Math.Round(corner.Radius.Value * Math.Sin(θ), 2, MidpointRounding.AwayFromZero));
                data += $" L {point}";
                corner.Point.Value = point;
                list.Add(corner);
            }
            data += " Z";
            Data.Value = data;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
