using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using boilersGraphics.Helpers;
using Reactive.Bindings.Extensions;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.ViewModels;

public class GroupItemViewModel : DesignerItemViewModelBase, IObservable<GroupTransformNotification>, ICloneable
{
    private double _heightOld;
    private double _leftOld;
    private double _lotateAngleOld;
    private double _topOld;
    private double _widthOld;

    public GroupItemViewModel()
    {
        Left.Subscribe(l =>
            {
                var notification = new GroupTransformNotification
                {
                    Type = TransformType.Move,
                    LeftChange = l - _leftOld
                };
                GroupTransformObserversOnNext(notification);
                _leftOld = l;
            })
            .AddTo(_CompositeDisposable);
        Top.Subscribe(t =>
            {
                var notification = new GroupTransformNotification
                {
                    Type = TransformType.Move,
                    TopChange = t - _topOld
                };
                GroupTransformObserversOnNext(notification);
                _topOld = t;
            })
            .AddTo(_CompositeDisposable);
        Width.Subscribe(w =>
            {
                var notification = new GroupTransformNotification
                {
                    Type = TransformType.Resize,
                    GroupLeftTop = new Point(Left.Value, Top.Value),
                    OldWidth = _widthOld,
                    OldHeight = _heightOld,
                    WidthChange = w - _widthOld
                };
                GroupTransformObserversOnNext(notification);
                _widthOld = w;
            })
            .AddTo(_CompositeDisposable);
        Height.Subscribe(h =>
            {
                var notification = new GroupTransformNotification
                {
                    Type = TransformType.Resize,
                    GroupLeftTop = new Point(Left.Value, Top.Value),
                    OldWidth = _widthOld,
                    OldHeight = _heightOld,
                    HeightChange = h - _heightOld
                };
                GroupTransformObserversOnNext(notification);
                _heightOld = h;
            })
            .AddTo(_CompositeDisposable);
        RotationAngle.Subscribe(a =>
            {
                var notification = new GroupTransformNotification
                {
                    Type = TransformType.Rotate,
                    GroupLeftTop = new Point(Left.Value, Top.Value),
                    GroupCenter = new Point(Left.Value + Width.Value / 2, Top.Value + Height.Value / 2),
                    RotateAngleChange = a - _lotateAngleOld
                };
                GroupTransformObserversOnNext(notification);

                _lotateAngleOld = a;
            })
            .AddTo(_CompositeDisposable);
        UpdatingStrategy.Value = PathGeometryUpdatingStrategy.Fixed;
    }

    private void GroupTransformObserversOnNext(GroupTransformNotification notification)
    {
        observers.ForEach(x => x.OnNext(notification));
    }

    public void AddGroup(OperationRecorder recorder, SelectableDesignerItemViewModelBase viewModel)
    {
        recorder.Current.ExecuteSetProperty(viewModel, "GroupDisposable", Subscribe(viewModel));
    }

    public override PathGeometry CreateGeometry(bool flag = false)
    {
        return null;
    }


    public override Type GetViewType()
    {
        return typeof(DockPanel);
    }

    #region IObservable<GroupTransformNotification>

    public List<IObserver<GroupTransformNotification>> observers = new();

    public override bool SupportsPropertyDialog => false;

    public IDisposable Subscribe(IObserver<GroupTransformNotification> observer)
    {
        observers.Add(observer);
        return new GroupItemViewModelDisposable(this, observer);
    }

    private class GroupItemViewModelDisposable : IDisposable
    {
        private readonly GroupItemViewModel _groupItemViewModel;
        private readonly IObserver<GroupTransformNotification> _observer;

        public GroupItemViewModelDisposable(GroupItemViewModel groupItemViewModel,
            IObserver<GroupTransformNotification> observer)
        {
            _groupItemViewModel = groupItemViewModel;
            _observer = observer;
        }

        public void Dispose()
        {
            _groupItemViewModel.observers.Remove(_observer);
        }
    }

    #endregion IObservable<GroupTransformNotification>

    #region IClonable

    public override object Clone()
    {
        var clone = new GroupItemViewModel();
        clone.Owner = Owner;
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.Matrix.Value = Matrix.Value;
        clone.RotationAngle.Value = RotationAngle.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        return clone;
    }

    public override void OpenPropertyDialog()
    {
        throw new NotImplementedException();
    }

    #endregion //IClonable
}