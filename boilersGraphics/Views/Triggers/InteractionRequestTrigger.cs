using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Views.Triggers
{
    public class InteractionRequestTrigger : EventTrigger
    {
        protected override string GetEventName()
        {
            return "Raised";
        }
    }
}
