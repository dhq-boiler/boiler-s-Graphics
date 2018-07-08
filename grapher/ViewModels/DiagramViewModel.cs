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
        public DelegateCommand AlignTopCommand { get; private set; }
        public DelegateCommand AlignVerticalCenterCommand { get; private set; }
        public DelegateCommand AlignBottomCommand { get; private set; }
        public DelegateCommand AlignLeftCommand { get; private set; }
        public DelegateCommand AlignHorizontalCenterCommand { get; private set; }
        public DelegateCommand AlignRightCommand { get; private set; }
        public DelegateCommand DistributeHorizontalCommand { get; private set; }

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
            AlignTopCommand = new DelegateCommand(() => ExecuteAlignTopCommand(), () => CanExecuteAlign());
            AlignVerticalCenterCommand = new DelegateCommand(() => ExecuteAlignVerticalCenterCommand(), () => CanExecuteAlign());
            AlignBottomCommand = new DelegateCommand(() => ExecuteAlignBottomCommand(), () => CanExecuteAlign());
            AlignLeftCommand = new DelegateCommand(() => ExecuteAlignLeftCommand(), () => CanExecuteAlign());
            AlignHorizontalCenterCommand = new DelegateCommand(() => ExecuteAlignHorizontalCenterCommand(), () => CanExecuteAlign());
            AlignRightCommand = new DelegateCommand(() => ExecuteAlignRightCommand(), () => CanExecuteAlign());
            DistributeHorizontalCommand = new DelegateCommand(() => ExecuteDistributeHorizontalCommand(), () => CanExecuteDistribute());

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

                    AlignTopCommand.RaiseCanExecuteChanged();
                    AlignVerticalCenterCommand.RaiseCanExecuteChanged();
                    AlignBottomCommand.RaiseCanExecuteChanged();
                    AlignLeftCommand.RaiseCanExecuteChanged();
                    AlignHorizontalCenterCommand.RaiseCanExecuteChanged();
                    AlignRightCommand.RaiseCanExecuteChanged();
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

        public void DeselectAll()
        {
            foreach (var item in Items)
            {
                item.IsSelected = false;
            }
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

        #region Grouping

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

            var groupItems = from it in Items
                             where it.ParentID == groupItem.ID
                             select it;

            var theMostForwardItem = (from it in groupItems
                                      orderby it.ZIndex.Value descending
                                      select it).Take(1).SingleOrDefault();

            var sortList = (from it in Items
                            where it.ZIndex.Value < theMostForwardItem.ZIndex.Value && it.ID != groupItem.ID
                            orderby it.ZIndex.Value descending
                            select it).ToList();

            var swapItems = (from it in groupItems
                             orderby it.ZIndex.Value descending
                             select it).Skip(1);

            for (int i = 0; i < swapItems.Count(); ++i)
            {
                var it = swapItems.ElementAt(i);
                it.ZIndex.Value = theMostForwardItem.ZIndex.Value - i - 1;
            }

            var swapItemsCount = swapItems.Count();

            for (int i = 0, j = 0; i < sortList.Count(); ++i)
            {
                var it = sortList.ElementAt(i);
                if (it.ParentID == groupItem.ID)
                {
                    j++;
                    continue;
                }
                it.ZIndex.Value = theMostForwardItem.ZIndex.Value - swapItemsCount - (i - j) - 1;
            }

            groupItem.ZIndex.Value = theMostForwardItem.ZIndex.Value + 1;

            var adds = from item in Items
                       where item.ID != groupItem.ID && item.ZIndex.Value >= groupItem.ZIndex.Value
                       select item;

            foreach (var add in adds)
            {
                add.ZIndex.Value += 1;
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

                var groupZIndex = groupRoot.ZIndex.Value;

                var it = from item in Items
                         where item.ZIndex.Value > groupZIndex
                         select item;

                foreach (var x in it)
                {
                    x.ZIndex.Value -= 1;
                }
            }
        }

        private bool CanExecuteUngroup()
        {
            var items = from item in SelectedItems.OfType<GroupItemViewModel>()
                        select item;
            return items.Count() > 0;
        }

        #endregion //Grouping

        #region Ordering

        private void ExecuteBringForwardCommand()
        {
            var ordered = from item in SelectedItems
                          orderby item.ZIndex.Value descending
                          select item;

            int count = Items.Count;

            for (int i = 0; i < ordered.Count(); ++i)
            {
                int currentIndex = ordered.ElementAt(i).ZIndex.Value;
                var next = (from x in Items
                            where x.ZIndex.Value == currentIndex + 1
                            select x).SingleOrDefault();

                if (next == null) continue;

                int newIndex = next.ParentID != Guid.Empty ? Items.Single(x => x.ID == next.ParentID).ZIndex.Value : Math.Min(count - 1 - i, currentIndex + 1);
                if (currentIndex != newIndex)
                {
                    if (ordered.ElementAt(i) is GroupItemViewModel)
                    {
                        ordered.ElementAt(i).ZIndex.Value = newIndex;

                        var children = from item in Items
                                       where item.ParentID == ordered.ElementAt(i).ID
                                       orderby item.ZIndex.Value descending
                                       select item;

                        int youngestChildrenZIndex = 0;

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            youngestChildrenZIndex = child.ZIndex.Value = newIndex - j - 1;
                        }

                        var younger = from item in Items
                                      where item.ID != ordered.ElementAt(i).ID && item.ParentID != ordered.ElementAt(i).ID
                                      && item.ZIndex.Value <= ordered.ElementAt(i).ZIndex.Value && item.ZIndex.Value >= youngestChildrenZIndex
                                      select item;

                        var x = from item in Items
                                where item.ID != ordered.ElementAt(i).ID && item.ParentID != ordered.ElementAt(i).ID
                                && item.ZIndex.Value < youngestChildrenZIndex
                                select item;

                        var z = x.ToList();
                        z.AddRange(younger);

                        for (int j = 0; j < z.Count(); ++j)
                        {
                            z.ElementAt(j).ZIndex.Value = j;
                        }
                    }
                    else
                    {
                        ordered.ElementAt(i).ZIndex.Value = newIndex;
                        var exists = Items.Where(item => item.ZIndex.Value == newIndex);

                        foreach (var item in exists)
                        {
                            if (item != ordered.ElementAt(i))
                            {
                                if (item is GroupItemViewModel)
                                {
                                    var children = from it in Items
                                                   where it.ParentID == item.ID
                                                   select it;

                                    foreach (var child in children)
                                    {
                                        child.ZIndex.Value -= 1;
                                    }

                                    item.ZIndex.Value = currentIndex + children.Count();
                                }
                                else
                                {
                                    item.ZIndex.Value = currentIndex;
                                }
                                break;
                            }
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
                var previous = (from x in Items
                                where x.ZIndex.Value == currentIndex - 1
                                select x).SingleOrDefault();

                if (previous == null) continue;

                int newIndex = previous is GroupItemViewModel ? Items.Where(x => x.ParentID == previous.ID).Min(x => x.ZIndex.Value) : Math.Max(i, currentIndex - 1);
                if (currentIndex != newIndex)
                {
                    if (ordered.ElementAt(i) is GroupItemViewModel)
                    {
                        var children = (from item in Items
                                        where item.ParentID == ordered.ElementAt(i).ID
                                        orderby item.ZIndex.Value descending
                                        select item).ToList();

                        if (children.Any(c => c.ZIndex.Value == 0)) continue;

                        ordered.ElementAt(i).ZIndex.Value = newIndex;

                        int youngestChildrenZIndex = 0;

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            youngestChildrenZIndex = child.ZIndex.Value = newIndex - j - 1;
                        }

                        var older = from item in Items
                                    where item.ID != ordered.ElementAt(i).ID && item.ParentID != ordered.ElementAt(i).ID
                                    && item.ZIndex.Value <= ordered.ElementAt(i).ZIndex.Value && item.ZIndex.Value >= youngestChildrenZIndex
                                    select item;

                        var x = from item in Items
                                where item.ID != ordered.ElementAt(i).ID && item.ParentID != ordered.ElementAt(i).ID
                                && item.ZIndex.Value > ordered.ElementAt(i).ZIndex.Value
                                select item;

                        var z = older.ToList();
                        z.AddRange(x);
                        z.Reverse();

                        for (int j = 0; j < z.Count(); ++j)
                        {
                            var elm = z.ElementAt(j);
                            elm.ZIndex.Value = Items.Count() - j - 1;
                        }
                    }
                    else
                    {
                        ordered.ElementAt(i).ZIndex.Value = newIndex;
                        var exists = Items.Where(item => item.ZIndex.Value == newIndex);

                        foreach (var item in exists)
                        {
                            if (item != ordered.ElementAt(i))
                            {
                                if (item.ParentID != Guid.Empty)
                                {
                                    var children = from it in Items
                                                   where it.ParentID == item.ParentID
                                                   select it;

                                    foreach (var child in children)
                                    {
                                        child.ZIndex.Value += 1;
                                    }

                                    var parent = (from it in Items
                                                  where it.ID == item.ParentID
                                                  select it).Single();

                                    parent.ZIndex.Value = children.Max(x => x.ZIndex.Value) + 1;
                                }
                                else
                                {
                                    item.ZIndex.Value = currentIndex;
                                }
                                break;
                            }
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
                var current = ordered.ElementAt(i);
                int currentIndex = current.ZIndex.Value;
                int newIndex = Items.Count - 1;
                if (currentIndex != newIndex)
                {
                    var oldCurrentIndex = current.ZIndex.Value;
                    current.ZIndex.Value = newIndex;

                    if (current is GroupItemViewModel)
                    {
                        var children = from item in Items
                                       where item.ParentID == current.ID
                                       orderby item.ZIndex.Value descending
                                       select item;

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            child.ZIndex.Value = current.ZIndex.Value - j - 1;
                        }

                        var minValue = children.Min(x => x.ZIndex.Value);

                        var other = (from item in Items
                                     where item.ParentID != current.ID && item.ID != current.ID
                                     orderby item.ZIndex.Value descending
                                     select item).ToList();

                        for (int j = 0; j < other.Count(); ++j)
                        {
                            var item = other.ElementAt(j);
                            item.ZIndex.Value = minValue - j - 1;
                        }
                    }
                    else
                    {
                        var exists = Items.Where(item => item.ZIndex.Value <= newIndex && item.ZIndex.Value > oldCurrentIndex);

                        foreach (var item in exists)
                        {
                            if (item != current)
                            {
                                item.ZIndex.Value -= 1;
                            }
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
                var current = ordered.ElementAt(i);
                int currentIndex = current.ZIndex.Value;
                int newIndex = current is GroupItemViewModel ? Items.Where(x => x.ParentID == current.ID).Count() : 0;
                if (currentIndex != newIndex)
                {
                    var oldCurrentIndex = current.ZIndex.Value;
                    current.ZIndex.Value = newIndex;

                    if (current is GroupItemViewModel)
                    {
                        var children = (from item in Items
                                        where item.ParentID == current.ID
                                        orderby item.ZIndex.Value descending
                                        select item).ToList();

                        for (int j = 0; j < children.Count(); ++j)
                        {
                            var child = children.ElementAt(j);
                            child.ZIndex.Value = current.ZIndex.Value - j - 1;
                        }

                        var other = (from item in Items
                                     where item.ParentID != current.ID && item.ID != current.ID
                                     orderby item.ZIndex.Value descending
                                     select item).ToList();

                        var maxValue = Items.Count() - 1;

                        for (int j = 0; j < other.Count(); ++j)
                        {
                            var item = other.ElementAt(j);
                            item.ZIndex.Value = maxValue - j;
                        }
                    }
                    else
                    {
                        var exists = Items.Where(item => item.ZIndex.Value >= newIndex && item.ZIndex.Value < oldCurrentIndex);

                        foreach (var item in exists)
                        {
                            if (item != current)
                            {
                                item.ZIndex.Value += 1;
                            }
                        }
                    }
                }
            }
        }

        private bool CanExecuteOrder()
        {
            return SelectedItems.Count() > 0;
        }

        #endregion //Ordering

        #region Alignment

        private void ExecuteAlignTopCommand()
        {
            if (SelectedItems.Count() > 1)
            {
                var first = SelectedItems.First();
                double top = GetTop(first);

                foreach (var item in SelectedItems)
                {
                    double delta = top - GetTop(item);
                    SetTop(item, GetTop(item) + delta);
                }
            }
        }

        private void ExecuteAlignVerticalCenterCommand()
        {
            if (SelectedItems.Count() > 1)
            {
                var first = SelectedItems.First();
                double bottom = GetTop(first) + GetHeight(first) / 2;

                foreach (var item in SelectedItems)
                {
                    double delta = bottom - (GetTop(item) + GetHeight(item) / 2);
                    SetTop(item, GetTop(item) + delta);
                }
            }
        }

        private void ExecuteAlignBottomCommand()
        {
            if (SelectedItems.Count() > 1)
            {
                var first = SelectedItems.First();
                double bottom = GetTop(first) + GetHeight(first);

                foreach (var item in SelectedItems)
                {
                    double delta = bottom - (GetTop(item) + GetHeight(item));
                    SetTop(item, GetTop(item) + delta);
                }
            }
        }

        private void ExecuteAlignLeftCommand()
        {
            if (SelectedItems.Count() > 1)
            {
                var first = SelectedItems.First();
                double left = GetLeft(first);

                foreach (var item in SelectedItems)
                {
                    double delta = left - GetLeft(item);
                    SetLeft(item, GetLeft(item) + delta);
                }
            }
        }

        private void ExecuteAlignHorizontalCenterCommand()
        {
            if (SelectedItems.Count() > 1)
            {
                var first = SelectedItems.First();
                double center = GetLeft(first) + GetWidth(first) / 2;

                foreach (var item in SelectedItems)
                {
                    double delta = center - (GetLeft(item) + GetWidth(item) / 2);
                    SetLeft(item, GetLeft(item) + delta);
                }
            }
        }

        private void ExecuteAlignRightCommand()
        {
            if (SelectedItems.Count() > 1)
            {
                var first = SelectedItems.First();
                double right = GetLeft(first) + GetWidth(first);

                foreach (var item in SelectedItems)
                {
                    double delta = right - (GetLeft(item) + GetWidth(item));
                    SetLeft(item, GetLeft(item) + delta);
                }
            }
        }

        private void ExecuteDistributeHorizontalCommand()
        {
            var selectedItems = from item in SelectedItems
                                let itemLeft = GetLeft(item)
                                orderby itemLeft
                                select item;

            if (selectedItems.Count() > 1)
            {
                double left = double.MaxValue;
                double right = double.MinValue;
                double sumWidth = 0;

                foreach (var item in selectedItems)
                {
                    left = Math.Min(left, GetLeft(item));
                    right = Math.Max(right, GetLeft(item) + GetWidth(item));
                    sumWidth += GetWidth(item);
                }

                double distance = Math.Max(0, (right - left - sumWidth) / (selectedItems.Count() - 1));
                double offset = GetLeft(selectedItems.First());

                foreach (var item in selectedItems)
                {
                    double delta = offset - GetLeft(item);
                    SetLeft(item, GetLeft(item) + delta);
                    offset = offset + GetWidth(item) + distance;
                }
            }
        }

        private bool CanExecuteAlign()
        {
            return SelectedItems.Count() > 1;
        }

        private bool CanExecuteDistribute()
        {
            return SelectedItems.Count() > 1;
        }

        private double GetWidth(SelectableDesignerItemViewModelBase item)
        {
            return item is DesignerItemViewModelBase ? (item as DesignerItemViewModelBase).Width.Value
                 : item is ConnectorBaseViewModel ? Math.Max((item as ConnectorBaseViewModel).SourceA.X - (item as ConnectorBaseViewModel).SourceB.X, (item as ConnectorBaseViewModel).SourceB.X - (item as ConnectorBaseViewModel).SourceA.X)
                 : (item as GroupItemViewModel).Width.Value;
        }

        private void SetLeft(SelectableDesignerItemViewModelBase item, double value)
        {
            if (item is DesignerItemViewModelBase di)
            {
                di.Left.Value = value;
            }
            else if (item is ConnectorBaseViewModel connector)
            {
                //do nothing
            }
        }

        private double GetLeft(SelectableDesignerItemViewModelBase item)
        {
            return item is DesignerItemViewModelBase ? (item as DesignerItemViewModelBase).Left.Value
                : item is ConnectorBaseViewModel ? Math.Min((item as ConnectorBaseViewModel).SourceA.X, (item as ConnectorBaseViewModel).SourceB.X)
                : Items.Where(x => x.ParentID == (item as GroupItemViewModel).ID).Min(x => GetLeft(x));
        }

        private double GetHeight(SelectableDesignerItemViewModelBase item)
        {
            return item is DesignerItemViewModelBase ? (item as DesignerItemViewModelBase).Height.Value
                 : item is ConnectorBaseViewModel ? Math.Max((item as ConnectorBaseViewModel).SourceA.Y - (item as ConnectorBaseViewModel).SourceB.Y, (item as ConnectorBaseViewModel).SourceB.Y - (item as ConnectorBaseViewModel).SourceA.Y)
                 : (item as GroupItemViewModel).Height.Value;
        }

        private void SetTop(SelectableDesignerItemViewModelBase item, double value)
        {
            if (item is DesignerItemViewModelBase di)
            {
                di.Top.Value = value;
            }
            else if (item is ConnectorBaseViewModel connector)
            {
                //do nothing
            }
        }

        private double GetTop(SelectableDesignerItemViewModelBase item)
        {
            return item is DesignerItemViewModelBase ? (item as DesignerItemViewModelBase).Top.Value
                : item is ConnectorBaseViewModel ? Math.Min((item as ConnectorBaseViewModel).SourceA.Y, (item as ConnectorBaseViewModel).SourceB.Y)
                : Items.Where(x => x.ParentID == (item as GroupItemViewModel).ID).Min(x => GetTop(x));
        }

        #endregion //Alignment

        private IEnumerable<SelectableDesignerItemViewModelBase> GetGroupMembers(SelectableDesignerItemViewModelBase item)
        {
            var list = new List<SelectableDesignerItemViewModelBase>();
            list.Add(item);
            var children = Items.Where(x => x.ParentID == item.ID);
            list.AddRange(children);
            return list;
        }

        public static Rect GetBoundingRectangle(IEnumerable<SelectableDesignerItemViewModelBase> items)
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

                    var p0 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value + designerItem.Height.Value / 2);
                    var p1 = new Point(designerItem.Left.Value, designerItem.Top.Value);
                    var p2 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value);
                    var p3 = new Point(designerItem.Left.Value + designerItem.Width.Value, designerItem.Top.Value + designerItem.Height.Value);
                    var p4 = new Point(designerItem.Left.Value, designerItem.Top.Value + designerItem.Height.Value);

                    var vector_p0_center = p0 - centerPoint;
                    var vector_p1_center = p1 - centerPoint;
                    var vector_p2_center = p2 - centerPoint;
                    var vector_p3_center = p3 - centerPoint;
                    var vector_p4_center = p4 - centerPoint;

                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p1_center), p1);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p2_center), p2);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p3_center), p3);
                    UpdateBoundary(ref x1, ref y1, ref x2, ref y2, centerPoint, angleInDegrees + Vector.AngleBetween(vector_p0_center, vector_p4_center), p4);
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
