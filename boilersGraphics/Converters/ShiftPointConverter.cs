using boilersGraphics.ViewModels;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace boilersGraphics.Converters
{
    class ShiftPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModel = value as BezierCurveViewModel;
            switch (parameter)
            {
                case "Point0":
                    return new Point(viewModel.Points[0].X - viewModel.Points[0].X + 50, viewModel.Points[0].Y - viewModel.Points[0].Y + 50);
                case "ControlPoint1":
                    return new Point(viewModel.ControlPoint1.Value.X - viewModel.Points[0].X + 50, viewModel.ControlPoint1.Value.Y - viewModel.Points[0].Y + 50);
                case "ControlPoint2":
                    return new Point(viewModel.ControlPoint2.Value.X - viewModel.Points[0].X + 50, viewModel.ControlPoint2.Value.Y - viewModel.Points[0].Y + 50);
                case "Point1":
                    return new Point(viewModel.Points[1].X - viewModel.Points[0].X + 50, viewModel.Points[1].Y - viewModel.Points[0].Y + 50);
                default:
                    return new Point();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
