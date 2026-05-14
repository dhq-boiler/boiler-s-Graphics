using boilersGraphics.Helpers.Anchors;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using boilersGraphics.ViewModels.Connectors;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.Helpers.Anchors;

/// <summary>
/// Phase 3.5 / Q-8 案 B: AnchorReferenceFinder の pure ロジックを検証する。
/// AnchorViewModel.Id を明示参照しているコネクタの抽出のみ。暗黙 9 点参照は対象外。
/// </summary>
[TestFixture]
public class AnchorReferenceFinderTest
{
    [Test, Apartment(ApartmentState.STA)]
    public void OrthogonalのBeginAnchorRefが一致したら拾う()
    {
        var anchor = new AnchorViewModel();
        var oc = new OrthogonalConnectorViewModel();
        oc.BeginAnchorRef.Value = anchor.ID.ToString();
        var result = AnchorReferenceFinder.FindReferring(new SelectableDesignerItemViewModelBase[] { oc }, anchor.ID).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.SameAs(oc));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void OrthogonalのEndAnchorRefが一致したら拾う()
    {
        var anchor = new AnchorViewModel();
        var oc = new OrthogonalConnectorViewModel();
        oc.EndAnchorRef.Value = anchor.ID.ToString();
        var result = AnchorReferenceFinder.FindReferring(new SelectableDesignerItemViewModelBase[] { oc }, anchor.ID).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void AnchorBezierでも同様に拾う()
    {
        var anchor = new AnchorViewModel();
        var ab = new AnchorBezierConnectorViewModel();
        ab.BeginAnchorRef.Value = anchor.ID.ToString();
        var result = AnchorReferenceFinder.FindReferring(new SelectableDesignerItemViewModelBase[] { ab }, anchor.ID).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.SameAs(ab));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void 違うGuidのコネクタは拾わない()
    {
        var anchor = new AnchorViewModel();
        var oc = new OrthogonalConnectorViewModel();
        oc.BeginAnchorRef.Value = Guid.NewGuid().ToString();
        var result = AnchorReferenceFinder.FindReferring(new SelectableDesignerItemViewModelBase[] { oc }, anchor.ID).ToList();
        Assert.That(result, Is.Empty);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void 暗黙9点参照は対象外()
    {
        var anchor = new AnchorViewModel();
        var ownerGuid = Guid.NewGuid();
        var oc = new OrthogonalConnectorViewModel();
        // 暗黙 9 点参照: "{ownerGuid}#tl" 形式。anchor.ID には一致しないので対象外
        oc.BeginAnchorRef.Value = $"{ownerGuid}#tl";
        var result = AnchorReferenceFinder.FindReferring(new SelectableDesignerItemViewModelBase[] { oc }, anchor.ID).ToList();
        Assert.That(result, Is.Empty);
    }

    [Test, Apartment(ApartmentState.STA)]
    public void 複数コネクタの中から該当のみ抽出()
    {
        var anchor = new AnchorViewModel();
        var match1 = new OrthogonalConnectorViewModel();
        match1.BeginAnchorRef.Value = anchor.ID.ToString();
        var match2 = new AnchorBezierConnectorViewModel();
        match2.EndAnchorRef.Value = anchor.ID.ToString();
        var noMatch = new OrthogonalConnectorViewModel();
        noMatch.BeginAnchorRef.Value = Guid.NewGuid().ToString();

        var all = new SelectableDesignerItemViewModelBase[] { match1, noMatch, match2 };
        var result = AnchorReferenceFinder.FindReferring(all, anchor.ID).ToList();
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result, Does.Contain(match1));
        Assert.That(result, Does.Contain(match2));
    }

    [Test, Apartment(ApartmentState.STA)]
    public void StraightConnectorは対象外()
    {
        var anchor = new AnchorViewModel();
        var straight = new StraightConnectorViewModel();
        var result = AnchorReferenceFinder.FindReferring(new SelectableDesignerItemViewModelBase[] { straight }, anchor.ID).ToList();
        Assert.That(result, Is.Empty, "StraightConnector はそもそも AnchorRef を持たない");
    }

    [Test, Apartment(ApartmentState.STA)]
    public void all_nullは空()
    {
        var anchor = new AnchorViewModel();
        var result = AnchorReferenceFinder.FindReferring(null, anchor.ID).ToList();
        Assert.That(result, Is.Empty);
    }
}
