﻿using boilersGraphics.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public abstract class ConnectorBaseViewModel : SelectableDesignerItemViewModelBase, IObserver<TransformNotification>, ICloneable
    {
        private ObservableCollection<Point> _Points;

        public ConnectorBaseViewModel(int id, IDiagramViewModel parent) : base(id, parent)
        {
            Init();
        }

        public ConnectorBaseViewModel()
        {
            Init();
        }

        public ReactiveProperty<Point> LeftTop { get; set; }

        public ReadOnlyReactivePropertySlim<double> Width { get; set; }

        public ReadOnlyReactivePropertySlim<double> Height { get; set; }

        public ObservableCollection<Point> Points
        {
            get { return _Points; }
            set { SetProperty(ref _Points, value); }
        }
        public ReadOnlyReactivePropertySlim<SnapPointViewModel> SnapPoint0VM { get; protected set; }
        public ReadOnlyReactivePropertySlim<SnapPointViewModel> SnapPoint1VM { get; protected set; }

        private void Init()
        {
            _Points = new ObservableCollection<Point>();
            InitPathFinder();
            LeftTop = Points.ObserveProperty(x => x.Count)
                            .Where(x => x > 0)
                            .Select(_ => new Point(Points.Min(x => x.X), Points.Min(x => x.Y)))
                            .ToReactiveProperty();
            Width = Points.ObserveProperty(x => x.Count)
                          .Where(x => x > 0)
                          .Select(_ => Points.Max(x => x.X) - Points.Min(x => x.X))
                          .ToReadOnlyReactivePropertySlim();
            Height = Points.ObserveProperty(x => x.Count)
                          .Where(x => x > 0)
                          .Select(_ => Points.Max(x => x.Y) - Points.Min(x => x.Y))
                          .ToReadOnlyReactivePropertySlim();
        }

        protected virtual void InitPathFinder() { }

        #region IObserver<TransformNotification>

        public void OnNext(TransformNotification value)
        {
        }

        #endregion //IObserver<TransformNotification>

        #region IObserver<GroupTransformNotification>

        public override void OnNext(GroupTransformNotification value)
        {
            var oldWidth = value.OldWidth;
            var oldHeight = value.OldHeight;

            switch (value.Type)
            {
                case TransformType.Move:
                    var a = Points[0];
                    var b = Points[1];
                    a.X += value.LeftChange;
                    b.X += value.LeftChange;
                    a.Y += value.TopChange;
                    b.Y += value.TopChange;
                    Points[0] = a;
                    Points[1] = b;
                    break;
                case TransformType.Resize:
                    a = Points[0];
                    b = Points[1];
                    a.X = (a.X - value.GroupLeftTop.X) * ((oldWidth + value.WidthChange) / (oldWidth)) + value.GroupLeftTop.X;
                    b.X = (b.X - value.GroupLeftTop.X) * ((oldWidth + value.WidthChange) / (oldWidth)) + value.GroupLeftTop.X;
                    a.Y = (a.Y - value.GroupLeftTop.Y) * ((oldHeight + value.HeightChange) / (oldHeight)) + value.GroupLeftTop.Y;
                    b.Y = (b.Y - value.GroupLeftTop.Y) * ((oldHeight + value.HeightChange) / (oldHeight)) + value.GroupLeftTop.Y;
                    Points[0] = a;
                    Points[1] = b;
                    break;
                case TransformType.Rotate:
                    a = Points[0];
                    b = Points[1];
                    var diffAngle = value.RotateAngleChange;
                    var center = value.GroupCenter;
                    var matrix = new Matrix();
                    //derive rotated 0 degree point
                    matrix.RotateAt(-RotationAngle.Value, center.X, center.Y);
                    var origA = matrix.Transform(a);
                    var origB = matrix.Transform(b);
                    //derive rotated N degrees point from rotated 0 degree point in transform result
                    matrix = new Matrix();
                    RotationAngle.Value += diffAngle;
                    matrix.RotateAt(RotationAngle.Value, center.X, center.Y);
                    var newA = matrix.Transform(origA);
                    var newB = matrix.Transform(origB);
                    Points[0] = newA;
                    Points[1] = newB;
                    break;
            }
        }

        #endregion //IObserver<TransformNotification>
    }
}
