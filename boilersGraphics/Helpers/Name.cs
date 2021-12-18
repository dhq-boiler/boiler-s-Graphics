using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    static class Name
    {
        public static string GetNewLayerName(DiagramViewModel diagramViewModel)
        {
            return $"{Resources.Name_Layer}{diagramViewModel.LayerCount++}";
        }

        public static string GetNewLayerItemName(DiagramViewModel diagramViewModel)
        {
            return $"{Resources.Name_Item}{diagramViewModel.LayerItemCount++}";
        }
    }
}
