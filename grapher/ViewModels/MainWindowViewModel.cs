using grapher.Models;
using grapher.Views.Behaviors;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace grapher.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private ObservableCollection<RenderItemViewModel> _RenderItems;
        private DropAcceptDescription _Description;

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
    }
}
