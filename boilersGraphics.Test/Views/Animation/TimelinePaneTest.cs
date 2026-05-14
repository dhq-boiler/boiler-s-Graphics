using boilersGraphics.ViewModels.Animation;
using boilersGraphics.Views.Animation;
using NUnit.Framework;
using System.Threading;

namespace boilersGraphics.Test.Views.Animation;

[TestFixture]
public class TimelinePaneTest
{
    [Test, RequiresThread(ApartmentState.STA)]
    public void TimelinePane_は構文エラーなくInitializeComponentできる()
    {
        var pane = new TimelinePane();
        Assert.That(pane, Is.Not.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TimelinePane_に_TimelineViewModel_を_DataContext_に設定できる()
    {
        var pane = new TimelinePane();
        var vm = new TimelineViewModel(duration: 5.0, fps: 30);
        pane.DataContext = vm;

        Assert.That(pane.DataContext, Is.SameAs(vm));
        Assert.That(vm.Tracks.Count, Is.EqualTo(0));
    }
}
