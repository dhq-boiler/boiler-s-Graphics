using boilersGraphics.Properties;
using boilersGraphics.ViewModels;

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
