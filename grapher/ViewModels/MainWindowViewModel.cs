using grapher.Extensions;
using grapher.Models;
using grapher.Views.Behaviors;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Xml.Serialization;

namespace grapher.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private ObservableCollection<RenderItemViewModel> _RenderItems;
        private DropAcceptDescription _Description;
        private bool _SelectIsChecked;
        private bool _RectangleIsChecked;
        private Cursor _ViewportCursor;
        private Cursor _ApplicationCursor;
        private bool _StraightLineIsChecked;
        private bool _EllipseIsChecked;
        private Type[] extraTypes =
            {
                typeof(BitmapCacheBrush),
                typeof(LinearGradientBrush),
                typeof(RadialGradientBrush),
                typeof(SolidColorBrush),
                typeof(DrawingBrush),
                typeof(ImageBrush),
                typeof(VisualBrush),
                typeof(MatrixTransform),
                typeof(RotateTransform),
                typeof(ScaleTransform),
                typeof(SkewTransform),
                typeof(TransformGroup),
                typeof(TranslateTransform)
            };

        public ICommand SelectModeCommand { get; set; }

        public ICommand StraightLineModeCommand { get; set; }

        public ICommand RectangleModeCommand { get; set; }

        public ICommand EllipseModeCommand { get; set; }

        public ICommand OpenFileCommand { get; set; }

        public ICommand SaveAsCommand { get; set; }

        public Behavior DrawRectangleBehavior { get; set; } = new DrawRectangleBehavior();

        public Behavior DrawStraightLineBehavior { get; set; } = new DrawStraightLineBehavior();

        public Behavior DrawEllipseBehavior { get; set; } = new DrawEllipseBehavior();

        public ObservableCollection<RenderItemViewModel> RenderItems
        {
            get { return _RenderItems; }
            set { SetProperty(ref _RenderItems, value); }
        }

        public MainWindowViewModel()
        {
            RenderItems = new ObservableCollection<RenderItemViewModel>();
            RenderItems.Add(new RectangleViewModel(new Rectangle() { X = 50, Y = 50, Width = 100, Height = 100, Stroke = new SolidColorBrush(Colors.Black), Fill = new SolidColorBrush(Colors.Gray) }));
            RenderItems.Add(new RectangleViewModel(new Rectangle() { X = 250, Y = 250, Width = 100, Height = 100, Stroke = new SolidColorBrush(Colors.Black), Fill = new SolidColorBrush(Colors.Blue) }));
            RenderItems.Add(new EllipseViewModel(new Ellipse() { X = 300, Y = 100, Width = 150, Height = 100, Stroke = new SolidColorBrush(Colors.Red), Fill = new LinearGradientBrush(Colors.DarkBlue, Colors.LightBlue, 0) }));
            Description = new DropAcceptDescription();
            Description.DragDrop += Description_DragDrop;

            RegisterCommands();
        }

        public void Initialize()
        {
            ShowDpi();

            ApplicationCursor = Cursors.Arrow;
            SwitchToSelectMode();
        }

        [Conditional("DEBUG")]
        private static void ShowDpi()
        {
            var dpiX = 96.0 * App.Current.MainWindow.DpiXFactor();
            var dpiY = 96.0 * App.Current.MainWindow.DpiYFactor();
            Debug.WriteLine(dpiX, "DpiX");
            Debug.WriteLine(dpiY, "DpiY");
        }

        private void RegisterCommands()
        {
            SelectModeCommand = new DelegateCommand(() =>
            {
                SwitchToSelectMode();
            });
            StraightLineModeCommand = new DelegateCommand(() =>
            {
                SwitchToStraightLineMode();
            });
            RectangleModeCommand = new DelegateCommand(() =>
            {
                SwitchToRectangleMode();
            });
            EllipseModeCommand = new DelegateCommand(() =>
            {
                SwitchToEllipseMode();
            });
            OpenFileCommand = new DelegateCommand(() =>
            {
                OpenFileDialogAndDeserialize();
            });
            SaveAsCommand = new DelegateCommand(() =>
            {
                OpenSaveFileDialogAndSerialize();
            });
        }

        private void OpenFileDialogAndDeserialize()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "開く";
            dialog.Filter = "全てのファイル|*.*|GRAPHERファイル|*.grapher";
            if (dialog.ShowDialog() == true)
            {
                Deserialize(dialog.FileName);
            }
        }

        private void OpenSaveFileDialogAndSerialize()
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "名前を付けて保存";
            dialog.Filter = "全てのファイル|*.*|GRAPHERファイル|*.grapher";
            if (dialog.ShowDialog() == true)
            {
                Serialize(dialog.FileName);
            }
        }

        private void Deserialize(string fileName)
        {
            var deserializer = new XmlSerializer(typeof(Diagram), extraTypes);
            var sr = new StreamReader(fileName, new UTF8Encoding(false));
            var obj = deserializer.Deserialize(sr) as Diagram;
            RenderItems = new ObservableCollection<RenderItemViewModel>();
            foreach (var item in obj.RenderItems)
            {
                switch (item.GetType().Name)
                {
                    case "Rectangle":
                        RenderItems.Add(new RectangleViewModel(item as Rectangle));
                        break;
                    case "StraightLine":
                        RenderItems.Add(new StraightLineViewModel(item as StraightLine));
                        break;
                    case "Ellipse":
                        RenderItems.Add(new EllipseViewModel(item as Ellipse));
                        break;
                }
            }
        }

        private void Serialize(string fileName)
        {
            var renderItems = RenderItems.Select(i => i.Model);
            var serializeObj = new Diagram();
            serializeObj.RenderItems = renderItems.ToList();
            var serializer = new XmlSerializer(typeof(Diagram), extraTypes);
            var sw = new StreamWriter(fileName, false, new UTF8Encoding(false));
            serializer.Serialize(sw, serializeObj);
            sw.Close();
        }

        private void SwitchToRectangleMode()
        {
            RectangleIsChecked = true;
            SelectIsChecked = false;
            StraightLineIsChecked = false;
            EllipseIsChecked = false;

            ViewportCursor = Cursors.Cross;

            var itemsControl = (ItemsControl)App.Current.MainWindow.FindName("viewport_host");
            var canvas = (Canvas)itemsControl.GetChildOfType<Canvas>();
            var behaviors = Interaction.GetBehaviors(canvas);
            behaviors.Remove(DrawStraightLineBehavior);
            if (!behaviors.Contains(DrawRectangleBehavior))
            {
                behaviors.Add(DrawRectangleBehavior);
            }
            behaviors.Remove(DrawEllipseBehavior);
        }

        private void SwitchToStraightLineMode()
        {
            StraightLineIsChecked = true;
            SelectIsChecked = false;
            RectangleIsChecked = false;
            EllipseIsChecked = false;

            ViewportCursor = Cursors.Cross;

            var itemsControl = (ItemsControl)App.Current.MainWindow.FindName("viewport_host");
            var canvas = (Canvas)itemsControl.GetChildOfType<Canvas>();
            var behaviors = Interaction.GetBehaviors(canvas);
            if (!behaviors.Contains(DrawStraightLineBehavior))
            {
                behaviors.Add(DrawStraightLineBehavior);
            }
            behaviors.Remove(DrawRectangleBehavior);
            behaviors.Remove(DrawEllipseBehavior);
        }

        private void SwitchToEllipseMode()
        {
            EllipseIsChecked = true;
            SelectIsChecked = false;
            RectangleIsChecked = false;
            StraightLineIsChecked = false;

            ViewportCursor = Cursors.Cross;

            var itemsControl = (ItemsControl)App.Current.MainWindow.FindName("viewport_host");
            var canvas = (Canvas)itemsControl.GetChildOfType<Canvas>();
            var behaviors = Interaction.GetBehaviors(canvas);
            behaviors.Remove(DrawStraightLineBehavior);
            behaviors.Remove(DrawRectangleBehavior);
            if (!behaviors.Contains(DrawEllipseBehavior))
            {
                behaviors.Add(DrawEllipseBehavior);
            }
        }

        private void SwitchToSelectMode()
        {
            SelectIsChecked = true;
            StraightLineIsChecked = false;
            RectangleIsChecked = false;
            EllipseIsChecked = false;

            ViewportCursor = Cursors.Arrow;

            var itemsControl = (ItemsControl)App.Current.MainWindow.FindName("viewport_host");
            var canvas = (Canvas)itemsControl.GetChildOfType<Canvas>();
            var behaviors = Interaction.GetBehaviors(canvas);
            behaviors.Remove(DrawStraightLineBehavior);
            behaviors.Remove(DrawRectangleBehavior);
            behaviors.Remove(DrawEllipseBehavior);
        }

        private void Description_DragDrop(System.Windows.DragEventArgs obj)
        {
            var item = obj.Data.GetData(typeof(DraggingItem)) as DraggingItem;
            var source = (obj.Source as FrameworkElement).GetParentOfType<Canvas>();

            var position = obj.GetPosition(source);

            var rectangle = item.Item as RectangleViewModel;
            if (rectangle != null)
            {
                rectangle.Model.X = position.X - item.XOffset;
                rectangle.Model.Y = position.Y - item.YOffset;
            }

            var line = item.Item as StraightLineViewModel;
            if (line != null)
            {
                line.Model.X += position.X - item.XOffset;
                line.Model.Y += position.Y - item.YOffset;
                (line.Model as StraightLine).X2 += position.X - item.XOffset;
                (line.Model as StraightLine).Y2 += position.Y - item.YOffset;
            }

            var ellipse = item.Item as EllipseViewModel;
            if (ellipse != null)
            {
                ellipse.Model.X = position.X - item.XOffset;
                ellipse.Model.Y = position.Y - item.YOffset;
            }
        }

        public DropAcceptDescription Description
        {
            get { return _Description; }
            set { SetProperty(ref _Description, value); }
        }

        public Cursor ApplicationCursor
        {
            get { return _ApplicationCursor; }
            set { SetProperty(ref _ApplicationCursor, value); }
        }

        public Cursor ViewportCursor
        {
            get { return _ViewportCursor; }
            set { SetProperty(ref _ViewportCursor, value); }
        }

        public bool SelectIsChecked
        {
            get { return _SelectIsChecked; }
            set { SetProperty(ref _SelectIsChecked, value); }
        }

        public bool StraightLineIsChecked
        {
            get { return _StraightLineIsChecked; }
            set { SetProperty(ref _StraightLineIsChecked, value); }
        }

        public bool RectangleIsChecked
        {
            get { return _RectangleIsChecked; }
            set { SetProperty(ref _RectangleIsChecked, value); }
        }

        public bool EllipseIsChecked
        {
            get { return _EllipseIsChecked; }
            set { SetProperty(ref _EllipseIsChecked, value); }
        }
    }
}
