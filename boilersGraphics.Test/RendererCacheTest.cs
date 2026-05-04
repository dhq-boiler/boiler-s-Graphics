using boilersGraphics.Helpers;
using NUnit.Framework;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class RendererCacheTest
    {
        [Test]
        public void TryGetDrawingVisual_未登録ならfalse()
        {
            var cache = new RendererCache();
            Assert.That(cache.TryGetDrawingVisual("k", out var cached), Is.False);
            Assert.That(cached, Is.Null);
        }

        [Test]
        public void AddDrawingVisualしたあとTryGetDrawingVisualで取得できる()
        {
            var cache = new RendererCache();
            var cdv = new CachedDrawingVisual();
            cache.AddDrawingVisual("k", cdv);
            Assert.That(cache.TryGetDrawingVisual("k", out var cached), Is.True);
            Assert.That(cached, Is.SameAs(cdv));
        }

        [Test]
        public void AddDrawingVisual_同一キーは上書き()
        {
            var cache = new RendererCache();
            var first = new CachedDrawingVisual();
            var second = new CachedDrawingVisual();
            cache.AddDrawingVisual("k", first);
            cache.AddDrawingVisual("k", second);
            cache.TryGetDrawingVisual("k", out var cached);
            Assert.That(cached, Is.SameAs(second));
        }

        [Test]
        public void RemoveDrawingVisual_存在すればtrueを返しDisposeも呼ばれる()
        {
            var cache = new RendererCache();
            var cdv = new CachedDrawingVisual();
            cache.AddDrawingVisual("k", cdv);
            Assert.That(cache.RemoveDrawingVisual("k"), Is.True);
            Assert.That(cache.TryGetDrawingVisual("k", out _), Is.False);
        }

        [Test]
        public void RemoveDrawingVisual_存在しなければfalse()
        {
            var cache = new RendererCache();
            Assert.That(cache.RemoveDrawingVisual("missing"), Is.False);
        }

        [Test]
        public void IsDirty_MarkItemDirtyで真になる()
        {
            var cache = new RendererCache();
            var item = new object();
            Assert.That(cache.IsDirty(item), Is.False);
            cache.MarkItemDirty(item);
            Assert.That(cache.IsDirty(item), Is.True);
        }

        [Test]
        public void MarkItemDirty_nullは無視()
        {
            var cache = new RendererCache();
            Assert.That(() => cache.MarkItemDirty(null), Throws.Nothing);
        }

        [Test]
        public void ClearDirtyFlagでフラグが消える()
        {
            var cache = new RendererCache();
            var item = new object();
            cache.MarkItemDirty(item);
            cache.ClearDirtyFlag(item);
            Assert.That(cache.IsDirty(item), Is.False);
        }

        [Test]
        public void InvalidateDrawingVisualCache_nullで全クリアし統計も0()
        {
            var cache = new RendererCache();
            cache.AddDrawingVisual("a", new CachedDrawingVisual());
            cache.AddDrawingVisual("b", new CachedDrawingVisual());
            cache.MarkItemDirty(new object());
            cache.InvalidateDrawingVisualCache(null);
            var stats = cache.GetStatistics(0);
            Assert.That(stats.DrawingVisualCacheCount, Is.EqualTo(0));
            Assert.That(stats.DirtyItemsCount, Is.EqualTo(0));
        }

        [Test]
        public void InvalidateDrawingVisualCache_引数指定で個別ダーティマーク()
        {
            var cache = new RendererCache();
            var item = new object();
            cache.InvalidateDrawingVisualCache(item);
            Assert.That(cache.IsDirty(item), Is.True);
        }

        [Test]
        public void ClearAll_ドローイング_ダーティ_LastModifiedすべてクリア()
        {
            var cache = new RendererCache();
            cache.AddDrawingVisual("a", new CachedDrawingVisual());
            cache.MarkItemDirty(new object());
            cache.ClearAll();
            var stats = cache.GetStatistics(0);
            Assert.That(stats.DrawingVisualCacheCount, Is.EqualTo(0));
            Assert.That(stats.DirtyItemsCount, Is.EqualTo(0));
            Assert.That(stats.LastModifiedItem, Is.Null);
        }

        [Test]
        public void GetStatistics_引数のViewCacheCountを返す()
        {
            var cache = new RendererCache();
            cache.AddDrawingVisual("a", new CachedDrawingVisual());
            cache.MarkItemDirty(new object());
            var stats = cache.GetStatistics(7);
            Assert.That(stats.DrawingVisualCacheCount, Is.EqualTo(1));
            Assert.That(stats.DirtyItemsCount, Is.EqualTo(1));
            Assert.That(stats.ViewCacheCount, Is.EqualTo(7));
        }

        [Test]
        public void GetStatistics_LastModifiedItemは型名()
        {
            var cache = new RendererCache();
            cache.MarkItemDirty("hello"); // string
            var stats = cache.GetStatistics(0);
            Assert.That(stats.LastModifiedItem, Is.EqualTo("String"));
        }

        [Test]
        public void CacheStatistics_ToStringはサマリ文字列()
        {
            var s = new CacheStatistics
            {
                DrawingVisualCacheCount = 3,
                VisualBrushCacheCount = 2,
                DirtyItemsCount = 1,
                ViewCacheCount = 5,
                LastModifiedItem = "Foo"
            };
            var str = s.ToString();
            Assert.That(str, Does.Contain("3").And.Contain("2").And.Contain("1").And.Contain("5").And.Contain("Foo"));
        }

        [Test]
        public void CacheStatistics_LastModifiedNullはNoneと表示()
        {
            var s = new CacheStatistics();
            Assert.That(s.ToString(), Does.Contain("None"));
        }

        [Test]
        public void CachedDrawingVisual_Disposeを2回呼んでも例外なし()
        {
            var cdv = new CachedDrawingVisual();
            Assert.That(() => { cdv.Dispose(); cdv.Dispose(); }, Throws.Nothing);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetOrCreateVisualBrush_同一viewでは同じインスタンスを返す()
        {
            var cache = new RendererCache();
            var view = new Border();
            var brush1 = cache.GetOrCreateVisualBrush(view);
            var brush2 = cache.GetOrCreateVisualBrush(view);
            Assert.That(brush2, Is.SameAs(brush1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void GetOrCreateVisualBrush_別viewなら別インスタンス()
        {
            var cache = new RendererCache();
            var v1 = new Border();
            var v2 = new Border();
            var b1 = cache.GetOrCreateVisualBrush(v1);
            var b2 = cache.GetOrCreateVisualBrush(v2);
            Assert.That(b2, Is.Not.SameAs(b1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ClearVisualBrushCache_キャッシュをクリアして次回は新規作成()
        {
            var cache = new RendererCache();
            var view = new Border();
            var b1 = cache.GetOrCreateVisualBrush(view);
            cache.ClearVisualBrushCache();
            var b2 = cache.GetOrCreateVisualBrush(view);
            Assert.That(b2, Is.Not.SameAs(b1));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void ClearAll_VisualBrushCacheもクリア()
        {
            var cache = new RendererCache();
            var view = new Border();
            cache.GetOrCreateVisualBrush(view);
            var stats1 = cache.GetStatistics(0);
            Assert.That(stats1.VisualBrushCacheCount, Is.EqualTo(1));
            cache.ClearAll();
            var stats2 = cache.GetStatistics(0);
            Assert.That(stats2.VisualBrushCacheCount, Is.EqualTo(0));
        }
    }
}
