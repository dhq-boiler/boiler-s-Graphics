using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace boilersGraphics.Controls
{
    [DesignTimeVisible(true)]
    public class SimpleGrid : Panel
    {
        #region 依存プロパティ

        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register("Rows",
            typeof(int),
            typeof(SimpleGrid),
            new FrameworkPropertyMetadata(1, new PropertyChangedCallback(SimpleGrid.OnRowsChanged)));

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns",
            typeof(int),
            typeof(SimpleGrid),
            new FrameworkPropertyMetadata(1, new PropertyChangedCallback(SimpleGrid.OnColumnsChanged)));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
            typeof(Orientation),
            typeof(SimpleGrid),
            new FrameworkPropertyMetadata(Orientation.Horizontal));

        #endregion //依存プロパティ

        #region 依存プロパティコールバック

        private static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SimpleGrid ctrl = d as SimpleGrid;
            if (ctrl != null)
            { }
        }

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SimpleGrid ctrl = d as SimpleGrid;
            if (ctrl != null)
            { }
        }

        #endregion //依存プロパティコールバック

        #region CLR プロパティ

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        #endregion //CLR プロパティ

        private List<Cell> _cells;

        /// <summary>
        /// 子要素を配置し、パネルのサイズを決定する。
        /// </summary>
        /// <param name="finalSize">パネル自体と子要素を配置するために使用する親の末尾の領域。</param>
        /// <returns>使用する実際のサイズ。</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Rect viewport = new Rect(new Point(0, 0), finalSize);

            if (Orientation == Orientation.Horizontal)
            {
                LinkedList<List<UIElement>> rows = GetChildrenStructure();
                var iterator = rows.First;
                for (int y = 0; y < rows.Count; ++y)
                {
                    var topHeight = _cells.Where(a => a.Y < y).GroupBy(b => b.Y).Sum(c => c.Max(d => d.Height));
                    var heightOn = _cells.Where(a => a.Y == y).Max(b => b.Height);
                    var row = iterator.Value;
                    if (row == null) continue;
                    for (int x = 0; x < row.Count; ++x)
                    {
                        var cell = row[x];
                        var leftWidth = _cells.Where(a => a.X < x).GroupBy(b => b.X).Sum(c => c.Max(d => d.Width));
                        var widthOn = _cells.Where(a => a.X == x).Max(b => b.Width);
                        Rect finalRect = new Rect(leftWidth, topHeight, widthOn, heightOn);
                        cell.Arrange(finalRect);
                    }
                    iterator = iterator.Next;
                }
            }
            else
            {
                LinkedList<List<UIElement>> cols = GetChildrenStructure();
                var iterator = cols.First;
                for (int x = 0; x < cols.Count; ++x)
                {
                    var leftWidth = _cells.Where(a => a.X < x).GroupBy(b => b.X).Sum(c => c.Max(d => d.Width));
                    var widthOn = _cells.Where(a => a.X == x).Max(b => b.Width);
                    
                    var row = iterator.Value;
                    if (row == null) continue;
                    for (int y = 0; y < row.Count; ++y)
                    {
                        var cell = row[y];
                        var topHeight = _cells.Where(a => a.Y < y).GroupBy(b => b.Y).Sum(c => c.Max(d => d.Height));
                        var heightOn = _cells.Where(a => a.Y == y).Max(b => b.Height);
                        Rect finalRect = new Rect(leftWidth, topHeight, widthOn, heightOn);
                        cell.Arrange(finalRect);
                    }
                    iterator = iterator.Next;
                }
            }

            return finalSize;
        }

        /// <summary>
        /// 子要素に必要なレイアウトのサイズを測定し、パネルのサイズを決定する。
        /// </summary>
        /// <param name="availableSize">子要素に与えることができる使用可能なサイズ。</param>
        /// <returns>レイアウト時にこのパネルが必要とするサイズ。</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            LinkedList<List<UIElement>> rows = GetChildrenStructure();
            _cells = getCellsSize(Orientation, availableSize, rows);
            double totalMaxWidth = _cells.GroupBy(a => a.X).Sum(b => b.Max(c => c.Width));
            double totalMaxHeight = _cells.GroupBy(a => a.Y).Sum(b => b.Max(c => c.Height));

            return new Size(totalMaxWidth, totalMaxHeight);
        }

        private static List<Cell> getCellsSize(Orientation Orientation, Size availableSize, LinkedList<List<UIElement>> rows)
        {
            List<Cell> cells = new List<Cell>();

            if (Orientation == Orientation.Horizontal)
            {
                var iterator = rows.First;
                for (int y = 0; y < rows.Count; ++y)
                {
                    var row = iterator.Value;
                    for (int x = 0; x < row.Count; ++x)
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
                for (int x = 0; x < rows.Count; ++x)
                {
                    var row = iterator.Value;
                    for (int y = 0; y < row.Count; ++y)
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
            LinkedList<List<UIElement>> rows = new LinkedList<List<UIElement>>();

            if (Orientation == Orientation.Horizontal)
            {
                foreach (UIElement child in this.InternalChildren)
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
            }
            else
            {
                foreach (UIElement child in this.InternalChildren)
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
            }

            return rows;
        }

        private class Cell
        {
            internal int X { get; set; }
            internal int Y { get; set; }
            internal double Width { get; set; }
            internal double Height { get; set; }

            internal Cell(int x, int y, double width, double height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            public override string ToString()
            {
                return $"{{X={X}, Y={Y}, W={Width}, H={Height}}}";
            }
        }
    }
}
