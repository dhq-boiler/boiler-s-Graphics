using boilersGraphics.Models;
using NUnit.Framework;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ModelsBasicTest
    {
        // ---- Corner ----

        [Test]
        public void Corner_デフォルトプロパティ()
        {
            var c = new Corner();
            Assert.That(c.Number.Value, Is.EqualTo(0));
            Assert.That(c.Radius.Value, Is.EqualTo(0));
            Assert.That(c.Angle.Value, Is.EqualTo(0));
            Assert.That(c.Point.Value, Is.EqualTo(default(Point)));
        }

        [Test]
        public void Corner_Equals_全プロパティ一致でtrue()
        {
            var a = new Corner();
            a.Number.Value = 1;
            a.Radius.Value = 2.0;
            a.Angle.Value = 30.0;
            a.Point.Value = new Point(5, 6);

            var b = new Corner();
            b.Number.Value = 1;
            b.Radius.Value = 2.0;
            b.Angle.Value = 30.0;
            b.Point.Value = new Point(5, 6);

            Assert.That(a.Equals(b), Is.True);
        }

        [Test]
        public void Corner_Equals_違うNumberはfalse()
        {
            var a = new Corner();
            a.Number.Value = 1;
            var b = new Corner();
            b.Number.Value = 2;
            Assert.That(a.Equals(b), Is.False);
        }

        [Test]
        public void Corner_EqualsはCorner型以外のobjectでfalse()
        {
            var a = new Corner();
            Assert.That(a.Equals("not a corner"), Is.False);
            Assert.That(a.Equals(null), Is.False);
        }

        [Test]
        public void Corner_GetHashCodeは決定的()
        {
            var a = new Corner();
            a.Number.Value = 1;
            var hash1 = a.GetHashCode();
            var hash2 = a.GetHashCode();
            Assert.That(hash2, Is.EqualTo(hash1));
        }

        // ---- GradientStop ----

        [Test]
        public void GradientStop_ctorでColorとOffsetを初期化()
        {
            var gs = new boilersGraphics.Models.GradientStop(Colors.Red, 0.5);
            Assert.That(gs.Color.Value, Is.EqualTo(Colors.Red));
            Assert.That(gs.Offset.Value, Is.EqualTo(0.5));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GradientStop_ConvertToGradientStopでWPFのGradientStopに変換()
        {
            var gs = new boilersGraphics.Models.GradientStop(Colors.Blue, 0.7);
            var wpfStop = gs.ConvertToGradientStop();
            Assert.That(wpfStop.Color, Is.EqualTo(Colors.Blue));
            Assert.That(wpfStop.Offset, Is.EqualTo(0.7));
        }

        // ---- InOutPair ----

        [Test]
        public void InOutPair_ctorでInOutを初期化()
        {
            var p = new InOutPair(10, 20);
            Assert.That(p.In, Is.EqualTo(10));
            Assert.That(p.Out, Is.EqualTo(20));
        }

        [Test]
        public void InOutPair_ToStringはIn_Out形式()
        {
            var p = new InOutPair(3, 7);
            Assert.That(p.ToString(), Is.EqualTo("3 => 7"));
        }

        [Test]
        public void InOutPair_setterで値変更可能()
        {
            var p = new InOutPair(1, 2);
            p.In = 99;
            p.Out = 100;
            Assert.That(p.In, Is.EqualTo(99));
            Assert.That(p.Out, Is.EqualTo(100));
        }

        // ---- DraggingItem ----

        [Test]
        public void DraggingItem_プロパティの読み書き()
        {
            var d = new DraggingItem();
            d.Item = "anything";
            d.XOffset = 1.5;
            d.YOffset = 2.5;
            Assert.That(d.Item, Is.EqualTo("anything"));
            Assert.That(d.XOffset, Is.EqualTo(1.5));
            Assert.That(d.YOffset, Is.EqualTo(2.5));
        }

        // ---- PrivacyPolicyAgreement ----

        [Test]
        public void PrivacyPolicyAgreement_プロパティの読み書き()
        {
            var enacted = new DateTime(2024, 1, 1);
            var agreed = new DateTime(2024, 6, 1);
            var p = new PrivacyPolicyAgreement
            {
                DateOfEnactment = enacted,
                IsAgree = true,
                DateOfAgreement = agreed,
            };
            Assert.That(p.DateOfEnactment, Is.EqualTo(enacted));
            Assert.That(p.IsAgree, Is.True);
            Assert.That(p.DateOfAgreement, Is.EqualTo(agreed));
        }

        // ---- RenderItem (and Diagram) ----

        [Test]
        public void RenderItem_X_Y_Width_Height_IsSelected()
        {
            var r = new RenderItem
            {
                X = 1,
                Y = 2,
                Width = 30,
                Height = 40,
                IsSelected = true,
            };
            Assert.That(r.X, Is.EqualTo(1));
            Assert.That(r.Y, Is.EqualTo(2));
            Assert.That(r.Width, Is.EqualTo(30));
            Assert.That(r.Height, Is.EqualTo(40));
            Assert.That(r.IsSelected, Is.True);
        }

        [Test]
        public void Diagram_RenderItemsプロパティを保持()
        {
            var d = new Diagram
            {
                RenderItems = new System.Collections.Generic.List<RenderItem>
                {
                    new RenderItem { X = 1 },
                    new RenderItem { X = 2 },
                }
            };
            Assert.That(d.RenderItems.Count, Is.EqualTo(2));
            Assert.That(d.RenderItems[0].X, Is.EqualTo(1));
        }

        // ---- Rectangle / Ellipse / StraightLine ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Rectangle_StrokeStringとFillString_Brushが反映される()
        {
            var rect = new Rectangle();
            rect.StrokeString = "#FF000000";
            rect.FillString = "#FFFF0000";
            Assert.That(rect.StrokeString, Is.EqualTo("#FF000000"));
            Assert.That(rect.FillString, Is.EqualTo("#FFFF0000"));
            Assert.That(rect.Stroke, Is.InstanceOf<SolidColorBrush>());
            Assert.That(((SolidColorBrush)rect.Stroke).Color, Is.EqualTo(Colors.Black));
            Assert.That(((SolidColorBrush)rect.Fill).Color, Is.EqualTo(Colors.Red));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Rectangle_StrokeとFillをBrushで直接set()
        {
            var rect = new Rectangle();
            rect.Stroke = new SolidColorBrush(Colors.Green);
            rect.Fill = new SolidColorBrush(Colors.Yellow);
            Assert.That(((SolidColorBrush)rect.Stroke).Color, Is.EqualTo(Colors.Green));
            Assert.That(((SolidColorBrush)rect.Fill).Color, Is.EqualTo(Colors.Yellow));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_StrokeStringとFillString_Brushが反映される()
        {
            var e = new Ellipse();
            e.StrokeString = "#FF112233";
            e.FillString = "#FFAABBCC";
            Assert.That(e.StrokeString, Is.EqualTo("#FF112233"));
            Assert.That(e.FillString, Is.EqualTo("#FFAABBCC"));
            Assert.That(((SolidColorBrush)e.Stroke).Color,
                Is.EqualTo(Color.FromArgb(0xFF, 0x11, 0x22, 0x33)));
            Assert.That(((SolidColorBrush)e.Fill).Color,
                Is.EqualTo(Color.FromArgb(0xFF, 0xAA, 0xBB, 0xCC)));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_StrokeとFillをBrushで直接set()
        {
            var e = new Ellipse();
            e.Stroke = new SolidColorBrush(Colors.Cyan);
            e.Fill = new SolidColorBrush(Colors.Magenta);
            Assert.That(((SolidColorBrush)e.Stroke).Color, Is.EqualTo(Colors.Cyan));
            Assert.That(((SolidColorBrush)e.Fill).Color, Is.EqualTo(Colors.Magenta));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void StraightLine_X2_Y2_BrushString()
        {
            var line = new StraightLine();
            line.X2 = 100;
            line.Y2 = 50;
            line.BrushString = "#FF00FF00";
            Assert.That(line.X2, Is.EqualTo(100));
            Assert.That(line.Y2, Is.EqualTo(50));
            Assert.That(line.BrushString, Is.EqualTo("#FF00FF00"));
            Assert.That(((SolidColorBrush)line.Brush).Color, Is.EqualTo(Colors.Lime));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void StraightLine_BrushをBrushで直接set()
        {
            var line = new StraightLine();
            line.Brush = new SolidColorBrush(Colors.Orange);
            Assert.That(((SolidColorBrush)line.Brush).Color, Is.EqualTo(Colors.Orange));
        }

        // ---- RootLayer ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void RootLayer_UpdateAppearanceは何もしない_例外なし()
        {
            // RootLayer は internal sealed... 実は internal なので Activator 経由
            var type = typeof(boilersGraphics.Models.PrivacyPolicyAgreement).Assembly
                .GetType("boilersGraphics.Models.RootLayer");
            Assert.That(type, Is.Not.Null);
            var instance = Activator.CreateInstance(type);
            var method = type.GetMethod("UpdateAppearance");
            Assert.That(() => method.Invoke(instance, new object[]
            {
                new System.Collections.Generic.List<boilersGraphics.ViewModels.SelectableDesignerItemViewModelBase>(),
                false,
            }), Throws.Nothing);
        }

        // ---- CanvasPage ----

        [Test]
        public void CanvasPage_ctorでNameを設定_他はデフォルト()
        {
            var page = new CanvasPage("page1");
            Assert.That(page.Name, Is.EqualTo("page1"));
            Assert.That(page.IsActive, Is.False);
            Assert.That(page.IsEditing, Is.False);
            Assert.That(page.SerializedData, Is.Null);
            Assert.That(page.DurationMs, Is.EqualTo(100));
            Assert.That(page.Thumbnail, Is.Null);
        }

        [Test]
        public void CanvasPage_全プロパティの読み書き()
        {
            var page = new CanvasPage("p");
            page.Name = "renamed";
            page.IsActive = true;
            page.IsEditing = true;
            page.DurationMs = 250;
            page.SerializedData = new System.Xml.Linq.XElement("Root");

            Assert.That(page.Name, Is.EqualTo("renamed"));
            Assert.That(page.IsActive, Is.True);
            Assert.That(page.IsEditing, Is.True);
            Assert.That(page.DurationMs, Is.EqualTo(250));
            Assert.That(page.SerializedData.Name.LocalName, Is.EqualTo("Root"));
        }

        // ---- Preference (internal) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Preference_全BindablePropertyの読み書き()
        {
            var p = new Preference();
            p.Width.Value = 1920;
            p.Height.Value = 1080;
            p.CanvasFillBrush.Value = new SolidColorBrush(Colors.White);
            p.CanvasEdgeThickness.Value = 1.0;
            p.CanvasEdgeBrush.Value = new SolidColorBrush(Colors.Black);
            p.EnablePointSnap.Value = true;
            p.SnapPower.Value = 5;
            p.EnableAutoSave.Value = true;
            p.AutoSaveType.Value = AutoSaveType.SetInterval;
            p.AutoSaveInterval.Value = TimeSpan.FromMinutes(10);
            p.EnableImageEmbedding.Value = true;
            p.EnableAutoScrollOnDrag.Value = false;
            p.AutoScrollOnDragSpeed.Value = 50;

            Assert.That(p.Width.Value, Is.EqualTo(1920));
            Assert.That(p.Height.Value, Is.EqualTo(1080));
            Assert.That(p.AutoSaveType.Value, Is.EqualTo(AutoSaveType.SetInterval));
            Assert.That(p.AutoSaveInterval.Value, Is.EqualTo(TimeSpan.FromMinutes(10)));
        }

        [Test]
        public void Preference_EdgeThicknessOptionsはデフォルトで16要素()
        {
            var p = new Preference();
            Assert.That(p.EdgeThicknessOptions.Count, Is.EqualTo(16));
            Assert.That(p.EdgeThicknessOptions[0], Is.EqualTo(0.0));
            Assert.That(p.EdgeThicknessOptions[15], Is.EqualTo(100.0));
        }
    }
}
