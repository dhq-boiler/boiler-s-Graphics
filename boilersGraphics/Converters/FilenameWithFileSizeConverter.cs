﻿using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using boilersGraphics.Helpers;

namespace boilersGraphics.Converters;

internal class FilenameWithFileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var file = value as string;
        var fileinfo = new FileInfo(file);
        return $"{file} [{FileSize.ConvertFileSizeUnit(fileinfo.Length)}]";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}