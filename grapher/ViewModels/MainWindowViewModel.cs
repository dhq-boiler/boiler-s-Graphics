using grapher.Extensions;
using grapher.Models;
using grapher.Views.Behaviors;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

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

        public ICommand SelectModeCommand { get; set; }

        public ICommand RectangleModeCommand { get; set; }

        public Behavior DrawBehavior { get; set; } = new DrawRectangleBehavior();

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
            Description = new DropAcceptDescription();
            Description.DragDrop += Description_DragDrop;

            RegisterCommands();
        }

        public void Initialize()
        {
            ApplicationCursor = Cursors.Arrow;
            SwitchToSelectMode();
        }

        private void RegisterCommands()
        {
            SelectModeCommand = new DelegateCommand(() =>
            {
                SwitchToSelectMode();
            });
            RectangleModeCommand = new DelegateCommand(() =>
            {
                SwitchToRectangleMode();
            });
        }

        private void SwitchToRectangleMode()
        {
            RectangleIsChecked = true;
            SelectIsChecked = false;

            ViewportCursor = Cursors.Cross;

            var itemsControl = (ItemsControl)App.Current.MainWindow.FindName("viewport_host");
            var canvas = (Canvas)itemsControl.GetChildOfType<Canvas>();
            var behaviors = Interaction.GetBehaviors(canvas);
            behaviors.Add(DrawBehavior);
        }

        private void SwitchToSelectMode()
        {
            SelectIsChecked = true;
            RectangleIsChecked = false;

            ViewportCursor = Cursors.Arrow;

            var itemsControl = (ItemsControl)App.Current.MainWindow.FindName("viewport_host");
            var canvas = (Canvas)itemsControl.GetChildOfType<Canvas>();
            var behaviors = Interaction.GetBehaviors(canvas);
            behaviors.Remove(DrawBehavior);
        }

        private void Description_DragDrop(System.Windows.DragEventArgs obj)
        {
            var item = obj.Data.GetData(typeof(DraggingItem)) as DraggingItem;
            var rectangle = item.Item as RenderItemViewModel;

            FrameworkElement source = (FrameworkElement)obj.Source;
            while (!(source is Canvas))
            {
                source = (FrameworkElement)VisualTreeHelper.GetParent((DependencyObject)source);
            }

            var position = obj.GetPosition((IInputElement)source);
            rectangle.X.Value = position.X - item.XOffset;
            rectangle.Y.Value = position.Y - item.YOffset;
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

        public bool RectangleIsChecked
        {
            get { return _RectangleIsChecked; }
            set { SetProperty(ref _RectangleIsChecked, value); }
        }
    }
}
