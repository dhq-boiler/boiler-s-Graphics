using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Helpers
{
    internal class WpfVisualTreeHelper : IVisualTreeHelper
    {
        public Rect GetDescendantBounds(Visual reference)
        {
            return VisualTreeHelper.GetDescendantBounds(reference);
        }
    }
}
