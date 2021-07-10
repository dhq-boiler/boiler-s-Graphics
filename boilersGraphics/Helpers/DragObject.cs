using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.Helpers
{
    // Wraps info of the dragged object into a class
    public class DragObject
    {
        public Size? DesiredSize { get; set; }
        public Type ContentType { get; set; }
    }
}
