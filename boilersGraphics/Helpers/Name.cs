using boilersGraphics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    static class Name
    {
        public static string GetNewLayerName()
        {
            return $"レイヤー{Layer.LayerCount++}";
        }

        public static string GetNewLayerItemName()
        {
            return $"アイテム{LayerItem.LayerItemCount++}";
        }
    }
}
