using boilersGraphics.Helpers;
using boilersGraphics.ViewModels.ColorCorrect;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class CurveTest
    {
        // 本クラスの本番上のバグ防止意図:
        // - トーンカーブの InOutPair 計算 (色補正テーブル) が壊れると、
        //   画像処理の出力色が崩れる重大バグになる。
        // - 想定する契約:
        //   * x が BezierSegment の [P0.X, P3.X] 内に入る場合のみ pair を生成。
        //   * y は [0, 255] にクランプされた値だけ採用。
        //   * 同じ x が複数 segment に出現してもペアは 1 件だけ追加 (先勝ち)。
        //   * 直線的なベジエ (P0=(0,0), P1=(85,85), P2=(170,170), P3=(255,255)) は
        //     恒等変換に近い (y ≈ x) を返す。

        private static (PathGeometry pg, PathSegmentCollection segs) BuildBezier(
            Point start, Point p1, Point p2, Point p3)
        {
            var fig = new PathFigure { StartPoint = start };
            fig.Segments.Add(new BezierSegment(p1, p2, p3, true));
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return (pg, fig.Segments);
        }

        private static (PathGeometry pg, PathSegmentCollection segs) BuildBezierChain(
            Point start, params (Point p1, Point p2, Point p3)[] beziers)
        {
            var fig = new PathFigure { StartPoint = start };
            foreach (var (p1, p2, p3) in beziers)
                fig.Segments.Add(new BezierSegment(p1, p2, p3, true));
            var pg = new PathGeometry();
            pg.Figures.Add(fig);
            return (pg, fig.Segments);
        }

        // ---- 単調変換 (恒等) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CalcInOutPairs_直線的ベジエは恒等変換に近い()
        {
            // P0 = (0,0), P1 = (85,85), P2 = (170,170), P3 = (255,255)
            // → 完全な直線、y = x
            var begin = new ToneCurveViewModel.Point(0, 0);
            var (pg, segs) = BuildBezier(
                start: new Point(0, 0),
                p1: new Point(85, 85),
                p2: new Point(170, 170),
                p3: new Point(255, 255));

            var pairs = Curve.CalcInOutPairs(pg, segs, begin);

            Assert.That(pairs.Count, Is.EqualTo(256));
            // 各 input に対して output ≈ input (Math.Round により ±1 程度の誤差)
            foreach (var p in pairs)
            {
                Assert.That(p.Out, Is.EqualTo(p.In).Within(1),
                    $"x={p.In} で y={p.Out} は恒等変換 ±1 範囲外");
            }
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CalcInOutPairs_直線的ベジエ_両端は厳密に一致()
        {
            var begin = new ToneCurveViewModel.Point(0, 0);
            var (pg, segs) = BuildBezier(
                start: new Point(0, 0),
                p1: new Point(85, 85),
                p2: new Point(170, 170),
                p3: new Point(255, 255));

            var pairs = Curve.CalcInOutPairs(pg, segs, begin);

            var first = pairs.First(p => p.In == 0);
            var last = pairs.First(p => p.In == 255);
            Assert.That(first.Out, Is.EqualTo(0));
            Assert.That(last.Out, Is.EqualTo(255));
        }

        // ---- 反転 (ネガ変換) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CalcInOutPairs_反転ベジエは出力が反転する()
        {
            // 実装は P0 = beginPoint.ToPoint() を使うので、beginPoint=(0,255) で
            // (P0=0,255) → (P3=255,0) の反転カーブを作る。
            var begin = new ToneCurveViewModel.Point(0, 255);
            var (pg, segs) = BuildBezier(
                start: new Point(0, 255),
                p1: new Point(85, 170),
                p2: new Point(170, 85),
                p3: new Point(255, 0));

            var pairs = Curve.CalcInOutPairs(pg, segs, begin);

            Assert.That(pairs.Count, Is.EqualTo(256));
            foreach (var p in pairs)
            {
                // y ≈ 255 - x
                Assert.That(p.Out, Is.EqualTo(255 - p.In).Within(1),
                    $"x={p.In} で y={p.Out} は反転 ±1 範囲外 (期待 ≈ {255 - p.In})");
            }
        }

        // ---- 範囲外スキップ ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CalcInOutPairs_xがP0XとP3Xの範囲外なら結果に追加されない()
        {
            // ベジエは [50, 100] 範囲のみ
            var begin = new ToneCurveViewModel.Point(50, 100);
            var (pg, segs) = BuildBezier(
                start: new Point(50, 100),
                p1: new Point(70, 100),
                p2: new Point(80, 100),
                p3: new Point(100, 100));

            var pairs = Curve.CalcInOutPairs(pg, segs, begin);

            // 50..100 だけ pair が作られる
            Assert.That(pairs.All(p => p.In >= 50 && p.In <= 100), Is.True,
                "x ∈ [50,100] 範囲外の pair が紛れ込んでいる");
            Assert.That(pairs.Any(p => p.In < 50), Is.False);
            Assert.That(pairs.Any(p => p.In > 100), Is.False);
        }

        // ---- 出力範囲外クランプ ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CalcInOutPairs_出力yが範囲外ならその点はスキップ()
        {
            // y を 256 以上に持っていくベジエ: 制御点を 300 にする
            // begin=(0,300) → 出力 y > 255 が多発し、対応する pair はスキップ
            var begin = new ToneCurveViewModel.Point(0, 300);
            var (pg, segs) = BuildBezier(
                start: new Point(0, 300),
                p1: new Point(85, 300),
                p2: new Point(170, 300),
                p3: new Point(255, 300));

            var pairs = Curve.CalcInOutPairs(pg, segs, begin);

            // 出力 y はすべて > 255 になるので pair は全くできない
            Assert.That(pairs, Is.Empty);
        }

        // ---- 重複 x はスキップ (先勝ち) ----

        [Test, RequiresThread(ApartmentState.STA)]
        public void CalcInOutPairs_重複xは最初の1件だけ追加()
        {
            // 2 つの BezierSegment が x=128 でオーバーラップ
            // 1 個目: (0,0) → (128,128)
            // 2 個目: (128,128) → (255,255) (連続した直線)
            // 内部 ループは _myPathSegmentCollection の OfType<BezierSegment>() で回す。
            // x=128 は両 segment の境界に入るので、最初の segment の値が採用され
            // 2 つ目は ret.Any(a => a.In == x) で skip される。
            var begin = new ToneCurveViewModel.Point(0, 0);
            var (pg, segs) = BuildBezierChain(
                start: new Point(0, 0),
                (new Point(43, 43), new Point(85, 85), new Point(128, 128)),
                (new Point(170, 170), new Point(212, 212), new Point(255, 255)));

            var pairs = Curve.CalcInOutPairs(pg, segs, begin);

            // 各 x は 1 度しか出現しない
            var inSet = pairs.Select(p => p.In).ToList();
            Assert.That(inSet.Distinct().Count(), Is.EqualTo(inSet.Count),
                "同じ x を持つ pair が複数回追加されている");
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void CalcInOutPairs_2セグメント連結は各セグメントの担当範囲をカバー()
        {
            var begin = new ToneCurveViewModel.Point(0, 0);
            var (pg, segs) = BuildBezierChain(
                start: new Point(0, 0),
                (new Point(43, 43), new Point(85, 85), new Point(128, 128)),
                (new Point(170, 170), new Point(212, 212), new Point(255, 255)));

            var pairs = Curve.CalcInOutPairs(pg, segs, begin);

            // x=0, 64, 128, 192, 255 すべて結果に含まれる
            Assert.That(pairs.Any(p => p.In == 0), Is.True);
            Assert.That(pairs.Any(p => p.In == 64), Is.True);
            Assert.That(pairs.Any(p => p.In == 128), Is.True);
            Assert.That(pairs.Any(p => p.In == 192), Is.True);
            Assert.That(pairs.Any(p => p.In == 255), Is.True);

            // 出力も恒等に近い
            var p64 = pairs.First(p => p.In == 64);
            var p192 = pairs.First(p => p.In == 192);
            Assert.That(p64.Out, Is.EqualTo(64).Within(2));
            Assert.That(p192.Out, Is.EqualTo(192).Within(2));
        }

        // ---- LandmarkControl オーバーロードは委譲だけなのでスキップ ----
        // (LandmarkControl は WPF UserControl で構築コスト高、
        //  実装は landmarkControl.InOutPairs を返すだけなのでテスト価値低い)
    }
}
