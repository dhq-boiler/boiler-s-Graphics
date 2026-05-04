using boilersGraphics.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ExtensionsVisualTreeTest
    {
        // 簡単な Visual Tree 構築ヘルパー
        private static (Grid root, TextBlock leaf) BuildTree(string leafName = "leaf")
        {
            var leaf = new TextBlock { Name = leafName };
            var inner = new Border { Child = leaf };
            var stack = new StackPanel();
            stack.Children.Add(inner);
            var root = new Grid();
            root.Children.Add(stack);
            // Measure/Arrange して Visual Tree を成立させる
            root.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            root.Arrange(new Rect(new Point(0, 0), root.DesiredSize));
            return (root, leaf);
        }

        // ---- GetChildOfType ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetChildOfType_深く埋め込まれた要素も見つかる()
        {
            var (root, leaf) = BuildTree();
            var found = root.GetChildOfType<TextBlock>();
            Assert.That(found, Is.SameAs(leaf));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetChildOfType_該当型がなければnull()
        {
            var (root, _) = BuildTree();
            var found = root.GetChildOfType<Button>();
            Assert.That(found, Is.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetChildOfType_引数nullでnull()
        {
            DependencyObject root = null;
            Assert.That(root.GetChildOfType<Button>(), Is.Null);
        }

        // ---- EnumerateChildOfType ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void EnumerateChildOfType_該当型をすべて返す()
        {
            var grid = new Grid();
            var tb1 = new TextBlock();
            var tb2 = new TextBlock();
            grid.Children.Add(tb1);
            grid.Children.Add(tb2);
            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            grid.Arrange(new Rect(new Point(0, 0), grid.DesiredSize));

            var items = grid.EnumerateChildOfType<TextBlock>().ToList();
            Assert.That(items.Count, Is.GreaterThanOrEqualTo(2));
            Assert.That(items, Does.Contain(tb1));
            Assert.That(items, Does.Contain(tb2));
        }

        // ---- FindAncestor ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void FindAncestor_型一致した親を返す()
        {
            var (root, leaf) = BuildTree();
            var grid = leaf.FindAncestor<Grid>();
            Assert.That(grid, Is.SameAs(root));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void FindAncestor_該当型がなければnull()
        {
            var (_, leaf) = BuildTree();
            var win = leaf.FindAncestor<Window>();
            Assert.That(win, Is.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void FindAncestor_自分自身が型ならばそれを返す()
        {
            var (root, _) = BuildTree();
            var grid = root.FindAncestor<Grid>();
            Assert.That(grid, Is.SameAs(root));
        }

        // ---- Children (DependencyObject) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Children_直接の子をyield()
        {
            var grid = new Grid();
            var tb = new TextBlock();
            grid.Children.Add(tb);
            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var children = grid.Children().ToList();
            Assert.That(children, Does.Contain(tb));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Children_子なしなら空()
        {
            var grid = new Grid();
            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var children = grid.Children().ToList();
            Assert.That(children, Is.Empty);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Children_nullで例外()
        {
            DependencyObject obj = null;
            // yield なので Linq evaluation で例外を発生させる
            Assert.That(() => obj.Children().ToList(),
                Throws.TypeOf<System.ArgumentNullException>());
        }

        // ---- Descendants ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Descendants_全子孫をyield()
        {
            var (root, leaf) = BuildTree();
            var all = root.Descendants().ToList();
            Assert.That(all, Does.Contain((DependencyObject)leaf));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Descendants_nullで例外()
        {
            DependencyObject obj = null;
            Assert.That(() => obj.Descendants().ToList(),
                Throws.TypeOf<System.ArgumentNullException>());
        }

        // ---- Children<T> / Descendants<T> ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ChildrenT_型でフィルタ()
        {
            var grid = new Grid();
            grid.Children.Add(new TextBlock());
            grid.Children.Add(new Button());
            grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var textBlocks = grid.Children<TextBlock>().ToList();
            Assert.That(textBlocks.Count, Is.GreaterThanOrEqualTo(1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DescendantsT_深い子孫もフィルタ()
        {
            var (root, leaf) = BuildTree();
            var blocks = root.Descendants<TextBlock>().ToList();
            Assert.That(blocks, Does.Contain(leaf));
        }

        // ---- GetParent<T> ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetParent_遡って一致する型を返す()
        {
            var (root, leaf) = BuildTree();
            var grid = leaf.GetParent<Grid>();
            Assert.That(grid, Is.SameAs(root));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetParent_該当型がなければdefault()
        {
            var (_, leaf) = BuildTree();
            var win = leaf.GetParent<Window>();
            Assert.That(win, Is.Null);
        }

        // ---- GetParentOfType ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetParentOfType_型指定で親階層を遡って取得()
        {
            var (root, leaf) = BuildTree();
            var grid = leaf.GetParentOfType<Grid>();
            Assert.That(grid, Is.SameAs(root));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetParentOfType_string版_名前一致で取得()
        {
            var (root, leaf) = BuildTree();
            root.Name = "MyRoot";
            var found = leaf.GetParentOfType("MyRoot");
            Assert.That(found, Is.SameAs(root));
        }

        // ---- GetVisualChild<T> (no dataContext) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetVisualChild_深い子孫を再帰的に探索()
        {
            var (root, leaf) = BuildTree();
            var found = root.GetVisualChild<TextBlock>();
            Assert.That(found, Is.SameAs(leaf));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetVisualChild_該当型なしならnull()
        {
            var (root, _) = BuildTree();
            var found = root.GetVisualChild<Button>();
            Assert.That(found, Is.Null);
        }

        // ---- GetChildren (FrameworkElement) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetChildren_子孫を全部yield()
        {
            var (root, leaf) = BuildTree();
            var all = root.GetChildren().ToList();
            Assert.That(all, Does.Contain((FrameworkElement)leaf));
        }

        // ---- FindChild ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void FindChild_名前指定で子を探索()
        {
            var (root, leaf) = BuildTree("targetName");
            var found = root.FindChild<TextBlock>("targetName");
            Assert.That(found, Is.SameAs(leaf));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void FindChild_見つからなければnull()
        {
            var (root, _) = BuildTree();
            var found = root.FindChild<TextBlock>("missingName");
            Assert.That(found, Is.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void FindChild_parent_nullでnull()
        {
            DependencyObject parent = null;
            var found = parent.FindChild<TextBlock>("anything");
            Assert.That(found, Is.Null);
        }

        // ---- FindRoot ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void FindRoot_VisualTreeの根を返す()
        {
            var (root, leaf) = BuildTree();
            var foundRoot = ((Visual)leaf).FindRoot();
            Assert.That(foundRoot, Is.SameAs(root));
        }

        // ---- BoundsRelativeTo ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void BoundsRelativeTo_自分自身に対するBoundsはゼロ原点()
        {
            var (root, _) = BuildTree();
            var bounds = root.BoundsRelativeTo(root);
            Assert.That(bounds.X, Is.EqualTo(0));
            Assert.That(bounds.Y, Is.EqualTo(0));
        }

        // ---- DpiXFactor / DpiYFactor ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void DpiFactor_PresentationSourceなしなら1を返す()
        {
            var (root, _) = BuildTree();
            // Window にぶら下がってないので PresentationSource は null → factor は 1
            Assert.That(root.DpiXFactor(), Is.EqualTo(1));
            Assert.That(root.DpiYFactor(), Is.EqualTo(1));
        }
    }
}
