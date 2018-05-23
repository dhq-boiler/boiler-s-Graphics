using System;
using System.Collections.Generic;

namespace grapher.Models
{
    [Serializable]
    public class Diagram
    {
        public List<RenderItem> RenderItems { get; set; }
    }
}
