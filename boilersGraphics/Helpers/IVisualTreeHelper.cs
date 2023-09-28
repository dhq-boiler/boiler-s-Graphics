using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers
{
    public interface IVisualTreeHelper
    {
         Rect GetDescendantBounds(Visual reference);
    }
}
