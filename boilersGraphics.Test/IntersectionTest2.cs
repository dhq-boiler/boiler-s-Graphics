using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System.Threading;
using System.Windows;

namespace boilersGraphics.Test
{
    // NOTE: 既存 IntersectionTest.cs があるかもしれないため別名にしておく
    [TestFixture]
    public class IntersectionMathTest
    {
        // 本クラスの本番上のバグ防止意図:
        // - 楕円 ↔ 線分の交点計算はスナップポイント検出 (吸着) の中核。
        //   バグるとペン先が貼り付かない / 妙な所に吸着するというユーザー体験
        //   直結のバグになる。回転対応版 (SupportRotation) が壊れると、
        //   傾いた楕円がスナップ対象として機能しなくなる。
        //
        // - 戻り値: Tuple<Point[], double>
        //   * Point[] : 交点の集合 (0/1/2 個)
        //   * double  : 判別式 (>0=2交点, ≈0=接線, <0=交点なし, NaN=空入力)

        private static NEllipseViewModel NewEllipse(double left, double top, double width, double height,
                                                   double rotationAngle = 0)
        {
            App.IsTest = true;
            var e = new NEllipseViewModel();
            e.Left.Value = left;
            e.Top.Value = top;
            e.Width.Value = width;
            e.Height.Value = height;
            e.RotationAngle.Value = rotationAngle;
            return e;
        }

