using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using Cysharp.Text;
using ObservableCollections;
using Prism.Commands;
using Prism.Mvvm;
using R3;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using TsOperationHistory;
using TsOperationHistory.Extensions;
using ZLinq;

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
        HalfEdgeThickness = EdgeThickness.Select(x => x / 2).ToReadOnlyBindableReactiveProperty();
        Init();
    }

    public SelectableDesignerItemViewModelBase()
    {
        HalfEdgeThickness = EdgeThickness.Select(x => x / 2).ToReadOnlyBindableReactiveProperty();
        Init();
    }

    public static int SelectedOrderCount { get; set; } = 0;

    public SelectableDesignerItemViewModelBase[] SelectedItems => Owner.SelectedItems.Value;

    public IDiagramViewModel Owner { get; set; }
    public int Id { get; set; }

    // ↓ Flags ↓

    public R3.BindableReactiveProperty<bool> IsSelected { get; } = new();
    public R3.BindableReactiveProperty<bool> EnableForSelection { get; } = new();
    public R3.BindableReactiveProperty<PathGeometryUpdatingStrategy> UpdatingStrategy { get; } = new();
    public enum PathGeometryUpdatingStrategy
    {
        Unknown,
        Initial,
        ResizeWhilePreservingOriginalShape,
        Fixed,
    }
    public R3.BindableReactiveProperty<bool> IsVisible { get; } = new();
    public R3.BindableReactiveProperty<bool> IsHitTestVisible { get; set; } = new();
    public R3.BindableReactiveProperty<bool> CanDrag { get; set; } = new(true);

    // ↑ Flags ↑

    public R3.BindableReactiveProperty<object> ChangeFormTriggerObject { get; } = new();
    public R3.BindableReactiveProperty<DateTime> ChangeFormDateTime { get; } = new(DateTime.Now);
    public R3.BindableReactiveProperty<LayerItem> LayerItem { get; } = new();

    public R3.BindableReactiveProperty<int> SelectedOrder { get; } = new();

    public R3.BindableReactiveProperty<Matrix> Matrix { get; } = new();

    public R3.BindableReactiveProperty<double> RotationAngle { get; } = new();

    public R3.BindableReactiveProperty<int> ZIndex { get; } = new();
    public R3.BindableReactiveProperty<Brush> EdgeBrush { get; } = new(Brushes.Transparent);
    public R3.BindableReactiveProperty<Brush> FillBrush { get; } = new(Brushes.Transparent);

    public R3.BindableReactiveProperty<double> EdgeThickness { get; } = new();

    public R3.IReadOnlyBindableReactiveProperty<double> HalfEdgeThickness { get; }

    public R3.IReadOnlyBindableReactiveProperty<PathGeometry> PathGeometry { get; set; }
    public R3.BindableReactiveProperty<PathGeometry> PathGeometryNoRotate { get; } = new();
    public R3.BindableReactiveProperty<PathGeometry> PathGeometryRotate { get; } = new();
    public R3.BindableReactiveProperty<PenLineJoin> StrokeLineJoin { get; } = new();
    public ObservableList<PenLineJoin> PenLineJoins { get; private set; }
    public R3.BindableReactiveProperty<DoubleCollection> StrokeDashArray { get; } = new();
    public R3.BindableReactiveProperty<double> StrokeMiterLimit { get; } = new();

    public string Name { get; set; }

    public Guid ID { get; set; } = Guid.NewGuid();

    public Guid ParentID { get; set; }

    public IDisposable GroupDisposable { get; set; }

    public R3.ReadOnlyReactiveProperty<double> SnapPointSize { get; } = Observable.Return(4)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactiveProperty();

    public R3.ReadOnlyReactiveProperty<double> ThumbSize { get; } = Observable.Return(7)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactiveProperty();

    public R3.ReadOnlyReactiveProperty<double> ThumbThickness { get; } = Observable.Return(1)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactiveProperty();

    public R3.ReadOnlyReactiveProperty<Thickness> RotateThumbMargin { get; } = Observable.Return(-20)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return new Thickness(0, standardSize / (rate / 100d), 0, 0); })
        .ToReadOnlyReactiveProperty();

    public R3.ReadOnlyReactiveProperty<Thickness> RotateThumbConnectorMargin { get; } = Observable.Return(-11)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return new Thickness(0, standardSize / (rate / 100d), 0, 0); })
        .ToReadOnlyReactiveProperty();

    public R3.ReadOnlyReactiveProperty<double> RotateThumbConnectorY2 { get; } = Observable.Return(11)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactiveProperty();

    public R3.ReadOnlyReactiveProperty<double> RotateThumbConnectorThickness { get; } = Observable.Return(1)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) => { return standardSize / (rate / 100d); }).ToReadOnlyReactiveProperty();

    public R3.ReadOnlyReactiveProperty<Thickness> RotateThumbGridMargin { get; } = Observable.Return(-3)
        .CombineLatest(DiagramViewModel.Instance.MagnificationRate,
            (standardSize, rate) =>
            {
                return new Thickness(standardSize / (rate / 100d), standardSize / (rate / 100d),
                    standardSize / (rate / 100d), standardSize / (rate / 100d));
            }).ToReadOnlyReactiveProperty();

    public abstract bool SupportsPropertyDialog { get; }

    public R3.BindableReactiveProperty<bool> IsOpenedInstructionDialog { get; } = new();

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
            foreach (var designerItemViewModelBase in Owner.SelectedItems.Value.AsValueEnumerable().ToList())
                Owner.MainWindowVM.Recorder.Current.ExecuteSetProperty(designerItemViewModelBase, "IsSelected.Value",
                    false);

        Owner.MainWindowVM.Recorder.Current.ExecuteSetProperty(this, "IsSelected.Value", select);
    }

    public bool IsSameGroup(SelectableDesignerItemViewModelBase target)
    {
        return ParentID == target.ParentID && ParentID != Guid.Empty;
    }

    public SelectableDesignerItemViewModelBase GetParent()
    {
        if (ParentID == Guid.Empty)
        {
            return null;
        }

        return Owner.AllItems.Value.AsValueEnumerable().FirstOrDefault(x => x.ID == ParentID);
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
        PenLineJoins = new ObservableList<PenLineJoin>
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

    private static readonly ConcurrentDictionary<Type, (PropertyInfo[] properties, FieldInfo[] fields)> _reflectionCache = new();
    private static readonly HashSet<string> _excludedPropertyNames = new()
    {
        "Parent",
        "SelectedItems", 
        "LayerItem"
    };

    public string ShowPropertiesAndFields()
    {
        var type = GetType();
        var (properties, fields) = _reflectionCache.GetOrAdd(type, GetPropertiesAndFields);

        using (var sb = ZString.CreateStringBuilder())
        {
            sb.Append('<');
            sb.Append(type.Name);
            sb.Append(">");
#if DEBUG
            sb.Append('{');
            // プロパティの処理
            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                if (_excludedPropertyNames.Contains(property.Name))
                    continue;

                try
                {
                    var value = property.GetValue(this);
                    sb.Append(property.Name);
                    sb.Append('=');
                    sb.Append(value?.ToString() ?? "null");
                    sb.Append(',');
                }
                catch (Exception)
                {
                    sb.Append(property.Name);
                    sb.Append("=<error>,");
                }
            }

            // フィールドの処理
            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                try
                {
                    var value = field.GetValue(this);
                    sb.Append(field.Name);
                    sb.Append('=');
                    sb.Append(value?.ToString() ?? "null");
                    sb.Append(',');
                }
                catch (Exception)
                {
                    sb.Append(field.Name);
                    sb.Append("=<error>,");
                }
            }

            // 最後のカンマを削除
            if (sb.Length > 0 && sb.ToString()[sb.Length - 1] == ',')
                sb.Remove(sb.Length - 1, 1);

            sb.Append('}');
#endif
            return sb.ToString();
        }
    }

    private static (PropertyInfo[] properties, FieldInfo[] fields) GetPropertiesAndFields(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        return (properties, fields);
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