using boilersGraphics.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public interface ISelectItems
    {
        DelegateCommand<object> SelectItemCommand { get; }
    }


    public abstract class SelectableDesignerItemViewModelBase : BindableBase, ISelectItems, IObserver<GroupTransformNotification>, IDisposable, ICloneable
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
        public ReactivePropertySlim<bool> IsDragging { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<int> SelectedOrder { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<Matrix> Matrix { get; } = new ReactivePropertySlim<Matrix>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<double> RotationAngle { get; } = new ReactivePropertySlim<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe | ReactivePropertyMode.DistinctUntilChanged);

        public ReactivePropertySlim<bool> EnableForSelection { get; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<int> ZIndex { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<Color> EdgeColor { get; } = new ReactivePropertySlim<Color>();

        public ReactivePropertySlim<double> EdgeThickness { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<PathGeometry> PathGeometry { get; set; } = new ReactivePropertySlim<PathGeometry>();

        public ReactivePropertySlim<PathGeometry> RotatePathGeometry { get; set; } = new ReactivePropertySlim<PathGeometry>();

        public ReactivePropertySlim<bool> EnablePathGeometryUpdate { get; set; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<bool> IsVisible { get; } = new ReactivePropertySlim<bool>();

        
        public ReactivePropertySlim<bool> IsHitTestVisible { get; set; } = new ReactivePropertySlim<bool>();

        public ReactivePropertySlim<Color> FillColor { get; } = new ReactivePropertySlim<Color>();

        public string Name { get; set; }

        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid ParentID { get; set; }

        public IDisposable GroupDisposable { get; internal set; }

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
                    designerItemViewModelBase.IsSelected.Value = false;
                }
            }

            IsSelected.Value = select;
        }

        public bool IsSameGroup(SelectableDesignerItemViewModelBase target)
        {
            return this.ParentID == target.ParentID && this.ParentID != Guid.Empty;
        }

        private void Init()
        {
            SelectItemCommand = new DelegateCommand<object>(p => SelectItem((bool)p, !IsSelected.Value));

            EnableForSelection.Value = true;
        }

        public abstract Type GetViewType();

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
    }
}