        // ---- FindEllipseSegmentIntersections (回転なし版) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_中心を貫く水平線は左右2つの交点を返す()
        {
            // 楕円: Left=0, Top=0, W=200, H=100  → 中心 (100, 50), a=100, b=50
            // 線分: y=50 で水平 (-100, 50) → (300, 50)
            // 期待: 交点 (0, 50) と (200, 50)
            var e = NewEllipse(0, 0, 200, 100);
            var result = Intersection.FindEllipseSegmentIntersections(
                e, new Point(-100, 50), new Point(300, 50), segment_only: true);

            Assert.That(result.Item2, Is.GreaterThan(0), "判別式は正 (2 交点)");
            Assert.That(result.Item1.Length, Is.EqualTo(2));

            var xs = new[] { result.Item1[0].X, result.Item1[1].X };
            System.Array.Sort(xs);
            Assert.That(xs[0], Is.EqualTo(0).Within(1e-6));
            Assert.That(xs[1], Is.EqualTo(200).Within(1e-6));
            Assert.That(result.Item1[0].Y, Is.EqualTo(50).Within(1e-6));
            Assert.That(result.Item1[1].Y, Is.EqualTo(50).Within(1e-6));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_中心を貫く垂直線は上下2つの交点を返す()
        {
            // 楕円: Left=0, Top=0, W=200, H=100, 中心 (100, 50)
            // 線分: x=100 で垂直 (100, -100) → (100, 200)
            // 期待: (100, 0), (100, 100)
            var e = NewEllipse(0, 0, 200, 100);
            var result = Intersection.FindEllipseSegmentIntersections(
                e, new Point(100, -100), new Point(100, 200), segment_only: true);

            Assert.That(result.Item2, Is.GreaterThan(0));
            Assert.That(result.Item1.Length, Is.EqualTo(2));

            var ys = new[] { result.Item1[0].Y, result.Item1[1].Y };
            System.Array.Sort(ys);
            Assert.That(ys[0], Is.EqualTo(0).Within(1e-6));
            Assert.That(ys[1], Is.EqualTo(100).Within(1e-6));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_遠く離れた線分は交点を返さない()
        {
            // 楕円中心 (100, 50), 線分は (1000, 1000) → (2000, 2000) で楕円から遠い
            var e = NewEllipse(0, 0, 200, 100);
            var result = Intersection.FindEllipseSegmentIntersections(
                e, new Point(1000, 1000), new Point(2000, 2000), segment_only: true);

            // 判別式 < 0 でも > 0 でも、segment_only=true なら線分内交点 0
            Assert.That(result.Item1.Length, Is.EqualTo(0));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_segment_only_falseなら延長線上の交点も返す()
        {
            // 楕円: 中心 (100, 50), W=200, H=100
            // 線分: y=50 で楕円より十分右側に離す (1000, 50) → (1100, 50)
            // この線分の延長線 (= 楕円中心を通る水平線) は (0, 50), (200, 50) で交差。
            // ただし t は負値となり線分パラメータ範囲 [0,1] 外。
            var e = NewEllipse(0, 0, 200, 100);

            var resultStrict = Intersection.FindEllipseSegmentIntersections(
                e, new Point(1000, 50), new Point(1100, 50), segment_only: true);
            // strict: t が範囲外なので空配列
            Assert.That(resultStrict.Item1.Length, Is.EqualTo(0));
            Assert.That(resultStrict.Item2, Is.GreaterThan(0), "判別式自体は正 (延長線は楕円を貫通)");

            var resultLoose = Intersection.FindEllipseSegmentIntersections(
                e, new Point(1000, 50), new Point(1100, 50), segment_only: false);
            // loose: 延長線上の 2 交点を返す
            Assert.That(resultLoose.Item1.Length, Is.EqualTo(2));

            // 戻り値座標は (0, 50) と (200, 50) になっているはず
            var xs = new[] { resultLoose.Item1[0].X, resultLoose.Item1[1].X };
            System.Array.Sort(xs);
            Assert.That(xs[0], Is.EqualTo(0).Within(1e-3));
            Assert.That(xs[1], Is.EqualTo(200).Within(1e-3));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_Widthが0なら空配列とNaN()
        {
            var e = NewEllipse(0, 0, 0, 100);
            var result = Intersection.FindEllipseSegmentIntersections(
                e, new Point(-100, 50), new Point(300, 50), segment_only: true);
            Assert.That(result.Item1.Length, Is.EqualTo(0));
            Assert.That(double.IsNaN(result.Item2), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_Heightが0なら空配列とNaN()
        {
            var e = NewEllipse(0, 0, 200, 0);
            var result = Intersection.FindEllipseSegmentIntersections(
                e, new Point(0, -100), new Point(0, 100), segment_only: true);
            Assert.That(result.Item1.Length, Is.EqualTo(0));
            Assert.That(double.IsNaN(result.Item2), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_線分の始点と終点が同一なら空配列とNaN()
        {
            var e = NewEllipse(0, 0, 200, 100);
            var pt = new Point(50, 50);
            var result = Intersection.FindEllipseSegmentIntersections(
                e, pt, pt, segment_only: true);
            Assert.That(result.Item1.Length, Is.EqualTo(0));
            Assert.That(double.IsNaN(result.Item2), Is.True);
        }

        // ---- FindEllipseSegmentIntersectionsSupportRotation ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_回転0度は非回転版と同じ結果()
        {
            var e = NewEllipse(0, 0, 200, 100, rotationAngle: 0);

            var noRot = Intersection.FindEllipseSegmentIntersections(
                e, new Point(-100, 50), new Point(300, 50), segment_only: true);
            var withRot = Intersection.FindEllipseSegmentIntersectionsSupportRotation(
                e, new Point(-100, 50), new Point(300, 50), segment_only: true);

            Assert.That(withRot.Item1.Length, Is.EqualTo(noRot.Item1.Length));
            Assert.That(withRot.Item2, Is.EqualTo(noRot.Item2).Within(1e-6));

            // 同じ交点が返る (順序は問わない)
            var noRotXs = new[] { noRot.Item1[0].X, noRot.Item1[1].X };
            var withRotXs = new[] { withRot.Item1[0].X, withRot.Item1[1].X };
            System.Array.Sort(noRotXs);
            System.Array.Sort(withRotXs);
            Assert.That(withRotXs[0], Is.EqualTo(noRotXs[0]).Within(1e-6));
            Assert.That(withRotXs[1], Is.EqualTo(noRotXs[1]).Within(1e-6));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_回転90度_横長楕円は縦長になる()
        {
            // 楕円: 中心 (100, 50), W=200, H=100 (横長)
            // 90度回転すると見た目は中心 (100, 50) で W=100, H=200 の縦長と同等
            // → 中心を通る垂直線 (x=100, y=-100→200) は y=-50 と y=150 で交差するはず
            //   (元の左右の交点 (0,50) (200,50) が 90度回転して (100, 150) (100, -50))
            var e = NewEllipse(0, 0, 200, 100, rotationAngle: 90);

            var result = Intersection.FindEllipseSegmentIntersectionsSupportRotation(
                e, new Point(100, -100), new Point(100, 200), segment_only: true);

            Assert.That(result.Item2, Is.GreaterThan(0));
            Assert.That(result.Item1.Length, Is.EqualTo(2));

            var ys = new[] { result.Item1[0].Y, result.Item1[1].Y };
            System.Array.Sort(ys);
            // 元の楕円 a=100 軸が 90度回転すると Y 方向に伸びる
            // y = 50 ± 100 で (-50) と (150)
            Assert.That(ys[0], Is.EqualTo(-50).Within(1e-3));
            Assert.That(ys[1], Is.EqualTo(150).Within(1e-3));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_回転版_Widthが0なら空配列とNaN()
        {
            var e = NewEllipse(0, 0, 0, 100, rotationAngle: 30);
            var result = Intersection.FindEllipseSegmentIntersectionsSupportRotation(
                e, new Point(-100, 50), new Point(300, 50), segment_only: true);
            Assert.That(result.Item1.Length, Is.EqualTo(0));
            Assert.That(double.IsNaN(result.Item2), Is.True);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void Ellipse_回転版_同一点線分なら空配列とNaN()
        {
            var e = NewEllipse(0, 0, 200, 100, rotationAngle: 45);
            var pt = new Point(50, 50);
            var result = Intersection.FindEllipseSegmentIntersectionsSupportRotation(
                e, pt, pt, segment_only: true);
            Assert.That(result.Item1.Length, Is.EqualTo(0));
            Assert.That(double.IsNaN(result.Item2), Is.True);
        }
    }
}
