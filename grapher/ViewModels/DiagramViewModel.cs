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
            BringForwardCommand = new DelegateCommand(() => ExecuteBringForwardCommand(), () => CanExecuteOrder());
            SendBackwardCommand = new DelegateCommand(() => ExecuteSendBackwardCommand(), () => CanExecuteOrder());
            BringForegroundCommand = new DelegateCommand(() => ExecuteBringForegroundCommand(), () => CanExecuteOrder());
            SendBackgroundCommand = new DelegateCommand(() => ExecuteSendBackgroundCommand(), () => CanExecuteOrder());

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
                    BringForwardCommand.RaiseCanExecuteChanged();
                    SendBackwardCommand.RaiseCanExecuteChanged();
                    BringForegroundCommand.RaiseCanExecuteChanged();
                    SendBackgroundCommand.RaiseCanExecuteChanged();
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
        public DelegateCommand BringForegroundCommand { get; private set; }
        public DelegateCommand BringForwardCommand { get; private set; }
        public DelegateCommand SendBackwardCommand { get; private set; }
        public DelegateCommand SendBackgroundCommand { get; private set; }

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
                item.EnableForSelection.Value = false;
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
                    child.EnableForSelection.Value = true;
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

        private void ExecuteBringForwardCommand()
        {
            var ordered = from item in SelectedItems
                          orderby item.ZIndex.Value descending
                          select item;

            int count = Items.Count;

            for (int i = 0; i < ordered.Count(); ++i)
            {
                int currentIndex = ordered.ElementAt(i).ZIndex.Value;
                int newIndex = Math.Min(count - 1 - i, currentIndex + 1);
                if (currentIndex != newIndex)
                {
                    ordered.ElementAt(i).ZIndex.Value = newIndex;
                    var exists = Items.Where(item => item.ZIndex.Value == newIndex);

                    foreach (var item in exists)
                    {
                        if (item != ordered.ElementAt(i))
                        {
                            item.ZIndex.Value = currentIndex;
                            break;
                        }
                    }
                }
            }
        }

        private void ExecuteSendBackwardCommand()
        {
            var ordered = from item in SelectedItems
                          orderby item.ZIndex.Value ascending
                          select item;

            int count = Items.Count;

            for (int i = 0; i < ordered.Count(); ++i)
            {
                int currentIndex = ordered.ElementAt(i).ZIndex.Value;
                int newIndex = Math.Max(i, currentIndex - 1);
                if (currentIndex != newIndex)
                {
                    ordered.ElementAt(i).ZIndex.Value = newIndex;
                    var exists = Items.Where(item => item.ZIndex.Value == newIndex);

                    foreach (var item in exists)
                    {
                        if (item != ordered.ElementAt(i))
                        {
                            item.ZIndex.Value = currentIndex;
                            break;
                        }
                    }
                }
            }
        }

        private void ExecuteBringForegroundCommand()
        {
            var ordered = from item in SelectedItems
                          orderby item.ZIndex.Value descending
                          select item;

            int count = Items.Count;

            for (int i = 0; i < ordered.Count(); ++i)
            {
                int currentIndex = ordered.ElementAt(i).ZIndex.Value;
                int newIndex = Items.Count - 1;
                if (currentIndex != newIndex)
                {
                    ordered.ElementAt(i).ZIndex.Value = newIndex;
                    var exists = Items.Where(item => item.ZIndex.Value <= newIndex);

                    foreach (var item in exists)
                    {
                        if (item != ordered.ElementAt(i))
                        {
                            item.ZIndex.Value -= 1;
                        }
                    }
                }
            }
        }

        private void ExecuteSendBackgroundCommand()
        {
            var ordered = from item in SelectedItems
                          orderby item.ZIndex.Value ascending
                          select item;

            int count = Items.Count;

            for (int i = 0; i < ordered.Count(); ++i)
            {
                int currentIndex = ordered.ElementAt(i).ZIndex.Value;
                int newIndex = 0;
                if (currentIndex != newIndex)
                {
                    ordered.ElementAt(i).ZIndex.Value = newIndex;
                    var exists = Items.Where(item => item.ZIndex.Value >= newIndex);

                    foreach (var item in exists)
                    {
                        if (item != ordered.ElementAt(i))
                        {
                            item.ZIndex.Value += 1;
                        }
                    }
                }
            }
        }

        private bool CanExecuteOrder()
        {
            return SelectedItems.Count() > 0;
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
                    var centerPoint = designerItem.CenterPoint.Value;
                    var angleInDegrees = designerItem.RotationAngle.Value;

                    var p1 = new Point(designerItem.Left.Value, designerItem.Top.Value);
                    var p2 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value);
                    var p3 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value + designerItem.Height.Value);
                    var p4 = new Point(designerItem.Left.Value, designerItem.Top.Value + designerItem.Height.Value);

                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + 135, p1);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + 45, p2);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees - 45, p3);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees - 135, p4);
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

        private static void UpdateBoundary(ref double x1, ref double y1, ref double x2, ref double y2, Point centerPoint, double angleInDegrees, Point point)
        {
            var rad = angleInDegrees * Math.PI / 180;

            var t = RotatePoint(centerPoint, point, rad);

            x1 = Math.Min(t.Item1, x1);
            y1 = Math.Min(t.Item2, y1);
            x2 = Math.Max(t.Item1, x2);
            y2 = Math.Max(t.Item2, y2);
        }

        private static Tuple<double, double> RotatePoint(Point center, Point point, double rad)
        {
            var z1 = point.X - center.X;
            var z2 = point.Y - center.Y;
            var x = center.X + Math.Sqrt(Math.Pow(z1, 2) + Math.Pow(z2, 2)) * Math.Cos(rad);
            var y = center.Y + Math.Sqrt(Math.Pow(z1, 2) + Math.Pow(z2, 2)) * Math.Sin(rad);

            return new Tuple<double, double>(x, y);
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
