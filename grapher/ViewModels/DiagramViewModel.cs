using grapher.Controls;
using grapher.Extensions;
using grapher.Messenger;
using grapher.UserControls;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace grapher.ViewModels
{
    public class DiagramViewModel : BindableBase, IDiagramViewModel, IDisposable
    {
        private ObservableCollection<SelectableDesignerItemViewModelBase> _items = new ObservableCollection<SelectableDesignerItemViewModelBase>();
        private Point _CurrentPoint;
        private ObservableCollection<Color> _EdgeColors = new ObservableCollection<Color>();
        private ObservableCollection<Color> _FillColors = new ObservableCollection<Color>();
        private CompositeDisposable _CompositeDisposable = new CompositeDisposable();
        private int _Width;
        private int _Height;
        private double _BorderThickness;
        private bool _MiddleButtonIsPressed;
        private Point _MousePointerPosition;

        public DelegateCommand<object> AddItemCommand { get; private set; }
        public DelegateCommand<object> RemoveItemCommand { get; private set; }
        public DelegateCommand<object> ClearSelectedItemsCommand { get; private set; }
        public DelegateCommand<object> CreateNewDiagramCommand { get; private set; }
        public DelegateCommand LoadCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand ExportCommand { get; private set; }
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
        public DelegateCommand DistributeVerticalCommand { get; private set; }
        public DelegateCommand SelectAllCommand { get; private set; }
        public DelegateCommand UniformWidthCommand { get; private set; }
        public DelegateCommand UniformHeightCommand { get; private set; }
        public DelegateCommand DuplicateCommand { get; private set; }
        public DelegateCommand<MouseWheelEventArgs> MouseWheelCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> PreviewMouseDownCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> PreviewMouseUpCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseLeaveCommand { get; private set; }
        public DelegateCommand<MouseEventArgs> MouseEnterCommand { get; private set; }

        public double ScaleX { get; set; } = 1.0;
        public double ScaleY { get; set; } = 1.0;

        public double BorderThickness
        {
            get { return _BorderThickness; }
            set { SetProperty(ref _BorderThickness, value); }
        }

        public DiagramViewModel()
        {
            AddItemCommand = new DelegateCommand<object>(p => ExecuteAddItemCommand(p));
            RemoveItemCommand = new DelegateCommand<object>(p => ExecuteRemoveItemCommand(p));
            ClearSelectedItemsCommand = new DelegateCommand<object>(p => ExecuteClearSelectedItemsCommand(p));
            CreateNewDiagramCommand = new DelegateCommand<object>(p => ExecuteCreateNewDiagramCommand(p));
            LoadCommand = new DelegateCommand(() => ExecuteLoadCommand());
            SaveCommand = new DelegateCommand(() => ExecuteSaveCommand());
            ExportCommand = new DelegateCommand(() => ExecuteExportCommand());
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
            DistributeVerticalCommand = new DelegateCommand(() => ExecuteDistributeVerticalCommand(), () => CanExecuteDistribute());
            SelectAllCommand = new DelegateCommand(() => ExecuteSelectAllCommand());
            UniformWidthCommand = new DelegateCommand(() => ExecuteUniformWidthCommand(), () => CanExecuteUniform());
            UniformHeightCommand = new DelegateCommand(() => ExecuteUniformHeightCommand(), () => CanExecuteUniform());
            DuplicateCommand = new DelegateCommand(() => ExecuteDuplicateCommand(), () => CanExecuteDuplicate());
            MouseWheelCommand = new DelegateCommand<MouseWheelEventArgs>(args =>
            {
                var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                var scrollViewer = diagramControl.GetChildOfType<ScrollViewer>();
                var dockpanel = scrollViewer.GetChildOfType<DockPanel>();
                var matrix = (dockpanel.LayoutTransform as MatrixTransform).Matrix;
                ScaleX += args.Delta / 1000d;
                ScaleY += args.Delta / 1000d;
                
                matrix.Scale(ScaleX, ScaleY);
                dockpanel.RenderTransform = new MatrixTransform(matrix);
                args.Handled = true;
            });
            PreviewMouseDownCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                if (args.MiddleButton == MouseButtonState.Pressed)
                {
                    _MiddleButtonIsPressed = true;
                    var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                    _MousePointerPosition = args.GetPosition(diagramControl);
                    diagramControl.Cursor = Cursors.SizeAll;
                }
            });
            PreviewMouseUpCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                ReleaseMiddleButton(args);
            });
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                if (_MiddleButtonIsPressed)
                {
                    var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                    var scrollViewer = diagramControl.GetChildOfType<ScrollViewer>();
                    var newMousePointerPosition = args.GetPosition(diagramControl);
                    var diff = newMousePointerPosition - _MousePointerPosition;
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - diff.Y);
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - diff.X);
                    _MousePointerPosition = newMousePointerPosition;
                }
            });
            MouseLeaveCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                if (_MiddleButtonIsPressed)
                {
                    ReleaseMiddleButton(args);
                }
            });
            MouseEnterCommand = new DelegateCommand<MouseEventArgs>(args =>
            {
                if (_MiddleButtonIsPressed)
                {
                    ReleaseMiddleButton(args);
                }
            });

            Items
                .ObserveElementProperty(x => x.IsSelected)
                .Subscribe(x =>
                {
                    if (x.Value)
                    {
                        SelectedItems.Add(x.Instance);
                    }
                    else
                    {
                        SelectedItems.Remove(x.Instance);
                    }
                })
                .AddTo(_CompositeDisposable);
            Items
                .ObserveRemoveChangedItems()
                .Merge(Items.ObserveReplaceChangedItems().Select(x => x.OldItem))
                .Subscribe(xs =>
                {
                    foreach (var x in xs)
                    {
                        if (x.IsSelected) { SelectedItems.Remove(x); }
                    }
                })
                .AddTo(_CompositeDisposable);
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
                    DistributeHorizontalCommand.RaiseCanExecuteChanged();
                    DistributeVerticalCommand.RaiseCanExecuteChanged();

                    UniformWidthCommand.RaiseCanExecuteChanged();
                    UniformHeightCommand.RaiseCanExecuteChanged();
                })
                .AddTo(_CompositeDisposable);

            EdgeColors.CollectionChangedAsObservable()
                .Subscribe(_ => RaisePropertyChanged("EdgeColors"))
                .AddTo(_CompositeDisposable);
            FillColors.CollectionChangedAsObservable()
                .Subscribe(_ => RaisePropertyChanged("FillColors"))
                .AddTo(_CompositeDisposable);

            EdgeColors.Add(Colors.Black);
            FillColors.Add(Colors.Transparent);

            BorderThickness = 1.0;
        }

        private void ReleaseMiddleButton(MouseEventArgs args)
        {
            if (args.MiddleButton == MouseButtonState.Released)
            {
                _MiddleButtonIsPressed = false;
                var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
                diagramControl.Cursor = Cursors.Arrow;
            }
        }

        public DiagramViewModel(int width, int height)
            : this()
        {
            Width = width;
            Height = height;

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

        public ReactiveCollection<SelectableDesignerItemViewModelBase> SelectedItems { get; } = new ReactiveCollection<SelectableDesignerItemViewModelBase>();

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

        public int Width
        {
            get { return _Width; }
            set { SetProperty(ref _Width, value); }
        }

        public int Height
        {
            get { return _Height; }
            set { SetProperty(ref _Height, value); }
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
                RemoveGroupMembers(item);
                _items.Remove(item);
                if (item is LetterDesignerItemViewModel)
                {
                    (item as LetterDesignerItemViewModel).CloseLetterSettingDialog();
                }
                item.Dispose();
                UpdateZIndex();
            }
        }

        private void UpdateZIndex()
        {
            var items = (from item in Items
                         orderby item.ZIndex.Value ascending
                         select item).ToList();

            for (int i = 0; i < items.Count; ++i)
            {
                items.ElementAt(i).ZIndex.Value = i;
            }
        }

        private void RemoveGroupMembers(SelectableDesignerItemViewModelBase item)
        {
            if (item is GroupItemViewModel groupItem)
            {
                var children = (from it in Items
                                where it.ParentID == groupItem.ID
                                select it).ToList();

                foreach (var child in children)
                {
                    RemoveGroupMembers(child);
                    Items.Remove(child);
                    child.Dispose();
                }
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

        private void ExecuteExportCommand()
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(App.Current.MainWindow);
            var diagramControl = App.Current.MainWindow.GetChildOfType<DiagramControl>();
            var itemsControl = diagramControl.GetChildOfType<ItemsControl>();
            var tempIsSelected = new Dictionary<SelectableDesignerItemViewModelBase, bool>();
            foreach (var item in itemsControl.Items.Cast<SelectableDesignerItemViewModelBase>())
            {
                tempIsSelected.Add(item, item.IsSelected);
                item.IsSelected = false;
            }
            var rtb = new RenderTargetBitmap((int)itemsControl.ActualWidth, (int)itemsControl.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(itemsControl);

            //IsSelectedの復元
            foreach (var item in itemsControl.Items.Cast<SelectableDesignerItemViewModelBase>())
            {
                bool outIsSelected;
                if (tempIsSelected.TryGetValue(item, out outIsSelected))
                {
                    item.IsSelected = outIsSelected;
                }
            }

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.QualityLevel = 100;
            using (var stream = File.Create("TEST.jpg"))
            {
                encoder.Save(stream);
            }
        }

        #region Save

        private void ExecuteSaveCommand()
        {
            var designerItems = this.Items.OfType<DesignerItemViewModelBase>();
            var connections = this.Items.OfType<ConnectorBaseViewModel>();

            XElement designerItemsXML = SerializeDesignerItems(designerItems);
            XElement connectionsXML = SerializeConnections(connections);

            XElement root = new XElement("Grapher");
            root.Add(designerItemsXML);
            root.Add(connectionsXML);

            SaveFile(root);
        }

        private XElement SerializeDesignerItems(IEnumerable<DesignerItemViewModelBase> designerItems)
        {
            XElement serializedItems = null;
            serializedItems = new XElement("DesignerItems",
                                       (from item in designerItems
                                       where item.GetType() != typeof(PictureDesignerItemViewModel)
                                       select new XElement("DesignerItem",
                                                  new XElement("ID", item.ID),
                                                  new XElement("ParentID", item.ParentID),
                                                  new XElement("Type", item.GetType().FullName),
                                                  new XElement("Left", item.Left.Value),
                                                  new XElement("Top", item.Top.Value),
                                                  new XElement("Width", item.Width.Value),
                                                  new XElement("Height", item.Height.Value),
                                                  new XElement("ZIndex", item.ZIndex.Value),
                                                  new XElement("Matrix", item.Matrix.Value),
                                                  new XElement("EdgeColor", item.EdgeColor),
                                                  new XElement("FillColor", item.FillColor)
                                              ))
                                       .Union(
                                           from item in designerItems
                                           where item.GetType() == typeof(PictureDesignerItemViewModel)
                                           select new XElement("DesignerItem",
                                                  new XElement("ID", item.ID),
                                                  new XElement("ParentID", item.ParentID),
                                                  new XElement("Type", item.GetType().FullName),
                                                  new XElement("Left", item.Left.Value),
                                                  new XElement("Top", item.Top.Value),
                                                  new XElement("Width", item.Width.Value),
                                                  new XElement("Height", item.Height.Value),
                                                  new XElement("ZIndex", item.ZIndex.Value),
                                                  new XElement("Matrix", item.Matrix.Value),
                                                  new XElement("EdgeColor", item.EdgeColor),
                                                  new XElement("FillColor", item.FillColor),
                                                  new XElement("FileName", (item as PictureDesignerItemViewModel).FileName)
                                            )
                                       )
                                       
                                   );

            return serializedItems;
        }

        private XElement SerializeConnections(IEnumerable<ConnectorBaseViewModel> connections)
        {
            var serializedConnections = new XElement("Connections",
                           from connection in connections
                           select new XElement("Connection",
                                      new XElement("ID", connection.ID),
                                      new XElement("ParentID", connection.ParentID),
                                      new XElement("Type", connection.GetType().FullName),
                                      new XElement("SourceID", connection.SourceConnectedDataItemID),
                                      new XElement("SinkID", connection.SinkConnectedDataItemID),
                                      new XElement("SourceA", connection.SourceA),
                                      new XElement("SourceB", connection.SourceB),
                                      new XElement("SourceOrientation", connection.SourceConnectorInfo.Orientation),
                                      new XElement("SinkOrientation", connection.SinkConnectorInfo.Orientation),
                                      new XElement("SourceDegree", GetDegreeOrNan(connection.SourceConnectorInfo)),
                                      new XElement("SinkDegree", GetDegreeOrNan(connection.SinkConnectorInfo)),
                                      new XElement("ZIndex", connection.ZIndex.Value),
                                      new XElement("EdgeColor", connection.EdgeColor)
                                     )
                                  );

            return serializedConnections;
        }

        private double GetDegreeOrNan(ConnectorInfoBase connectorInfo)
        {
            return connectorInfo is FullyCreatedConnectorInfo fully? fully.Degree: double.NaN;
        }

        private void SaveFile(XElement xElement)
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "Files (*.xml)|*.xml|All Files (*.*)|*.*";
            if (saveFile.ShowDialog() == true)
            {
                try
                {
                    xElement.Save(saveFile.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion //Save

        #region Load

        private void ExecuteLoadCommand()
        {
            var root = LoadSerializedDataFromFile();

            if (root == null)
            {
                return;
            }

            Items.Clear();

            var tempItems = new List<SelectableDesignerItemViewModelBase>();

            var designerItemXMLs = root.Elements("DesignerItems").Elements("DesignerItem");
            foreach (var designerItemXML in designerItemXMLs)
            {
                var item = (DesignerItemViewModelBase)DeserializeInstance(designerItemXML);
                item.Left.Value = double.Parse(designerItemXML.Element("Left").Value);
                item.Top.Value = double.Parse(designerItemXML.Element("Top").Value);
                item.Width.Value = double.Parse(designerItemXML.Element("Width").Value);
                item.Height.Value = double.Parse(designerItemXML.Element("Height").Value);
                item.ID = Guid.Parse(designerItemXML.Element("ID").Value);
                item.ParentID = Guid.Parse(designerItemXML.Element("ParentID").Value);
                item.ZIndex.Value = Int32.Parse(designerItemXML.Element("ZIndex").Value);
                item.Matrix.Value = new Matrix();
                item.EdgeColor = (Color)ColorConverter.ConvertFromString(designerItemXML.Element("EdgeColor").Value);
                item.FillColor = (Color)ColorConverter.ConvertFromString(designerItemXML.Element("FillColor").Value);
                item.Owner = this;
                if (item is PictureDesignerItemViewModel)
                {
                    var picture = item as PictureDesignerItemViewModel;
                    picture.FileName = designerItemXML.Element("FileName").Value;
                }
                tempItems.Add(item);
            }

            //connector
            var connectorXmls = root.Elements("Connections").Elements("Connection");
            foreach (var connectorXml in connectorXmls)
            {
                var item = (ConnectorBaseViewModel)DeserializeInstance(connectorXml);
                item.ID = Guid.Parse(connectorXml.Element("ID").Value);
                item.ParentID = Guid.Parse(connectorXml.Element("ParentID").Value);
                item.SourceA = Point.Parse(connectorXml.Element("SourceA").Value);
                item.SourceB = Point.Parse(connectorXml.Element("SourceB").Value);
                item.SourceConnectorInfo = DeserializeConnectorInfo(connectorXml, "SourceID", "SourceOrientation", "SourceDegree", item.SourceA, tempItems);
                item.SinkConnectorInfo = DeserializeConnectorInfo(connectorXml, "SinkID", "SinkOrientation", "SinkDegree", item.SourceB, tempItems);
                item.ZIndex.Value = Int32.Parse(connectorXml.Element("ZIndex").Value);
                item.EdgeColor = (Color)ColorConverter.ConvertFromString(connectorXml.Element("EdgeColor").Value);
                item.Owner = this;
                tempItems.Add(item);
            }

            //grouping
            foreach (var groupItem in tempItems.OfType<GroupItemViewModel>().ToList())
            {
                var children = from item in tempItems
                               where item.ParentID == groupItem.ID
                               select item;

                children.ToList().ForEach(x => groupItem.AddGroup(x));
            }

            Items.AddRange(tempItems.OrderBy(x => x.ZIndex.Value));
        }

        private ConnectorInfoBase DeserializeConnectorInfo(XElement connectorXml, string anyIdElementName, string anyOrientationElementName, string anyDegreeElementName, Point current, List<SelectableDesignerItemViewModelBase> tempItems)
        {
            var sourceId = Guid.Parse(connectorXml.Element(anyIdElementName).Value);
            return (sourceId != Guid.Empty ? new FullyCreatedConnectorInfo((DesignerItemViewModelBase)tempItems.Single(x => x.ID == sourceId), (ConnectorOrientation)Enum.Parse(typeof(ConnectorOrientation), connectorXml.Element(anyOrientationElementName).Value), double.Parse(connectorXml.Element(anyDegreeElementName).Value))
                    : (ConnectorInfoBase)new PartCreatedConnectionInfo(current));
        }

        private SelectableDesignerItemViewModelBase DeserializeInstance(XElement designerItemXML)
        {
            var className = designerItemXML.Element("Type").Value;
            return (SelectableDesignerItemViewModelBase)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetName().Name, className).Unwrap();
        }

        private XElement LoadSerializedDataFromFile()
        {
            var openFile = new OpenFileDialog();
            openFile.Filter = "Designer Files (*.xml)|*.xml|All Files (*.*)|*.*";

            if (openFile.ShowDialog() == true)
            {
                try
                {
                    return XElement.Load(openFile.FileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace, e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return null;
        }

        #endregion //Load

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
                groupItem.AddGroup(item);
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

                groupRoot.Dispose();

                Items.Remove(groupRoot);

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

        private void ExecuteDistributeVerticalCommand()
        {
            var selectedItems = from item in SelectedItems
                                let itemTop = GetTop(item)
                                orderby itemTop
                                select item;

            if (selectedItems.Count() > 1)
            {
                double top = double.MaxValue;
                double bottom = double.MinValue;
                double sumHeight = 0;

                foreach (var item in selectedItems)
                {
                    top = Math.Min(top, GetTop(item));
                    bottom = Math.Max(bottom, GetTop(item) + GetHeight(item));
                    sumHeight += GetHeight(item);
                }

                double distance = Math.Max(0, (bottom - top - sumHeight) / (selectedItems.Count() - 1));
                double offset = GetTop(selectedItems.First());

                foreach (var item in selectedItems)
                {
                    double delta = offset - GetTop(item);
                    SetTop(item, GetTop(item) + delta);
                    offset = offset + GetHeight(item) + distance;
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

        private void ExecuteSelectAllCommand()
        {
            Items.ToList().ForEach(x => x.IsSelected = true);
        }

        #region Uniform

        private void ExecuteUniformWidthCommand()
        {
            var selectedItems = SelectedItems.OfType<DesignerItemViewModelBase>();
            if (selectedItems.Count() > 1)
            {
                var first = selectedItems.First();
                double width = first.Width.Value;

                foreach (var item in selectedItems)
                {
                    double delta = width - item.Width.Value;
                    item.Width.Value += delta;
                }
            }
        }

        private void ExecuteUniformHeightCommand()
        {
            var selectedItems = SelectedItems.OfType<DesignerItemViewModelBase>();
            if (selectedItems.Count() > 1)
            {
                var first = selectedItems.First();
                double height = first.Height.Value;

                foreach (var item in selectedItems)
                {
                    double delta = height - item.Height.Value;
                    item.Height.Value += delta;
                }
            }
        }

        private bool CanExecuteUniform()
        {
            return SelectedItems.OfType<DesignerItemViewModelBase>().Count() > 1;
        }

        #endregion //Uniform

        #region Duplicate

        private void ExecuteDuplicateCommand()
        {
            DuplicateObjects(SelectedItems);
        }

        private void DuplicateObjects(IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            var selectedItems = from item in items.OfType<DesignerItemViewModelBase>()
                                orderby item.ZIndex.Value ascending
                                select item;

            var oldNewList = new List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>>();

            foreach (var item in selectedItems)
            {
                DuplicateDesignerItem(selectedItems, oldNewList, item);
            }

            var connectedConnectors = from item in items.OfType<ConnectorBaseViewModel>()
                                      where item.SourceConnectorInfo is FullyCreatedConnectorInfo || item.SinkConnectorInfo is FullyCreatedConnectorInfo
                                      select item;

            var connectedTargets = (from item in connectedConnectors
                                    where item.SourceConnectorInfo is FullyCreatedConnectorInfo
                                    select (item.SourceConnectorInfo as FullyCreatedConnectorInfo).DataItem)
                                   .Union(
                                    from item in connectedConnectors
                                    where item.SinkConnectorInfo is FullyCreatedConnectorInfo
                                    select (item.SinkConnectorInfo as FullyCreatedConnectorInfo).DataItem);

            var connectedItems = from item in items.OfType<DesignerItemViewModelBase>()
                                 where connectedTargets.Contains(item)
                                 select item;

            var selectedConnectors = from item in items.OfType<ConnectorBaseViewModel>()
                                     orderby item.ZIndex.Value ascending
                                     select item;

            foreach (var connector in selectedConnectors)
            {
                DuplicateConnector(connectedItems, oldNewList, connector);
            }
        }

        private void DuplicateDesignerItem(IOrderedEnumerable<DesignerItemViewModelBase> selectedItems, List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>> oldNewList, DesignerItemViewModelBase item, GroupItemViewModel parent = null)
        {
            if (item is GroupItemViewModel groupItem)
            {
                var cloneGroup = groupItem.Clone() as GroupItemViewModel;
                if (parent != null)
                {
                    cloneGroup.ParentID = parent.ID;
                    cloneGroup.EnableForSelection.Value = false;
                    parent.AddGroup(cloneGroup);
                }

                var children = from it in Items.OfType<DesignerItemViewModelBase>()
                               where it.ParentID == item.ID
                               orderby it.ZIndex.Value ascending
                               select it;

                foreach (var child in children)
                {
                    DuplicateDesignerItem(selectedItems, oldNewList, child, cloneGroup);
                }

                var childrenConnectors = from it in Items.OfType<ConnectorBaseViewModel>()
                                         where it.ParentID == item.ID
                                         orderby it.ZIndex.Value ascending
                                         select it;

                var childrenConnectedConnectors = from it in childrenConnectors
                                                  where it.SourceConnectorInfo is FullyCreatedConnectorInfo || it.SinkConnectorInfo is FullyCreatedConnectorInfo
                                                  select it;

                var childrenConnectedTargets = (from it in childrenConnectedConnectors
                                                where it.SourceConnectorInfo is FullyCreatedConnectorInfo
                                                select (it.SourceConnectorInfo as FullyCreatedConnectorInfo).DataItem)
                                                .Union(
                                                from it in childrenConnectedConnectors
                                                where it.SinkConnectorInfo is FullyCreatedConnectorInfo
                                                select (it.SinkConnectorInfo as FullyCreatedConnectorInfo).DataItem);

                var childrenConnectedItems = from it in selectedItems.Union(children).Distinct()
                                             where childrenConnectedTargets.Contains(it)
                                             select it;

                foreach (var connector in childrenConnectors)
                {
                    DuplicateConnector(childrenConnectedItems, oldNewList, connector, cloneGroup);
                }

                oldNewList.Add(new Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>(groupItem, cloneGroup));
                cloneGroup.ZIndex.Value = Items.Count();
                Items.Add(cloneGroup);
            }
            else
            {
                var clone = item.Clone() as SelectableDesignerItemViewModelBase;
                clone.ZIndex.Value = Items.Count();
                if (parent != null)
                {
                    clone.ParentID = parent.ID;
                    clone.EnableForSelection.Value = false;
                    parent.AddGroup(clone);
                }
                oldNewList.Add(new Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>(item, clone));
                Items.Add(clone);
            }
        }

        private void DuplicateConnector(IEnumerable<DesignerItemViewModelBase> connectedItems, List<Tuple<SelectableDesignerItemViewModelBase, SelectableDesignerItemViewModelBase>> oldNewList, ConnectorBaseViewModel connector, GroupItemViewModel groupItem = null)
        {
            var clone = connector.Clone() as ConnectorBaseViewModel;
            if (connector.SourceConnectorInfo is FullyCreatedConnectorInfo sourceInfo && connectedItems.Contains(sourceInfo.DataItem))
            {
                var connectDestination = oldNewList.Where(x => x.Item1 == sourceInfo.DataItem)
                                                   .Select(x => x.Item2)
                                                   .Cast<DesignerItemViewModelBase>()
                                                   .Single();
                clone.SourceConnectorInfo = new FullyCreatedConnectorInfo(connectDestination, sourceInfo.Orientation, sourceInfo.Degree);
            }
            if (connector.SinkConnectorInfo is FullyCreatedConnectorInfo sinkInfo && connectedItems.Contains(sinkInfo.DataItem))
            {
                var connectDestination = oldNewList.Where(x => x.Item1 == sinkInfo.DataItem)
                                                   .Select(x => x.Item2)
                                                   .Cast<DesignerItemViewModelBase>()
                                                   .Single();
                clone.SinkConnectorInfo = new FullyCreatedConnectorInfo(connectDestination, sinkInfo.Orientation, sinkInfo.Degree);
            }
            clone.ZIndex.Value = Items.Count();
            if (groupItem != null)
            {
                clone.ParentID = groupItem.ID;
                clone.EnableForSelection.Value = false;
                groupItem.AddGroup(clone);
            }
            Items.Add(clone);
        }

        private bool CanExecuteDuplicate()
        {
            return SelectedItems.Count() > 0;
        }

        #endregion //Duplicate

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

        #region IDisposable

        public void Dispose()
        {
            _CompositeDisposable.Dispose();
        }

        #endregion //IDisposable
    }
}
