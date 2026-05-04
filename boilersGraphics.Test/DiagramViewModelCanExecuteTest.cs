using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System;
using System.Threading;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class DiagramViewModelCanExecuteTest
    {
        private static (DiagramViewModel diagram, Layer layer) NewDiagram()
        {
            App.IsTest = true;
            var dlg = new Mock<IDialogService>();
            var mainVM = new MainWindowViewModel(dlg.Object);
            var diagram = new DiagramViewModel(mainVM);
            diagram.Layers.Clear();
            var layer = new Layer();
            layer.Name.Value = "L1";
            diagram.Layers.Add(layer);
            layer.IsSelected.Value = true;
            return (diagram, layer);
        }

        // ---- CanExecuteUndo / CanExecuteRedo ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteUndo_初期状態はfalse()
        {
            var (d, _) = NewDiagram();
            Assert.That(d.CanExecuteUndo(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteUndo_AddItem後はtrue()
        {
            var (d, _) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            Assert.That(d.CanExecuteUndo(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteRedo_初期状態はfalse()
        {
            var (d, _) = NewDiagram();
            Assert.That(d.CanExecuteRedo(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteRedo_Undo後はtrue()
        {
            var (d, _) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            d.UndoCommand.Execute();
            Assert.That(d.CanExecuteRedo(), Is.True);
        }

        // ---- CanExecuteCopy / CanExecuteCut / CanExecutePaste ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteCopy_未選択ならfalse()
        {
            var (d, _) = NewDiagram();
            Assert.That(d.CanExecuteCopy(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteCopy_アイテム選択でtrue()
        {
            var (d, layer) = NewDiagram();
            var item = new NRectangleViewModel();
            d.AddItemCommand.Execute(item);
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteCopy(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteCut_レイヤー選択あり()
        {
            var (d, _) = NewDiagram();
            // レイヤーは初期状態で IsSelected=true → SelectedLayers.Value > 0
            Assert.That(d.CanExecuteCut(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteCut_レイヤー未選択でfalse()
        {
            var (d, layer) = NewDiagram();
            layer.IsSelected.Value = false;
            Assert.That(d.CanExecuteCut(), Is.False);
        }

        // ---- CanExecuteClip / Exclude / Xor / Intersect ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteClip_2要素_先頭がPicture_true()
        {
            var (d, layer) = NewDiagram();
            var pic = new PictureDesignerItemViewModel();
            var rect = new NRectangleViewModel();
            d.AddItemCommand.Execute(pic);
            d.AddItemCommand.Execute(rect);
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteClip(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteClip_要素数が2でなければfalse()
        {
            var (d, layer) = NewDiagram();
            var pic = new PictureDesignerItemViewModel();
            d.AddItemCommand.Execute(pic);
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteClip(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteClip_先頭がPicture以外ならfalse()
        {
            var (d, layer) = NewDiagram();
            var rect = new NRectangleViewModel();
            var ellipse = new NEllipseViewModel();
            d.AddItemCommand.Execute(rect);
            d.AddItemCommand.Execute(ellipse);
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteClip(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteExclude_2要素_どちらもPicture以外でtrue()
        {
            var (d, layer) = NewDiagram();
            var rect = new NRectangleViewModel();
            var ellipse = new NEllipseViewModel();
            d.AddItemCommand.Execute(rect);
            d.AddItemCommand.Execute(ellipse);
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteExclude(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteExclude_片方Picture含むとfalse()
        {
            var (d, layer) = NewDiagram();
            var rect = new NRectangleViewModel();
            var pic = new PictureDesignerItemViewModel();
            d.AddItemCommand.Execute(rect);
            d.AddItemCommand.Execute(pic);
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteExclude(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteExclude_要素数1ならfalse()
        {
            var (d, layer) = NewDiagram();
            var rect = new NRectangleViewModel();
            d.AddItemCommand.Execute(rect);
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteExclude(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteXor_2要素どちらもPicture以外()
        {
            var (d, layer) = NewDiagram();
            var rect = new NRectangleViewModel();
            var ellipse = new NEllipseViewModel();
            d.AddItemCommand.Execute(rect);
            d.AddItemCommand.Execute(ellipse);
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteXor(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteXor_要素0ならfalse()
        {
            var (d, _) = NewDiagram();
            Assert.That(d.CanExecuteXor(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteIntersect_2要素どちらもPicture以外()
        {
            var (d, layer) = NewDiagram();
            var rect = new NRectangleViewModel();
            var ellipse = new NEllipseViewModel();
            d.AddItemCommand.Execute(rect);
            d.AddItemCommand.Execute(ellipse);
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteIntersect(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteIntersect_片方PictureならFalse()
        {
            var (d, layer) = NewDiagram();
            var pic = new PictureDesignerItemViewModel();
            var rect = new NRectangleViewModel();
            d.AddItemCommand.Execute(pic);
            d.AddItemCommand.Execute(rect);
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteIntersect(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteUnion_2要素どちらもPicture以外でtrue()
        {
            var (d, layer) = NewDiagram();
            var rect = new NRectangleViewModel();
            var ellipse = new NEllipseViewModel();
            d.AddItemCommand.Execute(rect);
            d.AddItemCommand.Execute(ellipse);
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteUnion(), Is.True);
        }

        // NOTE: CanExecuteUnion の `if (polyBezier != null) return true;` 経路は
        // GetSelectedItemsForCombine が ConnectorBaseViewModel (PolyBezier の親型)
        // を集めないため、現状の実装では到達できない dead code。
        // PolyBezier 単独選択は false が返る挙動を検証する。
        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteUnion_PolyBezier単独はfalseを返す現状実装()
        {
            var (d, layer) = NewDiagram();
            var pb = new PolyBezierViewModel();
            d.AddItemCommand.Execute(pb);
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteUnion(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteUnion_要素0ならfalse()
        {
            var (d, _) = NewDiagram();
            Assert.That(d.CanExecuteUnion(), Is.False);
        }

        // ---- CanExecuteGroup / CanExecuteUngroup ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteGroup_2要素以上でtrue()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            d.AddItemCommand.Execute(new NEllipseViewModel());
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteGroup(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteGroup_1要素ではfalse()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteGroup(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteUngroup_GroupItem選択でtrue()
        {
            var (d, layer) = NewDiagram();
            var group = new GroupItemViewModel();
            d.AddItemCommand.Execute(group);
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteUngroup(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteUngroup_通常アイテムでfalse()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteUngroup(), Is.False);
        }

        // ---- CanExecuteOrder / Align / Distribute / Uniform / Duplicate ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteOrder_選択ありでtrue()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteOrder(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteOrder_未選択でfalse()
        {
            var (d, _) = NewDiagram();
            Assert.That(d.CanExecuteOrder(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteAlign_2要素以上でtrue()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            d.AddItemCommand.Execute(new NEllipseViewModel());
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteAlign(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteAlign_1要素ではfalse()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteAlign(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteDistribute_2要素以上でtrue()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            d.AddItemCommand.Execute(new NEllipseViewModel());
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteDistribute(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteUniform_DesignerItem2要素以上でtrue()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            d.AddItemCommand.Execute(new NEllipseViewModel());
            layer.Children[0].IsSelected.Value = true;
            layer.Children[1].IsSelected.Value = true;
            Assert.That(d.CanExecuteUniform(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteUniform_1要素ではfalse()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteUniform(), Is.False);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteDuplicate_選択ありでtrue()
        {
            var (d, layer) = NewDiagram();
            d.AddItemCommand.Execute(new NRectangleViewModel());
            layer.Children[0].IsSelected.Value = true;
            Assert.That(d.CanExecuteDuplicate(), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CanExecuteDuplicate_未選択でfalse()
        {
            var (d, _) = NewDiagram();
            Assert.That(d.CanExecuteDuplicate(), Is.False);
        }
    }
}
