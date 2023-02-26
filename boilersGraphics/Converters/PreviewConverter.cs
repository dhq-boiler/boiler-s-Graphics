using boilersGraphics.Helpers;
using NLog;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;

namespace boilersGraphics.Converters;

internal class PreviewConverter : IValueConverter
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            var filename = (string)value;
            var image = new Image();
            var root = XElement.Load(filename);
            var attachments = root.Element("Attachments");
            if (attachments is null)
            {
                return CreateLabelForNotAvailable();
            }
            var picture = attachments.Element("Picture");
            if (picture is null)
            {
                return CreateLabelForNotAvailable();
            }
            var source = picture.Attribute("Source")?.Value;
            if (source is null)
            {
                return CreateLabelForNotAvailable();
            }
            image.Source = ObjectDeserializer.Base64StringToBitmap(source);
            return image;
        }
        catch (IOException)
        {
            return CreateLabelForNotAvailable();
        }
    }

    private static object CreateLabelForNotAvailable()
    {
        var label = new Label();
        label.Content = "プレビューは利用できません。";
        return label;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}