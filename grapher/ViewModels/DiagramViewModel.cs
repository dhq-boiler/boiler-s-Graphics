using grapher.Messenger;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace grapher.ViewModels
{
    public class DiagramViewModel : BindableBase, IDiagramViewModel
    {
        private ObservableCollection<SelectableDesignerItemViewModelBase> _items = new ObservableCollection<SelectableDesignerItemViewModelBase>();
        private Point _CurrentPoint;
        private ObservableCollection<Color> _EdgeColors = new ObservableCollection<Color>();
        private ObservableCollection<Color> _FillColors = new ObservableCollection<Color>();

        public DiagramViewModel()
        {
            AddItemCommand = new DelegateCommand<object>(p => ExecuteAddItemCommand(p));
            RemoveItemCommand = new DelegateCommand<object>(p => ExecuteRemoveItemCommand(p));
            ClearSelectedItemsCommand = new DelegateCommand<object>(p => ExecuteClearSelectedItemsCommand(p));
            CreateNewDiagramCommand = new DelegateCommand<object>(p => ExecuteCreateNewDiagramCommand(p));

            EdgeColors.CollectionChangedAsObservable()
                .Subscribe(_ => RaisePropertyChanged("EdgeColors"));
            FillColors.CollectionChangedAsObservable()
                .Subscribe(_ => RaisePropertyChanged("FillColors"));

            EdgeColors.Add(Colors.Black);
            FillColors.Add(Colors.Transparent);

            Mediator.Instance.Register(this);
        }

        [MediatorMessageSink("DoneDrawingMessage")]
        public void OnDoneDrawingMessage(bool dummy)
        {
            foreach (var item in Items.OfType<DesignerItemViewModelBase>())
            {
                item.ShowConnectors = false;
            }
        }

        public DelegateCommand<object> AddItemCommand { get; private set; }
        public DelegateCommand<object> RemoveItemCommand { get; private set; }
        public DelegateCommand<object> ClearSelectedItemsCommand { get; private set; }
        public DelegateCommand<object> CreateNewDiagramCommand { get; private set; }

        public ObservableCollection<SelectableDesignerItemViewModelBase> Items
        {
            get { return _items; }
        }

        public List<SelectableDesignerItemViewModelBase> SelectedItems
        {
            get { return Items.Where(x => x.IsSelected).ToList(); }
        }

        public ObservableCollection<Color> EdgeColors
        {
            get { return _EdgeColors; }
            set { SetProperty(ref _EdgeColors, value); }
        }

        public ObservableCollection<Color> FillColors
        {
            get { return _FillColors; }
            set { SetProperty(ref _FillColors, value); }
        }

        private void ExecuteAddItemCommand(object parameter)
        {
            if (parameter is SelectableDesignerItemViewModelBase)
            {
                SelectableDesignerItemViewModelBase item = (SelectableDesignerItemViewModelBase)parameter;
                item.Owner = this;
                _items.Add(item);
            }
        }

        private void ExecuteRemoveItemCommand(object parameter)
        {
            if (parameter is SelectableDesignerItemViewModelBase)
            {
                SelectableDesignerItemViewModelBase item = (SelectableDesignerItemViewModelBase)parameter;
                _items.Remove(item);
            }
        }

        private void ExecuteClearSelectedItemsCommand(object parameter)
        {
            foreach (SelectableDesignerItemViewModelBase item in Items)
            {
                item.IsSelected = false;
            }
        }

        private void ExecuteCreateNewDiagramCommand(object parameter)
        {
            Items.Clear();
        }

        public Point CurrentPoint
        {
            get { return _CurrentPoint; }
            set { SetProperty(ref _CurrentPoint, value); }
        }
    }
}
