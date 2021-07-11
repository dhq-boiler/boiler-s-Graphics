using boilersGraphics.Models;
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
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    class PolygonSettingViewModel : BindableBase, IDialogAware, IDisposable
    {
        private bool disposedValue;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public event Action<IDialogResult> RequestClose;

        public ReactiveProperty<PathSegmentCollection> Segments { get; private set; } = new ReactiveProperty<PathSegmentCollection>();
        public ReactiveProperty<double> StartPointX { get; private set; } = new ReactiveProperty<double>();
        public ReactiveProperty<double> StartPointY { get; private set; } = new ReactiveProperty<double>();

        public ReactiveProperty<Point> StartPoint { get; private set; } = new ReactiveProperty<Point>();
        public ObservableCollection<Corner> Corners { get; private set; } = new ObservableCollection<Corner>();

        public ReactiveProperty<bool> IsClosed { get; private set; } = new ReactiveProperty<bool>();

        public ReactiveCommand AddCornerCommand { get; } = new ReactiveCommand();

        public ReactiveCommand<Corner> RemoveCornerCommand { get; } = new ReactiveCommand<Corner>();

        public ReactiveCommand DrawCommand { get; }

        public string Title => "多角形";

        public PolygonSettingViewModel()
        {
            AddCornerCommand.Subscribe(_ =>
            {
                var corner = new Corner();
                corner.Number.Value = Corners.Count + 1;
                Corners.Add(corner);
            })
            .AddTo(_disposables);
            Segments.Value = new PathSegmentCollection();
            RemoveCornerCommand.Subscribe(x =>
            {
                int indexOf = Corners.IndexOf(x);
                Corners.Remove(x);
                Corners.Where(y => y.Number.Value - 1 > indexOf).ToList().ForEach(y => y.Number.Value -= 1);
                UpdateSegments();
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
                                                  { "Segments", Segments }
                                              }
                                              );
                RequestClose.Invoke(result);
            })
            .AddTo(_disposables);
        }

        private void UpdateSegments()
        {
            Segments.Value.Clear();
            var collection = new PathSegmentCollection();

            var list = new List<Corner>();
            foreach (var corner in Corners)
            {
                var angle = list.Sum(x => x.Angle.Value);
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
            Segments.Value = collection;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                Segments = null;
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
