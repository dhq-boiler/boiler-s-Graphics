using boilersGraphics.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Media;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.ViewModels
{
    public interface ISelectItems
    {
        DelegateCommand<object> SelectItemCommand { get; }
    }


    public abstract class SelectableDesignerItemViewModelBase : BindableBase, ISelectItems, IObserver<TransformNotification>, IObserver<GroupTransformNotification>, IObservable<TransformNotification>, IDisposable, ICloneable, IRestore
    {
        protected CompositeDisposable _CompositeDisposable = new CompositeDisposable();

        public static int SelectedOrderCount { get; set; } = 0;

        public SelectableDesignerItemViewModelBase(int id, IDiagramViewModel parent)
        {
            this.Id = id;
            this.Owner = parent;
            Init();
        }

        public SelectableDesignerItemViewModelBase()
        {
            Init();
        }

        public SelectableDesignerItemViewModelBase[] SelectedItems
        {
            get { return Owner.SelectedItems.Value; }
        }

        public IDiagramViewModel Owner { get; set; }
        public DelegateCommand<object> SelectItemCommand { get; private set; }
        public int Id { get; set; }

        public ReactivePropertySlim<bool> IsSelected { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<int> SelectedOrder { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<Matrix> Matrix { get; } = new ReactivePropertySlim<Matrix>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<double> RotationAngle { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<bool> EnableForSelection { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<int> ZIndex { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<Color> EdgeColor { get; } = new ReactivePropertySlim<Color>();

        public ReactivePropertySlim<double> EdgeThickness { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<PathGeometry> PathGeometry { get; set; } = new ReactivePropertySlim<PathGeometry>();

        public ReactivePropertySlim<bool> EnablePathGeometryUpdate { get; set; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> IsVisible { get; } = new ReactivePropertySlim<bool>();

        
        public ReactivePropertySlim<bool> IsHitTestVisible { get; set; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> CanDrag { get; set; } = new ReactivePropertySlim<bool>(true);

        public ReactivePropertySlim<Color> FillColor { get; } = new ReactivePropertySlim<Color>();

        public ReactivePropertySlim<PenLineJoin> PenLineJoin { get; } = new ReactivePropertySlim<PenLineJoin>();

        public ReactiveCollection<PenLineJoin> PenLineJoins { get; private set; }

        public string Name { get; set; }

        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid ParentID { get; set; }

        public IDisposable GroupDisposable { get; set; }


        public IDisposable Connect(SnapPointPosition snapPointEdgeTo, SnapPointPosition snapPointEdgeFrom, SelectableDesignerItemViewModelBase item)
        {
            var spai = new SnapPointAdsorptionInformation(snapPointEdgeTo, snapPointEdgeFrom, item);
            var disposable = Subscribe(spai);
            _CompositeDisposable.Add(disposable);
            return disposable;
        }

        public static void Disconnect(DesignerItemViewModelBase designerItem)
        {
            if (designerItem.SnapObjs != null)
                designerItem.SnapObjs.ForEach(x => x.Dispose());
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
            {
                foreach (var designerItemViewModelBase in Owner.SelectedItems.Value.ToList())
                {
                    Owner.MainWindowVM.Recorder.Current.ExecuteSetProperty(designerItemViewModelBase, "IsSelected.Value", false);
                }
            }

            Owner.MainWindowVM.Recorder.Current.ExecuteSetProperty(this, "IsSelected.Value", select);
        }

        public bool IsSameGroup(SelectableDesignerItemViewModelBase target)
        {
            return this.ParentID == target.ParentID && this.ParentID != Guid.Empty;
        }

        private void Init()
        {
            SelectItemCommand = new DelegateCommand<object>(p => SelectItem((bool)p, !IsSelected.Value));

            EnableForSelection.Value = true;

            RotationAngle.Subscribe(angle =>
            {
                if (angle > 360)
                {
                    RotationAngle.Value = angle % 360;
                }
            })
            .AddTo(_CompositeDisposable);
            PenLineJoins = new ReactiveCollection<PenLineJoin>();
            PenLineJoins.Add(System.Windows.Media.PenLineJoin.Miter);
            PenLineJoins.Add(System.Windows.Media.PenLineJoin.Bevel);
            PenLineJoins.Add(System.Windows.Media.PenLineJoin.Round);
        }

        public abstract Type GetViewType();

        public abstract void OpenPropertyDialog();

        public abstract bool SupportsPropertyDialog { get; }

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

        #region IDisposable

        public void Dispose()
        {
            if (_CompositeDisposable != null)
            {
                _CompositeDisposable.Dispose();
            }
            _CompositeDisposable = null;
        }

        #endregion //IDisposable

        #region IClonable

        public abstract object Clone();

        #endregion //IClonable

        #region IObserver<TransformNotification>

        public void OnNext(TransformNotification value)
        {
        }

        #endregion IObserver<TransformNotification>

        #region IObservable<TransformNotification>

        protected List<IObserver<TransformNotification>> _observers = new List<IObserver<TransformNotification>>();

        public IDisposable Subscribe(IObserver<TransformNotification> observer)
        {
            _observers.Add(observer);
            observer.OnNext(new TransformNotification()
            {
                Sender = this
            });
            return new SelectableDesignerItemViewModelBaseDisposable(this, observer);
        }

        public class SelectableDesignerItemViewModelBaseDisposable : IDisposable
        {
            private SelectableDesignerItemViewModelBase _obj;
            private IObserver<TransformNotification> _observer;
            public SelectableDesignerItemViewModelBaseDisposable(SelectableDesignerItemViewModelBase obj, IObserver<TransformNotification> observer)
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

        public void Restore(Action restorePropertiesAction)
        {
            restorePropertiesAction.Invoke();
        }

        public void Swap(SelectableDesignerItemViewModelBase other)
        {
            if (GetType() != other.GetType())
                throw new InvalidOperationException("GetType() != other.GetType()");
            SwapInternal_SwapProperties(this, other);
            SwapInternal_SwapFields(this, other);
        }

        private static void SwapInternal_SwapFields<T>(T left, T right)
        {
            var fieldInfos = typeof(T).GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

            foreach (var fieldInfo in fieldInfos)
            {
                var temp = fieldInfo.GetValue(left);
                fieldInfo.SetValue(left, fieldInfo.GetValue(right));
                fieldInfo.SetValue(right, temp);
            }
        }

        private static void SwapInternal_SwapProperties<T>(T left, T right)
        {
            var propertyInfos = typeof(T).GetProperties(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

            foreach (var propertyInfo in propertyInfos)
            {
                var temp = propertyInfo.GetValue(left);
                propertyInfo.SetValue(left, propertyInfo.GetValue(right));
                propertyInfo.SetValue(right, temp);
            }
        }

        public string ShowPropertiesAndFields()
        {
            string ret = $"<{GetType().Name}>{{";

            PropertyInfo[] properties = GetType().GetProperties(
                BindingFlags.Public
                | BindingFlags.Instance);

            foreach (var property in properties)
            {
                ret += $"{property.Name}={property.GetValue(this)},";
            }

            FieldInfo[] fields = GetType().GetFields(
                BindingFlags.Public
                | BindingFlags.Instance);

            foreach (var field in fields)
            {
                ret += $"{field.Name}={field.GetValue(this)},";
            }
            ret = ret.Remove(ret.Length - 1, 1);
            ret += $"}}";
            return ret;
        }
    }
}
