using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        public ReactivePropertySlim<double> StartPointX { get; private set; } = new ReactivePropertySlim<double>();
        public ReactivePropertySlim<double> StartPointY { get; private set; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<Point> StartPoint { get; private set; } = new ReactivePropertySlim<Point>();
        public ObservableCollection<Corner> Corners { get; private set; } = new ObservableCollection<Corner>();

        public ReactivePropertySlim<bool> IsClosed { get; private set; } = new ReactivePropertySlim<bool>();

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
            StartPointX.Subscribe(x =>
            {
                StartPoint.Value = new Point(StartPointX.Value, StartPointY.Value);
                UpdateSegments();
                IsClosed.Value = Corners.Count > 2 && StartPoint.Value == Corners.Last().Point.Value;
            })
            .AddTo(_disposables);
            StartPointY.Subscribe(x =>
            {
                StartPoint.Value = new Point(StartPointX.Value, StartPointY.Value);
                UpdateSegments();
                IsClosed.Value = Corners.Count > 2 && StartPoint.Value == Corners.Last().Point.Value;
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
            Corners.ObserveElementObservableProperty(x => x.Point)
                   .Subscribe(x =>
                   {
                       IsClosed.Value = StartPoint.Value == Corners.Last().Point.Value;
                   })
                   .AddTo(_disposables);
            DrawCommand = IsClosed
                         .ToReactiveCommand();
            DrawCommand.Subscribe(_ =>
            {
                var result = new DialogResult(ButtonResult.OK,
                                              new DialogParameters() 
                                              {
                                                  { "Corners", Corners },
                                                  { "Data", Data.Value },
                                                  { "StartPoint", StartPoint.Value }
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
            Data.Value = "";
            var collection = new PathSegmentCollection();
            var data = $"M {StartPoint.Value}";
            var list = new List<Corner>();
            foreach (var corner in Corners)
            {
                var angle = list.Sum(x => x.Angle.Value);
                var point = new System.Windows.Point(
                                Math.Round(corner.Radius.Value * Math.Cos(angle * Math.PI / 180.0), 2, MidpointRounding.AwayFromZero),
                                Math.Round(corner.Radius.Value * Math.Sin(angle * Math.PI / 180.0), 2, MidpointRounding.AwayFromZero));
                data += $" L {point}";
                collection.Add(
                    new LineSegment()
                    {
                        Point = new System.Windows.Point(
                                Math.Round(corner.Radius.Value * Math.Cos(angle * Math.PI / 180.0), 2, MidpointRounding.AwayFromZero),
                                Math.Round(corner.Radius.Value * Math.Sin(angle * Math.PI / 180.0), 2, MidpointRounding.AwayFromZero)
                            )
                    }
                );
                corner.Point.Value = new Point(
                                            Math.Round(corner.Radius.Value * Math.Cos(angle * Math.PI / 180.0), 2, MidpointRounding.AwayFromZero),
                                            Math.Round(corner.Radius.Value * Math.Sin(angle * Math.PI / 180.0), 2, MidpointRounding.AwayFromZero)
                                            );
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
