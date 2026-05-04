using NUnit.Framework;
using System;
using System.Reflection;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class GeometryCombinerTest
    {
        // NOTE: GeometryCombiner.Connect は現在どこからも呼ばれていない dead code。
        // 加えて Interpret は "M 0,0 L 10,10" のような空白区切り文字列を期待するが、
        // PathGeometry.ToString() は "F1M0,0L10,10" のようなコンパクト形式を返すため、
        // 通常入力では Interpret の foreach がトークンを 1 つも追加せず、最初の
        // figures.Last() で InvalidOperationException ("Sequence contains no elements")
        // を投げる。
        //
        // テスト対象としては内部 Figure / Line / BezierLine の Keyword と ToString
        // のみ書く。Connect / Interpret 自体は呼び出し元がないため一旦未テスト扱い。
        // 削除を提案する PR を別途立てたい。

        // ---- Line ----

        [Test]
        public void Line_Keywordは大文字L()
        {
            var asm = typeof(boilersGraphics.Helpers.ClipboardDTO).Assembly;
            var t = asm.GetType("boilersGraphics.Helpers.Line");
            Assert.That(t, Is.Not.Null);
            var method = t.GetMethod("Keyword", BindingFlags.Public | BindingFlags.Static);
            Assert.That(method.Invoke(null, null), Is.EqualTo("L"));
        }

        [Test]
        public void Line_ToString_LプレフィックスとPoint2を含む()
        {
            var asm = typeof(boilersGraphics.Helpers.ClipboardDTO).Assembly;
            var t = asm.GetType("boilersGraphics.Helpers.Line");
            var instance = Activator.CreateInstance(t, nonPublic: true);
            var figureType = asm.GetType("boilersGraphics.Helpers.Figure");
            figureType.GetProperty("Point2").SetValue(instance, new System.Windows.Point(3, 4));

            var s = instance.ToString();
            Assert.That(s, Does.StartWith("L "));
            Assert.That(s, Does.Contain("3"));
            Assert.That(s, Does.Contain("4"));
        }

        [Test]
        public void Line_Point1とPoint2を独立して保持()
        {
            var asm = typeof(boilersGraphics.Helpers.ClipboardDTO).Assembly;
            var t = asm.GetType("boilersGraphics.Helpers.Line");
            var instance = Activator.CreateInstance(t, nonPublic: true);
            var figureType = asm.GetType("boilersGraphics.Helpers.Figure");
            var p1 = new System.Windows.Point(1, 2);
            var p2 = new System.Windows.Point(3, 4);
            figureType.GetProperty("Point1").SetValue(instance, p1);
            figureType.GetProperty("Point2").SetValue(instance, p2);

            Assert.That(figureType.GetProperty("Point1").GetValue(instance), Is.EqualTo(p1));
            Assert.That(figureType.GetProperty("Point2").GetValue(instance), Is.EqualTo(p2));
        }

        // ---- BezierLine ----

        [Test]
        public void BezierLine_Keywordは大文字C()
        {
            var asm = typeof(boilersGraphics.Helpers.ClipboardDTO).Assembly;
            var t = asm.GetType("boilersGraphics.Helpers.BezierLine");
            Assert.That(t, Is.Not.Null);
            var method = t.GetMethod("Keyword", BindingFlags.Public | BindingFlags.Static);
            Assert.That(method.Invoke(null, null), Is.EqualTo("C"));
        }

        [Test]
        public void BezierLine_ToString_CプレフィックスとControlPoints_Point2()
        {
            var asm = typeof(boilersGraphics.Helpers.ClipboardDTO).Assembly;
            var t = asm.GetType("boilersGraphics.Helpers.BezierLine");
            var instance = Activator.CreateInstance(t, nonPublic: true);
            t.GetProperty("ControlPoint1").SetValue(instance, new System.Windows.Point(1, 2));
            t.GetProperty("ControlPoint2").SetValue(instance, new System.Windows.Point(3, 4));
            var figureType = asm.GetType("boilersGraphics.Helpers.Figure");
            figureType.GetProperty("Point2").SetValue(instance, new System.Windows.Point(5, 6));

            var s = instance.ToString();
            Assert.That(s, Does.StartWith("C "));
            Assert.That(s, Does.Contain("1"));
            Assert.That(s, Does.Contain("2"));
            Assert.That(s, Does.Contain("3"));
            Assert.That(s, Does.Contain("4"));
            Assert.That(s, Does.Contain("5"));
            Assert.That(s, Does.Contain("6"));
        }

        [Test]
        public void BezierLine_ControlPoint1_2を独立して保持()
        {
            var asm = typeof(boilersGraphics.Helpers.ClipboardDTO).Assembly;
            var t = asm.GetType("boilersGraphics.Helpers.BezierLine");
            var instance = Activator.CreateInstance(t, nonPublic: true);
            var cp1 = new System.Windows.Point(10, 20);
            var cp2 = new System.Windows.Point(30, 40);
            t.GetProperty("ControlPoint1").SetValue(instance, cp1);
            t.GetProperty("ControlPoint2").SetValue(instance, cp2);
            Assert.That(t.GetProperty("ControlPoint1").GetValue(instance), Is.EqualTo(cp1));
            Assert.That(t.GetProperty("ControlPoint2").GetValue(instance), Is.EqualTo(cp2));
        }
    }
}
