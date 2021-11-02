using boilersGraphics.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace boilersGraphics.Controls
{
    public class SnapPoint : Thumb
    {
        public static readonly DependencyProperty SnapPointPositionProperty = DependencyProperty.Register("SnapPointPosition", typeof(SnapPointPosition), typeof(SnapPoint));

        public SnapPointPosition SnapPointPosition
        {
            get { return (SnapPointPosition)GetValue(SnapPointPositionProperty); }
            set { SetValue(SnapPointPositionProperty, value); }
        }
    }
}
