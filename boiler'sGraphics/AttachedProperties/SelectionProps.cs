using boiler_sGraphics.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace boiler_sGraphics.AttachedProperties
{
    public static class SelectionProps
    {
        #region EnabledForSelection

        public static readonly DependencyProperty EnabledForSelectionProperty =
            DependencyProperty.RegisterAttached("EnabledForSelection", typeof(bool), typeof(SelectionProps),
                new FrameworkPropertyMetadata((bool)false,
                    new PropertyChangedCallback(OnEnabledForSelectionChanged)));

        public static bool GetEnabledForSelection(DependencyObject d)
        {
            return (bool)d.GetValue(EnabledForSelectionProperty);
        }

        public static void SetEnabledForSelection(DependencyObject d, bool value)
        {
            d.SetValue(EnabledForSelectionProperty, value);
        }

        private static void OnEnabledForSelectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)d;
            if ((bool)e.NewValue)
            {
                fe.PreviewMouseLeftButtonDown += Fe_PreviewMouseLeftButtonDown;
            }
            else
            {
                fe.PreviewMouseLeftButtonDown -= Fe_PreviewMouseLeftButtonDown;
            }
        }

        #endregion

        private static void Fe_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectableDesignerItemViewModelBase selectableDesignerItemViewModelBase =
                (SelectableDesignerItemViewModelBase)((FrameworkElement)sender).DataContext;

            if (selectableDesignerItemViewModelBase != null)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                {
                    if ((Keyboard.Modifiers & (ModifierKeys.Shift)) != ModifierKeys.None)
                    {
                        selectableDesignerItemViewModelBase.IsSelected = !selectableDesignerItemViewModelBase.IsSelected;
                    }

                    if ((Keyboard.Modifiers & (ModifierKeys.Control)) != ModifierKeys.None)
                    {
                        selectableDesignerItemViewModelBase.IsSelected = !selectableDesignerItemViewModelBase.IsSelected;
                    }
                }
                else
                {
                    selectableDesignerItemViewModelBase.Owner.DeselectAll();
                    selectableDesignerItemViewModelBase.Owner.EdgeColors.Clear();
                    selectableDesignerItemViewModelBase.Owner.FillColors.Clear();
                    selectableDesignerItemViewModelBase.IsSelected = true;

                    if (selectableDesignerItemViewModelBase is DesignerItemViewModelBase)
                    {
                        var viewModel = selectableDesignerItemViewModelBase as DesignerItemViewModelBase;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"(x, y) = ({viewModel.Left.Value}, {viewModel.Top.Value}) (w, h) = ({viewModel.Width.Value}, {viewModel.Height.Value})";
                    }
                    else if (selectableDesignerItemViewModelBase is ConnectorBaseViewModel)
                    {
                        var viewModel = selectableDesignerItemViewModelBase as ConnectorBaseViewModel;
                        (App.Current.MainWindow.DataContext as MainWindowViewModel).Details.Value = $"({viewModel.SourceA.X}, {viewModel.SourceA.Y}) - ({viewModel.SourceB.X}, {viewModel.SourceB.Y})";
                    }

                    Color edgeColor = Colors.Transparent;
                    Color fillColor = Colors.Transparent;
                    if (selectableDesignerItemViewModelBase is DesignerItemViewModelBase)
                    {
                        edgeColor = (selectableDesignerItemViewModelBase as DesignerItemViewModelBase).EdgeColor;
                        fillColor = (selectableDesignerItemViewModelBase as DesignerItemViewModelBase).FillColor;
                    }
                    else if (selectableDesignerItemViewModelBase is ConnectorBaseViewModel)
                    {
                        edgeColor = (selectableDesignerItemViewModelBase as ConnectorBaseViewModel).EdgeColor;
                    }
                    selectableDesignerItemViewModelBase.Owner.EdgeColors.Add(edgeColor);
                    selectableDesignerItemViewModelBase.Owner.FillColors.Add(fillColor);
                }
            }
        }
    }
}
