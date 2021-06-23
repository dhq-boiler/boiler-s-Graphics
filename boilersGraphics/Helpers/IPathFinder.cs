using boilersGraphics.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace boilersGraphics.Helpers
{
    public interface IPathFinder
    {
        List<Point> GetConnectionLine(ConnectorInfo source, ConnectorInfo sink, bool showLastLine);
        List<Point> GetConnectionLine(ConnectorInfo source, Point sinkPoint, ConnectorOrientation preferredOrientation);
    }
}
