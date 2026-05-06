using boilersGraphics.ViewModels;
using NUnit.Framework;
using R3;
using TsOperationHistory;
using TsOperationHistory.Extensions;

namespace boilersGraphics.Test;

[TestFixture]
public class DragMoveTest
{
    [Test]
    public void DirectAssignment_ChangesLeftValue()
    {
        var left = new BindableReactiveProperty<double>(100.0);
        Assert.That(left.Value, Is.EqualTo(100.0));

        left.Value = 150.0;
        Assert.That(left.Value, Is.EqualTo(150.0));
    }

    [Test]
    public void ExecuteSetProperty_ChangesBindableReactivePropertyValue()
    {
        var controller = new OperationController();
        var recorder = new OperationRecorder(controller);

        var item = new TestItem();
        item.Left.Value = 100.0;
        item.Top.Value = 200.0;

        recorder.BeginRecode();
        recorder.Current.ExecuteSetProperty(item, "Left.Value", 150.0);
        recorder.Current.ExecuteSetProperty(item, "Top.Value", 250.0);
        recorder.EndRecode();

        Assert.That(item.Left.Value, Is.EqualTo(150.0), "Left should be updated by ExecuteSetProperty");
        Assert.That(item.Top.Value, Is.EqualTo(250.0), "Top should be updated by ExecuteSetProperty");
    }

    [Test]
    public void ExecuteSetProperty_IncrementalDrag_AccumulatesCorrectly()
    {
        var controller = new OperationController();
        var recorder = new OperationRecorder(controller);

        var item = new TestItem();
        item.Left.Value = 100.0;
        item.Top.Value = 100.0;

        recorder.BeginRecode();

        // Simulate 5 drag delta events of 10px each
        for (int i = 0; i < 5; i++)
        {
            double left = item.Left.Value;
            double top = item.Top.Value;
            recorder.Current.ExecuteSetProperty(item, "Left.Value", left + 10.0);
            recorder.Current.ExecuteSetProperty(item, "Top.Value", top + 10.0);
        }

        recorder.EndRecode();

        Assert.That(item.Left.Value, Is.EqualTo(150.0), "Left should be 100 + 5*10 = 150");
        Assert.That(item.Top.Value, Is.EqualTo(150.0), "Top should be 100 + 5*10 = 150");
    }

    [Test]
    public void ExecuteSetProperty_Undo_RestoresOriginalValue()
    {
        var controller = new OperationController();
        var recorder = new OperationRecorder(controller);

        var item = new TestItem();
        item.Left.Value = 100.0;

        recorder.BeginRecode();
        recorder.Current.ExecuteSetProperty(item, "Left.Value", 200.0);
        recorder.EndRecode();

        Assert.That(item.Left.Value, Is.EqualTo(200.0));

        controller.Undo();
        Assert.That(item.Left.Value, Is.EqualTo(100.0), "Undo should restore to 100");
    }

    private class TestItem
    {
        public BindableReactiveProperty<double> Left { get; } = new(0);
        public BindableReactiveProperty<double> Top { get; } = new(0);
    }
}
