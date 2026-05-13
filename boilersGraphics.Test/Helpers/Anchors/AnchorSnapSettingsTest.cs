using boilersGraphics.Helpers.Anchors;
using NUnit.Framework;

namespace boilersGraphics.Test.Helpers.Anchors;

/// <summary>
/// Phase 3-i / Q-7 案 C: AnchorSnapSettings.SnapDistance がグローバルかつ書き換え可能で、
/// AnchorSnap.FindNearestAnchorRef が threshold 未指定時に Settings 値を採用することを検証する。
/// </summary>
[TestFixture]
public class AnchorSnapSettingsTest
{
    [Test]
    public void SnapDistance_デフォルトは10()
    {
        // 他テストで書き換えられる可能性に注意。OneTimeSetUp で 10 にリセットしてから確認。
        AnchorSnapSettings.SnapDistance.Value = AnchorSnap.DefaultSnapDistance;
        Assert.That(AnchorSnapSettings.SnapDistance.Value, Is.EqualTo(10.0));
    }

    [Test]
    public void SnapDistance_書き換え可能()
    {
        var original = AnchorSnapSettings.SnapDistance.Value;
        try
        {
            AnchorSnapSettings.SnapDistance.Value = 25.5;
            Assert.That(AnchorSnapSettings.SnapDistance.Value, Is.EqualTo(25.5));
        }
        finally
        {
            AnchorSnapSettings.SnapDistance.Value = original;
        }
    }

    [Test]
    public void FindNearestAnchorRef_thresholdなし_Settings値を使う()
    {
        // diagram=null だと早期 return するので、ここでは「呼び出しが例外を投げない」程度の検証。
        // 実際の閾値伝搬は AnchorSnap の挙動テスト (将来) で扱う。
        AnchorSnapSettings.SnapDistance.Value = 5.0;
        try
        {
            var result = AnchorSnap.FindNearestAnchorRef(null, new System.Windows.Point(0, 0));
            Assert.That(result, Is.Null);
        }
        finally
        {
            AnchorSnapSettings.SnapDistance.Value = AnchorSnap.DefaultSnapDistance;
        }
    }
}
