using boilersGraphics.Models;
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
            return $"レイヤー{diagramViewModel.LayerCount++}";
        }

        public static string GetNewLayerItemName(DiagramViewModel diagramViewModel)
        {
            return $"アイテム{diagramViewModel.LayerItemCount++}";
        }
    }
}
