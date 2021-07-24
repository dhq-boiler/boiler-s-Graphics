using boilersGraphics.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
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
        private bool _IsSelected;
        protected CompositeDisposable _CompositeDisposable = new CompositeDisposable();

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

        public ReactiveCollection<SelectableDesignerItemViewModelBase> SelectedItems
        {
            get { return Owner.SelectedItems; }
        }

        public IDiagramViewModel Owner { get; set; }
        public DelegateCommand<object> SelectItemCommand { get; private set; }
        public int Id { get; set; }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set { SetProperty(ref _IsSelected, value); }
        }

        public ReactiveProperty<Matrix> Matrix { get; } = new ReactiveProperty<Matrix>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);

        public ReactiveProperty<double> RotationAngle { get; } = new ReactiveProperty<double>(mode: ReactivePropertyMode.RaiseLatestValueOnSubscribe);

        public ReactiveProperty<bool> EnableForSelection { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<int> ZIndex { get; } = new ReactiveProperty<int>();

        public ReactiveProperty<Color> EdgeColor { get; } = new ReactiveProperty<Color>();

        public ReactiveProperty<double> EdgeThickness { get; } = new ReactiveProperty<double>();

        public ReactiveProperty<PathGeometry> PathGeometry { get; set; } = new ReactiveProperty<PathGeometry>();

        public ReactiveProperty<PathGeometry> RotatePathGeometry { get; set; } = new ReactiveProperty<PathGeometry>();

        public ReactiveProperty<bool> EnablePathGeometryUpdate { get; set; } = new ReactiveProperty<bool>();

        public ReactivePropertySlim<bool> IsVisible { get; } = new ReactivePropertySlim<bool>();

        public string Name { get; set; }

        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid ParentID { get; set; }

        public IDisposable GroupDisposable { get; internal set; }

        private void ExecuteSelectItemCommand(object param)
        {
            SelectItem((bool)param, !IsSelected);
        }

        private void SelectItem(bool newselect, bool select)
        {
            if (newselect)
            {
                foreach (var designerItemViewModelBase in Owner.SelectedItems.ToList())
                {
                    designerItemViewModelBase.IsSelected = false;
                }
            }

            IsSelected = select;
        }

        public bool IsSameGroup(SelectableDesignerItemViewModelBase target)
        {
            return this.ParentID == target.ParentID && this.ParentID != Guid.Empty;
        }

        private void Init()
        {
            SelectItemCommand = new DelegateCommand<object>(p => SelectItem((bool)p, !IsSelected));

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
