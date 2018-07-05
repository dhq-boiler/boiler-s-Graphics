using grapher.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Linq;
using System.Windows.Media;

namespace grapher.ViewModels
{
    public interface ISelectItems
    {
        DelegateCommand<object> SelectItemCommand { get; }
    }


    public abstract class SelectableDesignerItemViewModelBase : BindableBase, ISelectItems, IObserver<GroupTransformNotification>
    {
        private bool _IsSelected;

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

        public ReactiveProperty<Matrix> Matrix { get; } = new ReactiveProperty<Matrix>();

        public ReactiveProperty<double> RotationAngle { get; } = new ReactiveProperty<double>();

        public ReactiveProperty<bool> EnableForSelection { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<int> ZIndex { get; } = new ReactiveProperty<int>();

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

        private void Init()
        {
            SelectItemCommand = new DelegateCommand<object>(p => SelectItem((bool)p, !IsSelected));

            EnableForSelection.Value = true;
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
    }
}
