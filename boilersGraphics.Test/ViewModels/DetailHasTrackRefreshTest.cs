using boilersGraphics.Models.Animation;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Animation;
using NUnit.Framework;
using R3;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Test.ViewModels;

[TestFixture]
public class DetailHasTrackRefreshTest
{
    /// <summary>
    /// PropertyOptionsValueCombination は abstract なのでテスト用 dummy 派生を用意し、
    /// HasTrack の更新だけ検証する。Object / PropertyValue は不要。
    /// </summary>
    private class FakeRow : PropertyOptionsValueCombination
    {
        public FakeRow(string name) : base(name) { }
    }

    private static readonly Guid ItemA = Guid.NewGuid();
    private static readonly Guid ItemB = Guid.NewGuid();

    private static AnimationTrack TrackFor(Guid id, string path)
    {
        var t = new AnimationTrack(new PropertyRef(id, path, AnimatedValueType.Double));
        t.Keyframes.Add(new Keyframe(0.0, 1.0, EasingKind.LinearEase, EasingMode.EaseIn));
        return t;
    }

    [Test]
    public void Refresh_Tracks空_は全てHasTrack_false()
    {
        var tl = new TimelineViewModel(5.0, 30);
        var rows = new List<PropertyOptionsValueCombination>
        {
            new FakeRow("Left.Value"),
            new FakeRow("Top.Value"),
        };

        DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, tl);

        Assert.That(rows[0].HasTrack.Value, Is.False);
        Assert.That(rows[1].HasTrack.Value, Is.False);
    }

    [Test]
    public void Refresh_該当Track有_該当行のみtrue()
    {
        var tl = new TimelineViewModel(5.0, 30);
        tl.Tracks.Add(TrackFor(ItemA, "Left.Value"));
        var rows = new List<PropertyOptionsValueCombination>
        {
            new FakeRow("Left.Value"),
            new FakeRow("Top.Value"),
        };

        DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, tl);

        Assert.That(rows[0].HasTrack.Value, Is.True);
        Assert.That(rows[1].HasTrack.Value, Is.False);
    }

    [Test]
    public void Refresh_別ItemのTrack_は該当行false()
    {
        var tl = new TimelineViewModel(5.0, 30);
        tl.Tracks.Add(TrackFor(ItemB, "Left.Value"));
        var rows = new List<PropertyOptionsValueCombination>
        {
            new FakeRow("Left.Value"),
        };

        DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, tl);

        Assert.That(rows[0].HasTrack.Value, Is.False);
    }

    [Test]
    public void Refresh_複数Track_全行正しく分類()
    {
        var tl = new TimelineViewModel(5.0, 30);
        tl.Tracks.Add(TrackFor(ItemA, "Left.Value"));
        tl.Tracks.Add(TrackFor(ItemA, "Width.Value"));
        tl.Tracks.Add(TrackFor(ItemB, "Top.Value")); // 別 item は無視
        var rows = new List<PropertyOptionsValueCombination>
        {
            new FakeRow("Left.Value"),
            new FakeRow("Top.Value"),
            new FakeRow("Width.Value"),
            new FakeRow("Height.Value"),
        };

        DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, tl);

        Assert.That(rows[0].HasTrack.Value, Is.True, "Left");
        Assert.That(rows[1].HasTrack.Value, Is.False, "Top (別 item の Track のみ)");
        Assert.That(rows[2].HasTrack.Value, Is.True, "Width");
        Assert.That(rows[3].HasTrack.Value, Is.False, "Height");
    }

    [Test]
    public void Refresh_HasTrack_は_状態変化に追随()
    {
        var tl = new TimelineViewModel(5.0, 30);
        var rows = new List<PropertyOptionsValueCombination> { new FakeRow("Left.Value") };

        DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, tl);
        Assert.That(rows[0].HasTrack.Value, Is.False);

        tl.Tracks.Add(TrackFor(ItemA, "Left.Value"));
        DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, tl);
        Assert.That(rows[0].HasTrack.Value, Is.True);

        tl.Tracks.Clear();
        DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, tl);
        Assert.That(rows[0].HasTrack.Value, Is.False);
    }

    [Test]
    public void Refresh_rows_null_は_例外なし()
    {
        var tl = new TimelineViewModel(5.0, 30);
        Assert.DoesNotThrow(() => DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, null, tl));
    }

    [Test]
    public void Refresh_timeline_null_は_例外なし()
    {
        var rows = new List<PropertyOptionsValueCombination> { new FakeRow("Left.Value") };
        Assert.DoesNotThrow(() => DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, null));
    }

    [Test]
    public void Refresh_PropertyName_null_or_empty_の行_は_false()
    {
        var tl = new TimelineViewModel(5.0, 30);
        tl.Tracks.Add(TrackFor(ItemA, ""));
        var rows = new List<PropertyOptionsValueCombination>
        {
            new FakeRow(null),
            new FakeRow(""),
        };

        DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, tl);

        Assert.That(rows[0].HasTrack.Value, Is.False);
        Assert.That(rows[1].HasTrack.Value, Is.False);
    }

    [Test]
    public void Refresh_rows内にnull混入_は_スキップして例外なし()
    {
        var tl = new TimelineViewModel(5.0, 30);
        tl.Tracks.Add(TrackFor(ItemA, "Left.Value"));
        var rows = new List<PropertyOptionsValueCombination>
        {
            null,
            new FakeRow("Left.Value"),
        };

        Assert.DoesNotThrow(() => DetailViewModelBase<SelectableDesignerItemViewModelBase>
            .RefreshHasTrack(ItemA, rows, tl));
        Assert.That(rows[1].HasTrack.Value, Is.True);
    }
}
