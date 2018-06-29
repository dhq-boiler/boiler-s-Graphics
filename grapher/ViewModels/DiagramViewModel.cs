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
            GroupCommand = new DelegateCommand(() => ExecuteGroupItemsCommand(), () => CanExecuteGroup());
            UngroupCommand = new DelegateCommand(() => ExecuteUngroupItemsCommand(), () => CanExecuteUngroup());

            SelectedItems = Items
                .ObserveElementProperty(x => x.IsSelected)
                .Where(x => x.Instance.IsSelected)
                .Select(x => x.Instance)
                .ToReactiveCollection();
            SelectedItems.CollectionChangedAsObservable()
                .Subscribe(_ =>
                {
                    GroupCommand.RaiseCanExecuteChanged();
                    UngroupCommand.RaiseCanExecuteChanged();
                });

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
        public DelegateCommand GroupCommand { get; private set; }
        public DelegateCommand UngroupCommand { get; private set; }

        public ObservableCollection<SelectableDesignerItemViewModelBase> Items
        {
            get { return _items; }
        }

        public ReactiveCollection<SelectableDesignerItemViewModelBase> SelectedItems { get; }

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

        private void ExecuteGroupItemsCommand()
        {
            var items = from item in SelectedItems
                        where item.ParentID == Guid.Empty
                        select item;

            var rect = GetBoundingRectangle(items);

            var groupItem = new GroupItemViewModel();
            groupItem.Width.Value = rect.Width;
            groupItem.Height.Value = rect.Height;
            groupItem.Left.Value = rect.Left;
            groupItem.Top.Value = rect.Top;

            AddItemCommand.Execute(groupItem);

            foreach (var item in items)
            {
                item.GroupDisposable = groupItem.Subscribe(item);
                item.ParentID = groupItem.ID;
            }

            groupItem.SelectItemCommand.Execute(true);
        }

        private bool CanExecuteGroup()
        {
            var items = from item in SelectedItems
                        where item.ParentID == Guid.Empty
                        select item;
            return items.Count() > 1;
        }

        private void ExecuteUngroupItemsCommand()
        {
            var groups = from item in SelectedItems
                         where item.ParentID == Guid.Empty
                         select item;

            foreach (var groupRoot in groups.ToList())
            {
                var children = from child in Items
                               where child.ParentID == groupRoot.ID
                               select child;

                foreach (var child in children)
                {
                    child.GroupDisposable.Dispose();
                    child.ParentID = Guid.Empty;
                }

                SelectedItems.Remove(groupRoot);
                Items.Remove(groupRoot);
                //UpdateZIndex();
            }
        }

        private bool CanExecuteUngroup()
        {
            var items = from item in SelectedItems.OfType<GroupItemViewModel>()
                        select item;
            return items.Count() > 0;
        }

        private static Rect GetBoundingRectangle(IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            double x1 = Double.MaxValue;
            double y1 = Double.MaxValue;
            double x2 = Double.MinValue;
            double y2 = Double.MinValue;

            foreach (var item in items)
            {
                if (item is DesignerItemViewModelBase designerItem)
                {
                    x1 = Math.Min(designerItem.Left.Value, x1);
                    y1 = Math.Min(designerItem.Top.Value, y1);

                    x2 = Math.Max(designerItem.Left.Value + designerItem.Width.Value, x2);
                    y2 = Math.Max(designerItem.Top.Value + designerItem.Height.Value, y2);
                }
                else if (item is ConnectorBaseViewModel connector)
                {
                    x1 = Math.Min(Math.Min(connector.SourceA.X, connector.SourceB.X), x1);
                    y1 = Math.Min(Math.Min(connector.SourceA.Y, connector.SourceB.Y), y1);

                    x2 = Math.Max(Math.Max(connector.SourceA.X, connector.SourceB.X), x2);
                    y2 = Math.Max(Math.Max(connector.SourceA.Y, connector.SourceB.Y), y2);
                }
            }

            return new Rect(new Point(x1, y1), new Point(x2, y2));
        }

        /// <summary>
        /// 現在ポインティングしている座標
        /// ステータスバー上の座標インジケーターに使用される
        /// </summary>
        public Point CurrentPoint
        {
            get { return _CurrentPoint; }
            set { SetProperty(ref _CurrentPoint, value); }
        }
    }
}
