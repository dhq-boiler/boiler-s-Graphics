using boilersGraphics.Helpers.Anchors;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace boilersGraphics.Test.Helpers.Anchors;

[TestFixture]
public class NodeRelatedConnectorFinderTest
{
    [Test]
    public void RefersToNode_暗黙Ref_当該nodeのID_一致でtrue()
    {
        var nodeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var result = NodeRelatedConnectorFinder.RefersToNode(
            $"{nodeId}#tl", nodeId, new HashSet<Guid>());
        Assert.That(result, Is.True);
    }

    [Test]
    public void RefersToNode_暗黙Ref_別Guidの場合false()
    {
        var nodeId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var result = NodeRelatedConnectorFinder.RefersToNode(
            $"{otherId}#br", nodeId, new HashSet<Guid>());
        Assert.That(result, Is.False);
    }

    [Test]
    public void RefersToNode_明示AnchorRef_nodeのAnchorIDなら_true()
    {
        var nodeId = Guid.NewGuid();
        var anchorId = Guid.NewGuid();
        var owned = new HashSet<Guid> { anchorId };
        var result = NodeRelatedConnectorFinder.RefersToNode(
            anchorId.ToString(), nodeId, owned);
        Assert.That(result, Is.True);
    }

    [Test]
    public void RefersToNode_明示AnchorRef_owned外なら_false()
    {
        var nodeId = Guid.NewGuid();
        var anchorId = Guid.NewGuid();
        var result = NodeRelatedConnectorFinder.RefersToNode(
            anchorId.ToString(), nodeId, new HashSet<Guid>());
        Assert.That(result, Is.False);
    }

    [Test]
    public void RefersToNode_空Ref_false()
    {
        Assert.That(NodeRelatedConnectorFinder.RefersToNode(null, Guid.NewGuid(), new HashSet<Guid>()), Is.False);
        Assert.That(NodeRelatedConnectorFinder.RefersToNode(string.Empty, Guid.NewGuid(), new HashSet<Guid>()), Is.False);
    }

    [Test]
    public void RefersToNode_暗黙Ref_Guid部分が不正_false()
    {
        var result = NodeRelatedConnectorFinder.RefersToNode(
            "not-a-guid#tl", Guid.NewGuid(), new HashSet<Guid>());
        Assert.That(result, Is.False);
    }

    [Test]
    public void RefersToNode_明示Ref_Guidパース失敗_false()
    {
        var result = NodeRelatedConnectorFinder.RefersToNode(
            "not-a-guid", Guid.NewGuid(), new HashSet<Guid> { Guid.NewGuid() });
        Assert.That(result, Is.False);
    }

    [Test]
    public void FindRelated_diagramがnull_空列挙()
    {
        var result = NodeRelatedConnectorFinder.FindRelated(null, null);
        Assert.That(result, Is.Empty);
    }
}
