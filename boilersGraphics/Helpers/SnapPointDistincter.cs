using System;
using System.Collections.Generic;
using System.Windows;
using boilersGraphics.ViewModels;

namespace boilersGraphics.Helpers;

internal class SnapPointDistincter : IEqualityComparer<Point>
{
    public bool Equals(Point a, Point b)
    {
        var mainWindowVM = Application.Current.MainWindow.DataContext as MainWindowViewModel;
        var x = Math.Abs(a.X - b.X);
        var y = Math.Abs(a.Y - b.Y);
        var r = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
        return r < mainWindowVM.SnapPower.Value / 2;
    }

    public int GetHashCode(Point obj)
    {
        return obj.GetHashCode();
    }
}