using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using boilersGraphics.UserControls;
using boilersGraphics.ViewModels;

namespace boilersGraphics.Converters;

internal class PreviewConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var diagramViewModel =
            new DiagramViewModel(Application.Current.MainWindow.DataContext as MainWindowViewModel, true);
        diagramViewModel.Preview(value as string);
        var previewDiagramControl = new PreviewDiagramControl();
        previewDiagramControl.DataContext = diagramViewModel;
        return previewDiagramControl;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}