using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Reactive.Bindings.Extensions;
using SharpDX;

namespace boilersGraphics.ViewModels.ColorCorrect
{
    public class ToneCurveViewModel : BindableBase, INavigationAware, IDisposable
    {
        private CompositeDisposable _disposable = new CompositeDisposable();
        private bool _disposedValue;
        public ReactivePropertySlim<ColorCorrectViewModel> ViewModel { get; } = new();
        public ReactiveCollection<Point> Points { get; } = new();

        public class Point : BindableBase
        {
            public Point() { }

            public Point(double x, double y)
            {
                X.Value = x;
                Y.Value = y;
            }

            public ReactivePropertySlim<double> X { get; } = new ReactivePropertySlim<double>();
            public ReactivePropertySlim<double> Y { get; } = new ReactivePropertySlim<double>();
        }

        public ReactiveCommand<DragDeltaEventArgs> DragDeltaCommand { get; } = new();

        public ToneCurveViewModel()
        {
            Points.Add(new Point(0, 300));
            Points.Add(new Point(150, 150));
            Points.Add(new Point(300, 0));
            DragDeltaCommand.Subscribe(x =>
            { 
                Point unboxedPoint = (Point)(x.Source as Thumb).DataContext;
                if (Points.First() == unboxedPoint || Points.Last() == unboxedPoint)
                {
                    return;
                }
                int index = Points.IndexOf(unboxedPoint);
                unboxedPoint.X.Value = Math.Clamp(unboxedPoint.X.Value + x.HorizontalChange, 0, 300);
                unboxedPoint.Y.Value = Math.Clamp(unboxedPoint.Y.Value + x.VerticalChange, 0, 300);
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Points)));
            }).AddTo(_disposable);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel.Value = navigationContext.Parameters.GetValue<ColorCorrectViewModel>("ViewModel");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _disposable.Dispose();
                }

                _disposable = null;
                _disposedValue = true;
            }
        }

        // // TODO: 'Dispose(bool disposing)' にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします
        // ~ToneCureveViewModel()
        // {
        //     // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
