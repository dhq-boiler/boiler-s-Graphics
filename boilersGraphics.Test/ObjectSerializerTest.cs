using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.ColorCorrect;
using NUnit.Framework;
using R3;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ObjectSerializerTest
    {
        // 本クラスの本番上のバグ防止意図:
        // - ObjectSerializer.ExtractItem の出力 XML はプロジェクト保存ファイル
        //   (.bg / .xml) の構造そのもの。属性を 1 つ落とすと既存ファイルが
        //   読み戻せなくなる、もしくは新規保存ファイルが古い版で開けなくなる。
        // - 各派生型 (Rectangle/Ellipse/Polygon/Mosaic/Blur/ColorCorrect/Letter
        //   /StraightConnector/BezierCurve/PolyBezier/SnapPoint) の固有属性が
        //   出力されることをファイル互換のために pin する。
        //
        // - PictureDesignerItemViewModel / CroppedPictureDesignerItemViewModel は
        //   App.GetCurrentApp().MainWindow.DataContext 経由で MainWindowVM に
        //   触りに行くため、UI bootstrap なしの単体テストでは検証不可。
        //   (Save/Load の本物のシナリオで要 IDE / 統合テスト)。

        private static MainWindowViewModel _mainVM;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            App.IsTest = true;
            var dlg = new Moq.Mock<Prism.Services.Dialogs.IDialogService>();
            _mainVM = new MainWindowViewModel(dlg.Object);
        }

        // ---- DesignerItem 共通 ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void DesignerItem_共通フィールドが全部XMLに含まれる()
        {
            var rect = new NRectangleViewModel();
            rect.Left.Value = 10;
            rect.Top.Value = 20;
            rect.Width.Value = 30;
            rect.Height.Value = 40;
            rect.ZIndex.Value = 5;
            rect.EdgeBrush.Value = new SolidColorBrush(Colors.Red);
            rect.FillBrush.Value = new SolidColorBrush(Colors.Blue);
            rect.EdgeThickness.Value = 2;
            rect.RotationAngle.Value = 45;

            var x = ObjectSerializer.ExtractItem(rect);

            Assert.That(x.Name.LocalName, Is.EqualTo("DesignerItem"));
            // 必須フィールド全部
            Assert.That(x.Element("ID"), Is.Not.Null);
            Assert.That(x.Element("ParentID"), Is.Not.Null);
            Assert.That(x.Element("Type").Value, Is.EqualTo(typeof(NRectangleViewModel).FullName));
            Assert.That(x.Element("Left").Value, Is.EqualTo("10"));
            Assert.That(x.Element("Top").Value, Is.EqualTo("20"));
            Assert.That(x.Element("Width").Value, Is.EqualTo("30"));
            Assert.That(x.Element("Height").Value, Is.EqualTo("40"));
            Assert.That(x.Element("ZIndex").Value, Is.EqualTo("5"));
            Assert.That(x.Element("EdgeBrush"), Is.Not.Null);
            Assert.That(x.Element("FillBrush"), Is.Not.Null);
            Assert.That(x.Element("EdgeThickness").Value, Is.EqualTo("2"));
            Assert.That(x.Element("PathGeometryNoRotate"), Is.Not.Null);
            Assert.That(x.Element("PathGeometryRotate"), Is.Not.Null);
            Assert.That(x.Element("RotationAngle").Value, Is.EqualTo("45"));
            Assert.That(x.Element("StrokeLineJoin"), Is.Not.Null);
            Assert.That(x.Element("StrokeMiterLimit"), Is.Not.Null);
            Assert.That(x.Element("StrokeDashArray"), Is.Not.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void NRectangle_RadiusXとRadiusYが追加される()
        {
            var rect = new NRectangleViewModel();
            rect.RadiusX.Value = 7;
            rect.RadiusY.Value = 9;
            var x = ObjectSerializer.ExtractItem(rect);
            Assert.That(x.Element("RadiusX").Value, Is.EqualTo("7"));
            Assert.That(x.Element("RadiusY").Value, Is.EqualTo("9"));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void NEllipse_RadiusXY無し_共通フィールドのみ()
        {
            var e = new NEllipseViewModel();
            var x = ObjectSerializer.ExtractItem(e);
            Assert.That(x.Element("Type").Value, Is.EqualTo(typeof(NEllipseViewModel).FullName));
            Assert.That(x.Element("RadiusX"), Is.Null, "Ellipse は RadiusX を出力しない");
            Assert.That(x.Element("RadiusY"), Is.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void NPolygon_Dataが追加される()
        {
            var poly = new NPolygonViewModel();
            poly.Data.Value = "M 0,0 L 10,10";
            var x = ObjectSerializer.ExtractItem(poly);
            Assert.That(x.Element("Data").Value, Is.EqualTo("M 0,0 L 10,10"));
        }

        // ---- Effect 系 ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Mosaic_ColumnPixelsとRowPixelsが追加()
        {
            var m = new MosaicViewModel();
            m.ColumnPixels.Value = 16;
            m.RowPixels.Value = 24;
            var x = ObjectSerializer.ExtractItem(m);
            Assert.That(x.Element("ColumnPixels").Value, Is.EqualTo("16"));
            Assert.That(x.Element("RowPixels").Value, Is.EqualTo("24"));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void BlurEffect_KernelとSigmaが追加()
        {
            var b = new BlurEffectViewModel();
            b.KernelWidth.Value = 33;
            b.KernelHeight.Value = 55;
            b.Sigma.Value = 3.5;
            var x = ObjectSerializer.ExtractItem(b);
            Assert.That(x.Element("KernelWidth").Value, Is.EqualTo("33"));
            Assert.That(x.Element("KernelHeight").Value, Is.EqualTo("55"));
            Assert.That(x.Element("Sigma").Value, Is.EqualTo("3.5"));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ColorCorrect_HSVではAddHue_AddSaturation_AddValueが追加()
        {
            var cc = new ColorCorrectViewModel();
            cc.CCType.Value = ColorCorrectType.HSV;
            cc.AddHue.Value = 10;
            cc.AddSaturation.Value = 20;
            cc.AddValue.Value = 30;

            var x = ObjectSerializer.ExtractItem(cc);

            Assert.That(x.Element("CCType").Value, Is.EqualTo("Hsv"));
            Assert.That(x.Element("AddHue").Value, Is.EqualTo("10"));
            Assert.That(x.Element("AddSaturation").Value, Is.EqualTo("20"));
            Assert.That(x.Element("AddValue").Value, Is.EqualTo("30"));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ColorCorrect_NegativePositiveConversionは追加属性なし()
        {
            var cc = new ColorCorrectViewModel();
            cc.CCType.Value = ColorCorrectType.NegativePositiveConversion;

            var x = ObjectSerializer.ExtractItem(cc);

            Assert.That(x.Element("CCType").Value, Is.EqualTo("NegativePositiveConversion"));
            Assert.That(x.Element("AddHue"), Is.Null);
            Assert.That(x.Element("Curves"), Is.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ColorCorrect_ToneCurveではCurvesとTargetChannelが追加()
        {
            var cc = new ColorCorrectViewModel();
            cc.CCType.Value = ColorCorrectType.ToneCurve;

            var x = ObjectSerializer.ExtractItem(cc);

            Assert.That(x.Element("CCType").Value, Is.EqualTo("ToneCurve"));
            Assert.That(x.Element("TargetChannel"), Is.Not.Null);
            Assert.That(x.Element("Curves"), Is.Not.Null);
        }

        // ---- Letter ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Letter_LetterStringと書体属性が追加()
        {
            var l = new LetterDesignerItemViewModel();
            l.LetterString.Value = "Hello";
            l.IsBold.Value = true;
            l.IsItalic.Value = true;
            l.FontSize.Value = 14;
            l.IsAutoLineBreak.Value = true;
            // SelectedFontFamily は既定値で OK

            var x = ObjectSerializer.ExtractItem(l);

            Assert.That(x.Element("LetterString").Value, Is.EqualTo("Hello"));
            // XElement は bool を XmlConvert.ToString で "true"/"false" (lowercase) にする
            Assert.That(x.Element("IsBold").Value, Is.EqualTo("true"));
            Assert.That(x.Element("IsItalic").Value, Is.EqualTo("true"));
            Assert.That(x.Element("FontSize").Value, Is.EqualTo("14"));
            Assert.That(x.Element("AutoLineBreak").Value, Is.EqualTo("true"));
            Assert.That(x.Element("SelectedFontFamily"), Is.Not.Null);
        }

        // ---- Connector ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void StraightConnector_BeginPointとEndPointが追加_ConnectorItemルート()
        {
            #pragma warning disable CS0612 // 古い形式コンストラクタ警告は意図して使用
            var diagramVM = _mainVM.DiagramViewModel;
            var conn = new StraightConnectorViewModel(diagramVM, new Point(0, 0), new Point(10, 20));
            #pragma warning restore CS0612

            var x = ObjectSerializer.ExtractItem(conn);

            Assert.That(x.Name.LocalName, Is.EqualTo("ConnectorItem"));
            Assert.That(x.Element("Type").Value, Is.EqualTo(typeof(StraightConnectorViewModel).FullName));
            Assert.That(x.Element("BeginPoint"), Is.Not.Null);
            Assert.That(x.Element("EndPoint"), Is.Not.Null);
            Assert.That(x.Element("EdgeBrush"), Is.Not.Null);
            Assert.That(x.Element("PathGeometry"), Is.Not.Null);
            // Connector は FillBrush なし
            Assert.That(x.Element("FillBrush"), Is.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void BezierCurve_ControlPoint1と2が追加()
        {
            var bezier = new BezierCurveViewModel();
            bezier.Points.Add(new Point(0, 0));
            bezier.Points.Add(new Point(100, 100));
            bezier.ControlPoint1.Value = new Point(20, 80);
            bezier.ControlPoint2.Value = new Point(80, 20);

            var x = ObjectSerializer.ExtractItem(bezier);

            Assert.That(x.Name.LocalName, Is.EqualTo("ConnectorItem"));
            Assert.That(x.Element("ControlPoint1"), Is.Not.Null);
            Assert.That(x.Element("ControlPoint2"), Is.Not.Null);
            // BeginPoint/EndPoint も含まれる
            Assert.That(x.Element("BeginPoint"), Is.Not.Null);
            Assert.That(x.Element("EndPoint"), Is.Not.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void PolyBezier_Pointsが空白区切り文字列で追加()
        {
            var pb = new PolyBezierViewModel();
            pb.Points.Add(new Point(0, 0));
            pb.Points.Add(new Point(10, 20));
            pb.Points.Add(new Point(30, 40));

            var x = ObjectSerializer.ExtractItem(pb);

            Assert.That(x.Element("Points").Value, Is.EqualTo("0,0 10,20 30,40"));
        }

        // ---- SnapPoint ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void SnapPoint_SnapPointItemルート_全フィールド()
        {
            #pragma warning disable CS0612
            var diagramVM = _mainVM.DiagramViewModel;
            // 親 Connector を介して SnapPoint を作る
            var conn = new StraightConnectorViewModel(diagramVM, new Point(0, 0), new Point(50, 50));
            #pragma warning restore CS0612
            var snap = conn.SnapPoint0VM.Value;
            snap.Left.Value = 5;
            snap.Top.Value = 6;
            snap.Width.Value = 8;
            snap.Height.Value = 9;
            snap.Opacity.Value = 0.5;
            // SnapPointViewModel の PathGeometry プロパティ自体は base class で
            // set なし初期値 null。本番では Behavior などが値を埋めるが、ここでは
            // テスト用に空の PathGeometry を持つ ReactiveProperty を割り当てる。
            // (これがないと ObjectSerializer.ExtractItem が NullReferenceException を投げる)
            snap.PathGeometry = R3.Observable.Return(new System.Windows.Media.PathGeometry())
                .ToReadOnlyBindableReactiveProperty();

            var x = ObjectSerializer.ExtractItem(snap);

            Assert.That(x.Name.LocalName, Is.EqualTo("SnapPointItem"));
            Assert.That(x.Element("Left").Value, Is.EqualTo("5"));
            Assert.That(x.Element("Top").Value, Is.EqualTo("6"));
            Assert.That(x.Element("Width").Value, Is.EqualTo("8"));
            Assert.That(x.Element("Height").Value, Is.EqualTo("9"));
            Assert.That(x.Element("Opacity").Value, Is.EqualTo("0.5"));
            Assert.That(x.Element("Matrix"), Is.Not.Null);
            Assert.That(x.Element("PathGeometry"), Is.Not.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void SnapPoint_PathGeometry未設定だとExtractItemがNRE_実装の暗黙前提を文書化()
        {
            // SnapPointViewModel.PathGeometry プロパティは base class で set だけが用意され、
            // SnapPointViewModel ctor / Init では値を設定しない。ObjectSerializer.ExtractItem は
            // snapPointItem.PathGeometry.Value をそのまま参照するので、PathGeometry プロパティ
            // 自体が null だと NullReferenceException を投げる。
            //
            // 本番では SetSnapPointBehavior 等の経路で PathGeometry が事前に埋まる前提だが、
            // 暗黙の前提になっているため、ここで「契約として PathGeometry がない SnapPoint は
            // 直接シリアライズできない」ことを記録しておく。将来的には ObjectSerializer 側で
            // null-conditional を追加する fix を提案する余地あり。
            #pragma warning disable CS0612
            var diagramVM = _mainVM.DiagramViewModel;
            var conn = new StraightConnectorViewModel(diagramVM, new Point(0, 0), new Point(50, 50));
            #pragma warning restore CS0612
            var snap = conn.SnapPoint0VM.Value;
            // PathGeometry を意図的に未設定のまま

            Assert.That(() => ObjectSerializer.ExtractItem(snap),
                Throws.TypeOf<NullReferenceException>(),
                "実装が PathGeometry 設定済みを暗黙前提にしている");
        }

        // ---- 派生型がいずれでもない場合 ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ExtractItem_GroupItemは共通DesignerItemとしてシリアライズされる()
        {
            // GroupItemViewModel は DesignerItemViewModelBase 派生。
            // 専用属性なしで共通フィールドのみ出力される (実装上 if 分岐を持たない)。
            var group = new GroupItemViewModel();
            group.Width.Value = 100;
            group.Height.Value = 80;
            var x = ObjectSerializer.ExtractItem(group);
            Assert.That(x.Name.LocalName, Is.EqualTo("DesignerItem"));
            Assert.That(x.Element("Type").Value, Is.EqualTo(typeof(GroupItemViewModel).FullName));
            // 共通必須フィールドは入る
            Assert.That(x.Element("Width").Value, Is.EqualTo("100"));
            Assert.That(x.Element("Height").Value, Is.EqualTo("80"));
            // Group 専用属性は無い
            Assert.That(x.Element("ColumnPixels"), Is.Null);
            Assert.That(x.Element("KernelWidth"), Is.Null);
            Assert.That(x.Element("LetterString"), Is.Null);
        }

        // ---- ExtractItems (LayerItem コレクション) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void ExtractItems_LayerItemsルート_各LayerItemが子要素()
        {
            var item1 = new NRectangleViewModel();
            var item2 = new NEllipseViewModel();
            var layer = new Layer();
            var li1 = new LayerItem(item1, layer, "rect");
            var li2 = new LayerItem(item2, layer, "ellipse");

            var x = ObjectSerializer.ExtractItems(new[] { li1, li2 });

            Assert.That(x.Name.LocalName, Is.EqualTo("LayerItems"));
            var layerItemEls = x.Elements("LayerItem").ToList();
            Assert.That(layerItemEls.Count, Is.EqualTo(2));
            Assert.That(layerItemEls[0].Element("Name").Value, Is.EqualTo("rect"));
            Assert.That(layerItemEls[1].Element("Name").Value, Is.EqualTo("ellipse"));
        }

        // ---- SerializeLayers ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void SerializeLayers_各LayerにIsVisible_Name_Color_Childrenが含まれる()
        {
            var layer = new Layer();
            layer.Name.Value = "MyLayer";
            layer.Color.Value = Colors.Red;
            layer.IsVisible.Value = true;
            layer.IsExpanded.Value = false;

            var item = new NRectangleViewModel();
            var li = new LayerItem(item, layer, "child");
            layer.Children.Add(li);

            // SerializeLayers は NotifyCollectionChangedSynchronizedViewList を要求するので
            // Diagram 経由で生成
            var diagramVM = _mainVM.DiagramViewModel;
            diagramVM.Layers.Clear();
            diagramVM.Layers.Add(layer);

            var xs = ObjectSerializer.SerializeLayers(diagramVM.Layers).ToList();
            Assert.That(xs.Count, Is.EqualTo(1));
            var layerXml = xs[0];
            Assert.That(layerXml.Name.LocalName, Is.EqualTo("Layer"));
            Assert.That(layerXml.Element("Name").Value, Is.EqualTo("MyLayer"));
            Assert.That(layerXml.Element("IsVisible").Value, Is.EqualTo("true"));
            Assert.That(layerXml.Element("IsExpanded").Value, Is.EqualTo("false"));
            Assert.That(layerXml.Element("Color"), Is.Not.Null);
            Assert.That(layerXml.Element("Children"), Is.Not.Null);
            // Children 配下に LayerItem が 1 つ
            Assert.That(layerXml.Element("Children").Elements("LayerItem").Count(), Is.EqualTo(1));
        }

        // ---- PointsToStr (private) ----

        [Test]
        public void PointsToStr_空白区切りでX_Yをjoin()
        {
            // private static string PointsToStr(ObservableCollection<Point>) を reflection で叩く
            var asm = typeof(ObjectSerializer).Assembly;
            var t = asm.GetType("boilersGraphics.Helpers.ObjectSerializer");
            var method = t.GetMethod("PointsToStr",
                BindingFlags.NonPublic | BindingFlags.Static);

            var pts = new ObservableCollection<Point>
            {
                new Point(1, 2),
                new Point(3, 4),
                new Point(5, 6),
            };
            var result = method.Invoke(null, new object[] { pts });
            Assert.That(result, Is.EqualTo("1,2 3,4 5,6"));
        }

        [Test]
        public void PointsToStr_1点のみは末尾空白なし()
        {
            var asm = typeof(ObjectSerializer).Assembly;
            var t = asm.GetType("boilersGraphics.Helpers.ObjectSerializer");
            var method = t.GetMethod("PointsToStr",
                BindingFlags.NonPublic | BindingFlags.Static);

            var pts = new ObservableCollection<Point> { new Point(7, 8) };
            var result = method.Invoke(null, new object[] { pts });
            Assert.That(result, Is.EqualTo("7,8"));
        }
    }
}
