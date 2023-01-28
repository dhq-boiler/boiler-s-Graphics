using Microsoft.Xaml.Behaviors;

namespace boilersGraphics.Views.Triggers;

public class InteractionRequestTrigger : EventTrigger
{
    protected override string GetEventName()
    {
        return "Raised";
    }
}