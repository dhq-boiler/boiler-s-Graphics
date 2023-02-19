using boilersGraphics.UserControls;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Globalization;
using System.Windows.Data;

namespace boilersGraphics.Converters;

internal class PreviewConverter : IValueConverter
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var diagramViewModel =
            new DiagramViewModel(MainWindowViewModel.Instance, true);
        diagramViewModel.Preview(value as string);
        var previewDiagramControl = new PreviewDiagramControl();
        previewDiagramControl.DataContext = diagramViewModel;
        logger.Debug(previewDiagramControl);
        return previewDiagramControl;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}