using DependencyPropertyGenerator;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ZLinq;

namespace boilersGraphics.Controls;

[DesignTimeVisible(true)]
[DependencyProperty<int>("Rows", DefaultValue = 1)]
[DependencyProperty<int>("Columns", DefaultValue = 1)]
[DependencyProperty<Orientation>("Orientation", DefaultValue = Orientation.Horizontal)]
public partial class SimpleGrid : Panel
{
    private List<Cell> _cells;

    /// <summary>
    ///     子要素を配置し、パネルのサイズを決定する。
    /// </summary>
    /// <param name="finalSize">パネル自体と子要素を配置するために使用する親の末尾の領域。</param>
    /// <returns>使用する実際のサイズ。</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var viewport = new Rect(new Point(0, 0), finalSize);

        if (Orientation == Orientation.Horizontal)
        {
            var rows = GetChildrenStructure();
            var iterator = rows.First;
            for (var y = 0; y < rows.Count; ++y)
            {
                var topHeight = _cells.AsValueEnumerable().Where(a => a.Y < y).GroupBy(b => b.Y).Sum(c => c.AsValueEnumerable().Max(d => d.Height));
                var heightOn = _cells.AsValueEnumerable().Where(a => a.Y == y).Max(b => b.Height);
                var row = iterator.Value;
                if (row == null) continue;
                for (var x = 0; x < row.Count; ++x)
                {
                    var cell = row[x];
                    var leftWidth = _cells.AsValueEnumerable().Where(a => a.X < x).GroupBy(b => b.X).Sum(c => c.AsValueEnumerable().Max(d => d.Width));
                    var widthOn = _cells.AsValueEnumerable().Where(a => a.X == x).Max(b => b.Width);
                    var finalRect = new Rect(leftWidth, topHeight, widthOn, heightOn);
                    cell.Arrange(finalRect);
                }

                iterator = iterator.Next;
            }
        }
        else
        {
            var cols = GetChildrenStructure();
            var iterator = cols.First;
            for (var x = 0; x < cols.Count; ++x)
            {
                var leftWidth = _cells.AsValueEnumerable().Where(a => a.X < x).GroupBy(b => b.X).Sum(c => c.AsValueEnumerable().Max(d => d.Width));
                var widthOn = _cells.AsValueEnumerable().Where(a => a.X == x).Max(b => b.Width);

                var row = iterator.Value;
                if (row == null) continue;
                for (var y = 0; y < row.Count; ++y)
                {
                    var cell = row[y];
                    var topHeight = _cells.AsValueEnumerable().Where(a => a.Y < y).GroupBy(b => b.Y).Sum(c => c.AsValueEnumerable().Max(d => d.Height));
                    var heightOn = _cells.AsValueEnumerable().Where(a => a.Y == y).Max(b => b.Height);
                    var finalRect = new Rect(leftWidth, topHeight, widthOn, heightOn);
                    cell.Arrange(finalRect);
                }

                iterator = iterator.Next;
            }
        }

        return finalSize;
    }

    /// <summary>
    ///     子要素に必要なレイアウトのサイズを測定し、パネルのサイズを決定する。
    /// </summary>
    /// <param name="availableSize">子要素に与えることができる使用可能なサイズ。</param>
    /// <returns>レイアウト時にこのパネルが必要とするサイズ。</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        var rows = GetChildrenStructure();
        _cells = getCellsSize(Orientation, availableSize, rows);
        var totalMaxWidth = _cells.AsValueEnumerable().GroupBy(a => a.X).Sum(b => b.AsValueEnumerable().Max(c => c.Width));
        var totalMaxHeight = _cells.AsValueEnumerable().GroupBy(a => a.Y).Sum(b => b.AsValueEnumerable().Max(c => c.Height));

        return new Size(totalMaxWidth, totalMaxHeight);
    }

    private static List<Cell> getCellsSize(Orientation Orientation, Size availableSize,
        LinkedList<List<UIElement>> rows)
    {
        var cells = new List<Cell>();

        if (Orientation == Orientation.Horizontal)
        {
            var iterator = rows.First;
            for (var y = 0; y < rows.Count; ++y)
            {
                var row = iterator.Value;
                for (var x = 0; x < row.Count; ++x)
                {
                    var cell = row[x];
                    cell.Measure(availableSize);
                    var disiredSize = cell.DesiredSize;
                    cells.Add(new Cell(x, y, disiredSize.Width, disiredSize.Height));
                }

                iterator = iterator.Next;
            }
        }
        else
        {
            var iterator = rows.First;
            for (var x = 0; x < rows.Count; ++x)
            {
                var row = iterator.Value;
                for (var y = 0; y < row.Count; ++y)
                {
                    var cell = row[y];
                    cell.Measure(availableSize);
                    var disiredSize = cell.DesiredSize;
                    cells.Add(new Cell(x, y, disiredSize.Width, disiredSize.Height));
                }

                iterator = iterator.Next;
            }
        }

        return cells;
    }

    private LinkedList<List<UIElement>> GetChildrenStructure()
    {
        var rows = new LinkedList<List<UIElement>>();

        if (Orientation == Orientation.Horizontal)
            foreach (UIElement child in InternalChildren)
            {
                var currentList = rows.Last?.Value;
                if (currentList == null || (currentList.Count == Columns && rows.Count < Rows))
                {
                    var list = new List<UIElement>();
                    list.Add(child);
                    rows.AddLast(list);
                }
                else if (currentList.Count < Columns)
                {
                    currentList.Add(child);
                }
            }
        else
            foreach (UIElement child in InternalChildren)
            {
                var currentList = rows.Last?.Value;
                if (currentList == null || (currentList.Count == Rows && rows.Count < Columns))
                {
                    var list = new List<UIElement>();
                    list.Add(child);
                    rows.AddLast(list);
                }
                else if (currentList.Count < Rows)
                {
                    currentList.Add(child);
                }
            }

        return rows;
    }

    private class Cell
    {
        internal Cell(int x, int y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        internal int X { get; }
        internal int Y { get; }
        internal double Width { get; }
        internal double Height { get; }

        public override string ToString()
        {
            return $"{{X={X}, Y={Y}, W={Width}, H={Height}}}";
        }
    }

}