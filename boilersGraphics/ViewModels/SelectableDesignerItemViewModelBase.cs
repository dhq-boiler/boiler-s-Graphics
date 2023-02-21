using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.ViewModels;

public interface ISelectItems
{
    DelegateCommand<object> SelectItemCommand { get; }
}

public abstract class SelectableDesignerItemViewModelBase : BindableBase, ISelectItems,
    IObserver<TransformNotification>, IObserver<GroupTransformNotification>, IObservable<TransformNotification>,
    IDisposable, ICloneable, IRestore
{
    protected CompositeDisposable _CompositeDisposable = new();

    public SelectableDesignerItemViewModelBase(int id, IDiagramViewModel parent)
    {
        Id = id;
        Owner = parent;
        Init();
    }

    public SelectableDesignerItemViewModelBase()
    {
        Init();
    }

    public static int SelectedOrderCount { get; set; } = 0;

    public SelectableDesignerItemViewModelBase[] SelectedItems => Owner.SelectedItems.Value;

    public IDiagramViewModel Owner { get; set; }
    public int Id { get; set; }

    // ↓ Flags ↓

    public ReactivePropertySlim<bool> IsSelected { get; } = new();
    public ReactivePropertySlim<bool> EnableForSelection { get; } = new();
    public ReactivePropertySlim<PathGeometryUpdatingStrategy> UpdatingStrategy { get; } = new();
    public enum PathGeometryUpdatingStrategy
    {
        Unknown,
        Initial,
        ResizeWhilePreservingOriginalShape,
        Fixed,
    }
    public ReactivePropertySlim<bool> IsVisible { get; } = new();
    public ReactivePropertySlim<bool> IsHitTestVisible { get; set; } = new();
    public ReactivePropertySlim<bool> CanDrag { get; set; } = new(true);

    // ↑ Flags ↑

    public ReactivePropertySlim<object> ChangeFormTriggerObject { get; } = new();
    public ReactivePropertySlim<LayerItem> LayerItem { get; } = new();

    public ReactivePropertySlim<int> SelectedOrder { get; } = new();

    public ReactivePropertySlim<Matrix> Matrix { get; } = new(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe |
                                                                    ReactivePropertyMode.DistinctUntilChanged);

    public ReactivePropertySlim<double> RotationAngle { get; } =
        new(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

    public ReactivePropertySlim<int> ZIndex { get; } = new();
    public ReactivePropertySlim<Brush> EdgeBrush { get; } = new(Brushes.Transparent);
    public ReactivePropertySlim<Brush> FillBrush { get; } = new(Brushes.Transparent);

    public ReactivePropertySlim<double> EdgeThickness { get; } =
        new(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);

    public ReadOnlyReactivePropertySlim<double> HalfEdgeThickness
    {
        get { return EdgeThickness.Select(x => x / 2).ToReadOnlyReactivePropertySlim(); }
    }

    public ReadOnlyReactivePropertySlim<PathGeometry> PathGeometry { get; set; }
    public ReactivePropertySlim<PathGeometry> PathGeometryNoRotate { get; } = new();
    public ReactivePropertySlim<PathGeometry> PathGeometryRotate { get; } = new();
    public ReactivePropertySlim<PenLineJoin> StrokeLineJoin { get; } = new();
    public ReactiveCollection<PenLineJoin> PenLineJoins { get; private set; }
    public ReactivePropertySlim<DoubleCollection> StrokeDashArray { get; } = new();
    public ReactivePropertySlim<double> StrokeMiterLimit { get; } = new();

    public string Name { get; set; }

    public Guid ID { get; set; } = Guid.NewGuid();

    public Guid ParentID { get; set; }

    public IDisposable GroupDisposable { get; set; }

    public ReadOnlyReactivePropertySlim<double> SnapPointSize { get; } = Observable.Return(4)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactivePropertySlim();

    public ReadOnlyReactivePropertySlim<double> ThumbSize { get; } = Observable.Return(7)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactivePropertySlim();

    public ReadOnlyReactivePropertySlim<double> ThumbThickness { get; } = Observable.Return(1)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactivePropertySlim();

    public ReadOnlyReactivePropertySlim<Thickness> RotateThumbMargin { get; } = Observable.Return(-20)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return new Thickness(0, standardSize / (rate / 100d), 0, 0); })
        .ToReadOnlyReactivePropertySlim();

    public ReadOnlyReactivePropertySlim<Thickness> RotateThumbConnectorMargin { get; } = Observable.Return(-11)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return new Thickness(0, standardSize / (rate / 100d), 0, 0); })
        .ToReadOnlyReactivePropertySlim();

    public ReadOnlyReactivePropertySlim<double> RotateThumbConnectorY2 { get; } = Observable.Return(11)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactivePropertySlim();

    public ReadOnlyReactivePropertySlim<double> RotateThumbConnectorThickness { get; } = Observable.Return(1)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactivePropertySlim();

    public ReadOnlyReactivePropertySlim<Thickness> RotateThumbGridMargin { get; } = Observable.Return(-3)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) =>
            {
                return new Thickness(standardSize / (rate / 100d), standardSize / (rate / 100d),
                    standardSize / (rate / 100d), standardSize / (rate / 100d));
            }).ToReadOnlyReactivePropertySlim();

    public abstract bool SupportsPropertyDialog { get; }

    #region IClonable

    public abstract object Clone();

    #endregion //IClonable

    #region IDisposable

    public virtual void Dispose()
    {
        IsSelected.Dispose();
        EnableForSelection.Dispose();
        UpdatingStrategy.Dispose();
        IsVisible.Dispose();
        IsHitTestVisible.Dispose();
        CanDrag.Dispose();
        SelectedOrder.Dispose();
        Matrix.Dispose();
        RotationAngle.Dispose();
        ZIndex.Dispose();
        EdgeBrush.Dispose();
        FillBrush.Dispose();
        EdgeThickness.Dispose();
        HalfEdgeThickness.Dispose();
        if (PathGeometry is not null)
            PathGeometry.Dispose();
        if (PathGeometryNoRotate is not null)
            PathGeometryNoRotate.Dispose();
        if (PathGeometryRotate is not null)
            PathGeometryRotate.Dispose();
        StrokeLineJoin.Dispose();
        PenLineJoins.Dispose();
        StrokeDashArray.Dispose();
        StrokeMiterLimit.Dispose();
        SnapPointSize.Dispose();
        ThumbSize.Dispose();
        ThumbThickness.Dispose();
        RotateThumbMargin.Dispose();
        RotateThumbConnectorMargin.Dispose();
        RotateThumbConnectorY2.Dispose();
        RotateThumbConnectorThickness.Dispose();
        RotateThumbGridMargin.Dispose();
        if (_CompositeDisposable != null) _CompositeDisposable.Dispose();
        _CompositeDisposable = null;
    }

    #endregion //IDisposable

    #region IObserver<TransformNotification>

    public void OnNext(TransformNotification value)
    {
    }

    #endregion IObserver<TransformNotification>

    protected void UpdateChangeFormTriggerObject()
    {
        ChangeFormTriggerObject.Value = new object();
    }

    public void Restore(Action restorePropertiesAction)
    {
        restorePropertiesAction.Invoke();
    }

    public DelegateCommand<object> SelectItemCommand { get; private set; }

    public IDisposable Connect(SnapPointPosition snapPointEdgeTo, SnapPointPosition snapPointEdgeFrom,
        SelectableDesignerItemViewModelBase item)
    {
        var spai = new SnapPointAdsorptionInformation(snapPointEdgeTo, snapPointEdgeFrom, item);
        var disposable = Subscribe(spai);
        _CompositeDisposable.Add(disposable);
        return disposable;
    }

    public static void Disconnect(DesignerItemViewModelBase designerItem)
    {
        if (designerItem.SnapObjs != null) designerItem.SnapObjs.Dispose();
    }

    public static void Disconnect(SnapPointViewModel snapPointViewModel)
    {
        if (snapPointViewModel.SnapObjs != null)
            snapPointViewModel.SnapObjs.ForEach(x => x.Dispose());
    }

    private void ExecuteSelectItemCommand(object param)
    {
        SelectItem((bool)param, !IsSelected.Value);
    }

    private void SelectItem(bool newselect, bool select)
    {
        if (newselect)
            foreach (var designerItemViewModelBase in Owner.SelectedItems.Value.ToList())
                Owner.MainWindowVM.Recorder.Current.ExecuteSetProperty(designerItemViewModelBase, "IsSelected.Value",
                    false);

        Owner.MainWindowVM.Recorder.Current.ExecuteSetProperty(this, "IsSelected.Value", select);
    }

    public bool IsSameGroup(SelectableDesignerItemViewModelBase target)
    {
        return ParentID == target.ParentID && ParentID != Guid.Empty;
    }

    private void Init()
    {
        SelectItemCommand = new DelegateCommand<object>(p => SelectItem((bool)p, !IsSelected.Value));

        EnableForSelection.Value = true;

        RotationAngle.Subscribe(angle =>
            {
                if (angle > 360) RotationAngle.Value = angle % 360;
            })
            .AddTo(_CompositeDisposable);
        PenLineJoins = new ReactiveCollection<PenLineJoin>
        {
            PenLineJoin.Miter,
            PenLineJoin.Bevel,
            PenLineJoin.Round
        };
        StrokeDashArray.Value = new DoubleCollection();
    }

    public abstract Type GetViewType();

    public abstract void OpenPropertyDialog();

    public void Swap(SelectableDesignerItemViewModelBase other)
    {
        if (GetType() != other.GetType())
            throw new InvalidOperationException("GetType() != other.GetType()");
        SwapInternal_SwapProperties(this, other);
        SwapInternal_SwapFields(this, other);
    }

    private static void SwapInternal_SwapFields<T>(T left, T right)
    {
        var fieldInfos = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var fieldInfo in fieldInfos)
        {
            var temp = fieldInfo.GetValue(left);
            fieldInfo.SetValue(left, fieldInfo.GetValue(right));
            fieldInfo.SetValue(right, temp);
        }
    }

    private static void SwapInternal_SwapProperties<T>(T left, T right)
    {
        var propertyInfos = typeof(T).GetProperties(BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var propertyInfo in propertyInfos)
        {
            var temp = propertyInfo.GetValue(left);
            propertyInfo.SetValue(left, propertyInfo.GetValue(right));
            propertyInfo.SetValue(right, temp);
        }
    }

    public string ShowPropertiesAndFields()
    {
        var ret = $"<{GetType().Name}>{{";

        var properties = GetType().GetProperties(
            BindingFlags.Public
            | BindingFlags.Instance);

        foreach (var property in properties.Except(new[]
                 {
                     GetType().GetProperty("Parent"),
                     GetType().GetProperty("SelectedItems")
                 }))
            ret += $"{property.Name}={property.GetValue(this)},";

        var fields = GetType().GetFields(
            BindingFlags.Public
            | BindingFlags.Instance);

        foreach (var field in fields) ret += $"{field.Name}={field.GetValue(this)},";
        ret = ret.Remove(ret.Length - 1, 1);
        ret += "}";
        return ret;
    }

    public override string ToString()
    {
        return ShowPropertiesAndFields();
    }

    public virtual IDisposable BeginMonitor(Action action)
    {
        var compositeDisposable = new CompositeDisposable();
        return compositeDisposable;
    }

    #region IObserver<GroupTransformNotification>

    public abstract void OnNext(GroupTransformNotification value);

    public void OnError(Exception error)
    {
        throw new NotSupportedException();
    }

    public void OnCompleted()
    {
        throw new NotSupportedException();
    }

    #endregion //IObserver<GroupTransformNotification>

    #region IObservable<TransformNotification>

    protected List<IObserver<TransformNotification>> _observers = new();

    public IDisposable Subscribe(IObserver<TransformNotification> observer)
    {
        _observers.Add(observer);
        observer.OnNext(new TransformNotification
        {
            Sender = this
        });
        return new SelectableDesignerItemViewModelBaseDisposable(this, observer);
    }

    public virtual void OpenInstructionDialog()
    {
    }

    public class SelectableDesignerItemViewModelBaseDisposable : IDisposable
    {
        private readonly SelectableDesignerItemViewModelBase _obj;
        private readonly IObserver<TransformNotification> _observer;

        public SelectableDesignerItemViewModelBaseDisposable(SelectableDesignerItemViewModelBase obj,
            IObserver<TransformNotification> observer)
        {
            _obj = obj;
            _observer = observer;
        }

        public void Dispose()
        {
            _obj._observers.Remove(_observer);
        }
    }

    #endregion //IObservable<TransformNotification>
}