using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace boilersGraphics.Helpers
{
    class ObjectSerializer
    {
        public static IEnumerable<XElement> SerializeLayers(ObservableCollection<LayerTreeViewItemBase> layers)
        {
            var layersXML = from layer in layers
                            select new XElement("Layer", ExtractLayer(layer));
            return layersXML;
        }

        private static IEnumerable<XElement> ExtractLayer(LayerTreeViewItemBase layer)
        {
            var layerXML = new List<XElement>();
            layerXML.Add(new XElement("IsVisible", layer.IsVisible.Value));
            layerXML.Add(new XElement("Name", layer.Name.Value));
            layerXML.Add(new XElement("Color", layer.Color.Value));
            layerXML.Add(new XElement("Children", ExtractLayerItemFromLayer(layer)));
            return layerXML;
        }

        private static IEnumerable<XElement> ExtractLayerItemFromLayer(LayerTreeViewItemBase layer)
        {
            var layerItemsXML = from layerItem in layer.Children
                                select ExtractLayerItem(layerItem as LayerItem);
            return layerItemsXML;
        }

        public static XElement ExtractItems(IEnumerable<LayerItem> layerItems)
        {
            var layerItemsXML = new XElement("LayerItems",
                from layerItem in layerItems
                select ExtractLayerItem(layerItem)
                );
            return layerItemsXML;
        }

        private static XElement ExtractLayerItem(LayerItem layerItem)
        {
            return new XElement("LayerItem",
                                new XElement("IsVisible", layerItem.IsVisible.Value),
                                new XElement("Name", layerItem.Name.Value),
                                new XElement("Color", layerItem.Color.Value),
                                new XElement("Item", ExtractItem(layerItem.Item.Value)),
                                new XElement("Children", (from child in layerItem.Children
                                                          select ExtractLayerItem(child as LayerItem))
                                            )
                                );
        }

        public static XElement ExtractItem(SelectableDesignerItemViewModelBase item)
        {
            var designerItem = item as DesignerItemViewModelBase;
            var connectorItem = item as ConnectorBaseViewModel;
            var snapPointItem = item as SnapPointViewModel;
            if (designerItem != null)
            {
                var list = new List<XElement>();
                list.Add(new XElement("ID", designerItem.ID));
                list.Add(new XElement("ParentID", designerItem.ParentID));
                list.Add(new XElement("Type", designerItem.GetType().FullName));
                list.Add(new XElement("Left", designerItem.Left.Value));
                list.Add(new XElement("Top", designerItem.Top.Value));
                list.Add(new XElement("Width", designerItem.Width.Value));
                list.Add(new XElement("Height", designerItem.Height.Value));
                list.Add(new XElement("ZIndex", designerItem.ZIndex.Value));
                //list.Add(new XElement("Matrix", designerItem.Matrix.Value));
                list.Add(new XElement("EdgeBrush", designerItem.EdgeBrush.Value));
                list.Add(new XElement("FillBrush", designerItem.FillBrush.Value));
                list.Add(new XElement("EdgeThickness", designerItem.EdgeThickness.Value));
                list.Add(new XElement("PathGeometry", designerItem.PathGeometry.Value));
                list.Add(new XElement("RotationAngle", designerItem.RotationAngle.Value));
                if (designerItem is PictureDesignerItemViewModel picture)
                {
                    list.Add(new XElement("FileName", (designerItem as PictureDesignerItemViewModel).FileName));
                    var enableImageEmbedding = (App.GetCurrentApp().MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.EnableImageEmbedding.Value;
                    list.Add(new XElement("EnableImageEmbedding", enableImageEmbedding));
                    if (enableImageEmbedding)
                    {
                        var image = !string.IsNullOrEmpty(picture.FileName) ? new BitmapImage(new Uri(picture.FileName)) : picture.EmbeddedImage.Value;
                        var writeableBitmap = new WriteableBitmap(image);
                        using (var memStream = new MemoryStream())
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                            encoder.Save(memStream);
                            list.Add(new XElement("EmbeddedImageBase64", Convert.ToBase64String(memStream.ToArray())));
                        }
                    }
                }
                if (designerItem is ILetterDesignerItemViewModel)
                {
                    list.Add(new XElement("LetterString", (designerItem as ILetterDesignerItemViewModel).LetterString));
                    list.Add(new XElement("SelectedFontFamily", (designerItem as ILetterDesignerItemViewModel).SelectedFontFamily));
                    list.Add(new XElement("IsBold", (designerItem as ILetterDesignerItemViewModel).IsBold));
                    list.Add(new XElement("IsItalic", (designerItem as ILetterDesignerItemViewModel).IsItalic));
                    list.Add(new XElement("FontSize", (designerItem as ILetterDesignerItemViewModel).FontSize));
                    //list.Add(new XElement("PathGeometry", (designerItem as ILetterDesignerItemViewModel).PathGeometry));
                    list.Add(new XElement("AutoLineBreak", (designerItem as ILetterDesignerItemViewModel).AutoLineBreak));
                }
                if (designerItem is NPolygonViewModel)
                {
                    list.Add(new XElement("Data", (designerItem as NPolygonViewModel).Data.Value));
                }
                var designerItemXML = new XElement("DesignerItem", list);
                return designerItemXML;
            }
            else if (connectorItem != null)
            {
                var list = new List<XElement>();
                list.Add(new XElement("ID", connectorItem.ID));
                list.Add(new XElement("ParentID", connectorItem.ParentID));
                list.Add(new XElement("Type", connectorItem.GetType().FullName));
                if (connectorItem is StraightConnectorViewModel || connectorItem is BezierCurveViewModel)
                {
                    list.Add(new XElement("BeginPoint", connectorItem.Points[0]));
                    list.Add(new XElement("EndPoint", connectorItem.Points[1]));
                }
                list.Add(new XElement("ZIndex", connectorItem.ZIndex.Value));
                list.Add(new XElement("EdgeBrush", connectorItem.EdgeBrush.Value));
                list.Add(new XElement("EdgeThickness", connectorItem.EdgeThickness.Value));
                list.Add(new XElement("PathGeometry", connectorItem.PathGeometry.Value));
                list.Add(new XElement("LeftTop", connectorItem.LeftTop.Value));
                if (connectorItem is BezierCurveViewModel)
                {
                    list.Add(new XElement("ControlPoint1", (connectorItem as BezierCurveViewModel).ControlPoint1.Value));
                    list.Add(new XElement("ControlPoint2", (connectorItem as BezierCurveViewModel).ControlPoint2.Value));
                }
                if (connectorItem is PolyBezierViewModel)
                {
                    list.Add(new XElement("Points", PointsToStr(connectorItem.Points)));
                }
                var connectorItemXML = new XElement("ConnectorItem", list);
                return connectorItemXML;
            }
            else if (snapPointItem != null)
            {
                var list = new List<XElement>();
                list.Add(new XElement("ID", snapPointItem.ID));
                list.Add(new XElement("ParentID", snapPointItem.ParentID));
                list.Add(new XElement("Type", snapPointItem.GetType().FullName));
                list.Add(new XElement("Left", snapPointItem.Left.Value));
                list.Add(new XElement("Top", snapPointItem.Top.Value));
                list.Add(new XElement("Width", snapPointItem.Width.Value));
                list.Add(new XElement("Height", snapPointItem.Height.Value));
                list.Add(new XElement("Opacity", snapPointItem.Opacity.Value));
                list.Add(new XElement("ZIndex", snapPointItem.ZIndex.Value));
                list.Add(new XElement("Matrix", snapPointItem.Matrix.Value));
                list.Add(new XElement("EdgeBrush", snapPointItem.EdgeBrush.Value));
                list.Add(new XElement("FillBrush", snapPointItem.FillBrush.Value));
                list.Add(new XElement("EdgeThickness", snapPointItem.EdgeThickness.Value));
                list.Add(new XElement("PathGeometry", snapPointItem.PathGeometry.Value));
                var snappointItemXML = new XElement("SnapPointItem", list);
                return snappointItemXML;
            }
            else
                throw new Exception("Neither DesinerItem nor ConnectorItem");
        }

        private static string PointsToStr(ObservableCollection<Point> points)
        {
            var ret = string.Empty;
            foreach (Point point in points)
            {
                ret += $"{point.X},{point.Y}";
                if (point != points.Last())
                    ret += " ";
            }
            return ret;
        }

        public static IEnumerable<XElement> SerializeConnections(DiagramViewModel dialogViewModel, IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            return (from connection in items.WithPickupChildren(dialogViewModel.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                                                                      .Where(x => x is LayerItem)
                                                                                      .Select(x => (x as LayerItem).Item.Value)
                                                               ).OfType<ConnectorBaseViewModel>()
                    where connection.GetType() != typeof(BezierCurveViewModel)
                    select new XElement("Connection",
                               new XElement("ID", connection.ID),
                               new XElement("ParentID", connection.ParentID),
                               new XElement("Type", connection.GetType().FullName),
                               new XElement("BeginPoint", connection.Points[0]),
                               new XElement("EndPoint", connection.Points[1]),
                               new XElement("ZIndex", connection.ZIndex.Value),
                               new XElement("EdgeBrush", connection.EdgeBrush.Value),
                               new XElement("EdgeThickness", connection.EdgeThickness.Value),
                               new XElement("PathGeometry", connection.PathGeometry.Value)
                    ))
                    .Union(
                        from connection in items.WithPickupChildren(dialogViewModel.Layers.SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                                                                                      .Where(x => x is LayerItem)
                                                                                      .Select(x => (x as LayerItem).Item.Value)
                                                                   ).OfType<ConnectorBaseViewModel>()
                        where connection.GetType() == typeof(BezierCurveViewModel)
                        select new XElement("Connection",
                                    new XElement("ID", connection.ID),
                                    new XElement("ParentID", connection.ParentID),
                                    new XElement("Type", connection.GetType().FullName),
                                    new XElement("BeginPoint", connection.Points[0]),
                                    new XElement("EndPoint", connection.Points[1]),
                                    new XElement("ZIndex", connection.ZIndex.Value),
                                    new XElement("EdgeBrush", connection.EdgeBrush.Value),
                                    new XElement("EdgeThickness", connection.EdgeThickness.Value),
                                    new XElement("ControlPoint1", (connection as BezierCurveViewModel).ControlPoint1.Value),
                                    new XElement("ControlPoint2", (connection as BezierCurveViewModel).ControlPoint2.Value),
                                    new XElement("PathGeometry", connection.PathGeometry.Value)
                    ));
        }

        public static IEnumerable<XElement> SerializeConfiguration(DiagramViewModel diagramViewModel)
        {
            var colorSpots = new XElement("ColorSpots");
            colorSpots.Add(new XElement("ColorSpot0", diagramViewModel.ColorSpots.Value.ColorSpot0));
            colorSpots.Add(new XElement("ColorSpot1", diagramViewModel.ColorSpots.Value.ColorSpot1));
            colorSpots.Add(new XElement("ColorSpot2", diagramViewModel.ColorSpots.Value.ColorSpot2));
            colorSpots.Add(new XElement("ColorSpot3", diagramViewModel.ColorSpots.Value.ColorSpot3));
            colorSpots.Add(new XElement("ColorSpot4", diagramViewModel.ColorSpots.Value.ColorSpot4));
            colorSpots.Add(new XElement("ColorSpot5", diagramViewModel.ColorSpots.Value.ColorSpot5));
            colorSpots.Add(new XElement("ColorSpot6", diagramViewModel.ColorSpots.Value.ColorSpot6));
            colorSpots.Add(new XElement("ColorSpot7", diagramViewModel.ColorSpots.Value.ColorSpot7));
            colorSpots.Add(new XElement("ColorSpot8", diagramViewModel.ColorSpots.Value.ColorSpot8));
            colorSpots.Add(new XElement("ColorSpot9", diagramViewModel.ColorSpots.Value.ColorSpot9));
            colorSpots.Add(new XElement("ColorSpot10", diagramViewModel.ColorSpots.Value.ColorSpot10));
            colorSpots.Add(new XElement("ColorSpot11", diagramViewModel.ColorSpots.Value.ColorSpot11));
            colorSpots.Add(new XElement("ColorSpot12", diagramViewModel.ColorSpots.Value.ColorSpot12));
            colorSpots.Add(new XElement("ColorSpot13", diagramViewModel.ColorSpots.Value.ColorSpot13));
            colorSpots.Add(new XElement("ColorSpot14", diagramViewModel.ColorSpots.Value.ColorSpot14));
            colorSpots.Add(new XElement("ColorSpot15", diagramViewModel.ColorSpots.Value.ColorSpot15));
            colorSpots.Add(new XElement("ColorSpot16", diagramViewModel.ColorSpots.Value.ColorSpot16));
            colorSpots.Add(new XElement("ColorSpot17", diagramViewModel.ColorSpots.Value.ColorSpot17));
            colorSpots.Add(new XElement("ColorSpot18", diagramViewModel.ColorSpots.Value.ColorSpot18));
            colorSpots.Add(new XElement("ColorSpot19", diagramViewModel.ColorSpots.Value.ColorSpot19));
            colorSpots.Add(new XElement("ColorSpot20", diagramViewModel.ColorSpots.Value.ColorSpot20));
            colorSpots.Add(new XElement("ColorSpot21", diagramViewModel.ColorSpots.Value.ColorSpot21));
            colorSpots.Add(new XElement("ColorSpot22", diagramViewModel.ColorSpots.Value.ColorSpot22));
            colorSpots.Add(new XElement("ColorSpot23", diagramViewModel.ColorSpots.Value.ColorSpot23));
            colorSpots.Add(new XElement("ColorSpot24", diagramViewModel.ColorSpots.Value.ColorSpot24));
            colorSpots.Add(new XElement("ColorSpot25", diagramViewModel.ColorSpots.Value.ColorSpot25));
            colorSpots.Add(new XElement("ColorSpot26", diagramViewModel.ColorSpots.Value.ColorSpot26));
            colorSpots.Add(new XElement("ColorSpot27", diagramViewModel.ColorSpots.Value.ColorSpot27));
            colorSpots.Add(new XElement("ColorSpot28", diagramViewModel.ColorSpots.Value.ColorSpot28));
            colorSpots.Add(new XElement("ColorSpot29", diagramViewModel.ColorSpots.Value.ColorSpot29));
            colorSpots.Add(new XElement("ColorSpot30", diagramViewModel.ColorSpots.Value.ColorSpot30));
            colorSpots.Add(new XElement("ColorSpot31", diagramViewModel.ColorSpots.Value.ColorSpot31));
            colorSpots.Add(new XElement("ColorSpot32", diagramViewModel.ColorSpots.Value.ColorSpot32));
            colorSpots.Add(new XElement("ColorSpot33", diagramViewModel.ColorSpots.Value.ColorSpot33));
            colorSpots.Add(new XElement("ColorSpot34", diagramViewModel.ColorSpots.Value.ColorSpot34));
            colorSpots.Add(new XElement("ColorSpot35", diagramViewModel.ColorSpots.Value.ColorSpot35));
            colorSpots.Add(new XElement("ColorSpot36", diagramViewModel.ColorSpots.Value.ColorSpot36));
            colorSpots.Add(new XElement("ColorSpot37", diagramViewModel.ColorSpots.Value.ColorSpot37));
            colorSpots.Add(new XElement("ColorSpot38", diagramViewModel.ColorSpots.Value.ColorSpot38));
            colorSpots.Add(new XElement("ColorSpot39", diagramViewModel.ColorSpots.Value.ColorSpot39));
            colorSpots.Add(new XElement("ColorSpot40", diagramViewModel.ColorSpots.Value.ColorSpot40));
            colorSpots.Add(new XElement("ColorSpot41", diagramViewModel.ColorSpots.Value.ColorSpot41));
            colorSpots.Add(new XElement("ColorSpot42", diagramViewModel.ColorSpots.Value.ColorSpot42));
            colorSpots.Add(new XElement("ColorSpot43", diagramViewModel.ColorSpots.Value.ColorSpot43));
            colorSpots.Add(new XElement("ColorSpot44", diagramViewModel.ColorSpots.Value.ColorSpot44));
            colorSpots.Add(new XElement("ColorSpot45", diagramViewModel.ColorSpots.Value.ColorSpot45));
            colorSpots.Add(new XElement("ColorSpot46", diagramViewModel.ColorSpots.Value.ColorSpot46));
            colorSpots.Add(new XElement("ColorSpot47", diagramViewModel.ColorSpots.Value.ColorSpot47));
            colorSpots.Add(new XElement("ColorSpot48", diagramViewModel.ColorSpots.Value.ColorSpot48));
            colorSpots.Add(new XElement("ColorSpot49", diagramViewModel.ColorSpots.Value.ColorSpot49));
            colorSpots.Add(new XElement("ColorSpot50", diagramViewModel.ColorSpots.Value.ColorSpot50));
            colorSpots.Add(new XElement("ColorSpot51", diagramViewModel.ColorSpots.Value.ColorSpot51));
            colorSpots.Add(new XElement("ColorSpot52", diagramViewModel.ColorSpots.Value.ColorSpot52));
            colorSpots.Add(new XElement("ColorSpot53", diagramViewModel.ColorSpots.Value.ColorSpot53));
            colorSpots.Add(new XElement("ColorSpot54", diagramViewModel.ColorSpots.Value.ColorSpot54));
            colorSpots.Add(new XElement("ColorSpot55", diagramViewModel.ColorSpots.Value.ColorSpot55));
            colorSpots.Add(new XElement("ColorSpot56", diagramViewModel.ColorSpots.Value.ColorSpot56));
            colorSpots.Add(new XElement("ColorSpot57", diagramViewModel.ColorSpots.Value.ColorSpot57));
            colorSpots.Add(new XElement("ColorSpot58", diagramViewModel.ColorSpots.Value.ColorSpot58));
            colorSpots.Add(new XElement("ColorSpot59", diagramViewModel.ColorSpots.Value.ColorSpot59));
            colorSpots.Add(new XElement("ColorSpot60", diagramViewModel.ColorSpots.Value.ColorSpot60));
            colorSpots.Add(new XElement("ColorSpot61", diagramViewModel.ColorSpots.Value.ColorSpot61));
            colorSpots.Add(new XElement("ColorSpot62", diagramViewModel.ColorSpots.Value.ColorSpot62));
            colorSpots.Add(new XElement("ColorSpot63", diagramViewModel.ColorSpots.Value.ColorSpot63));
            colorSpots.Add(new XElement("ColorSpot64", diagramViewModel.ColorSpots.Value.ColorSpot64));
            colorSpots.Add(new XElement("ColorSpot65", diagramViewModel.ColorSpots.Value.ColorSpot65));
            colorSpots.Add(new XElement("ColorSpot66", diagramViewModel.ColorSpots.Value.ColorSpot66));
            colorSpots.Add(new XElement("ColorSpot67", diagramViewModel.ColorSpots.Value.ColorSpot67));
            colorSpots.Add(new XElement("ColorSpot68", diagramViewModel.ColorSpots.Value.ColorSpot68));
            colorSpots.Add(new XElement("ColorSpot69", diagramViewModel.ColorSpots.Value.ColorSpot69));
            colorSpots.Add(new XElement("ColorSpot70", diagramViewModel.ColorSpots.Value.ColorSpot70));
            colorSpots.Add(new XElement("ColorSpot71", diagramViewModel.ColorSpots.Value.ColorSpot71));
            colorSpots.Add(new XElement("ColorSpot72", diagramViewModel.ColorSpots.Value.ColorSpot72));
            colorSpots.Add(new XElement("ColorSpot73", diagramViewModel.ColorSpots.Value.ColorSpot73));
            colorSpots.Add(new XElement("ColorSpot74", diagramViewModel.ColorSpots.Value.ColorSpot74));
            colorSpots.Add(new XElement("ColorSpot75", diagramViewModel.ColorSpots.Value.ColorSpot75));
            colorSpots.Add(new XElement("ColorSpot76", diagramViewModel.ColorSpots.Value.ColorSpot76));
            colorSpots.Add(new XElement("ColorSpot77", diagramViewModel.ColorSpots.Value.ColorSpot77));
            colorSpots.Add(new XElement("ColorSpot78", diagramViewModel.ColorSpots.Value.ColorSpot78));
            colorSpots.Add(new XElement("ColorSpot79", diagramViewModel.ColorSpots.Value.ColorSpot79));
            colorSpots.Add(new XElement("ColorSpot80", diagramViewModel.ColorSpots.Value.ColorSpot80));
            colorSpots.Add(new XElement("ColorSpot81", diagramViewModel.ColorSpots.Value.ColorSpot81));
            colorSpots.Add(new XElement("ColorSpot82", diagramViewModel.ColorSpots.Value.ColorSpot82));
            colorSpots.Add(new XElement("ColorSpot83", diagramViewModel.ColorSpots.Value.ColorSpot83));
            colorSpots.Add(new XElement("ColorSpot84", diagramViewModel.ColorSpots.Value.ColorSpot84));
            colorSpots.Add(new XElement("ColorSpot85", diagramViewModel.ColorSpots.Value.ColorSpot85));
            colorSpots.Add(new XElement("ColorSpot86", diagramViewModel.ColorSpots.Value.ColorSpot86));
            colorSpots.Add(new XElement("ColorSpot87", diagramViewModel.ColorSpots.Value.ColorSpot87));
            colorSpots.Add(new XElement("ColorSpot88", diagramViewModel.ColorSpots.Value.ColorSpot88));
            colorSpots.Add(new XElement("ColorSpot89", diagramViewModel.ColorSpots.Value.ColorSpot89));
            colorSpots.Add(new XElement("ColorSpot90", diagramViewModel.ColorSpots.Value.ColorSpot90));
            colorSpots.Add(new XElement("ColorSpot91", diagramViewModel.ColorSpots.Value.ColorSpot91));
            colorSpots.Add(new XElement("ColorSpot92", diagramViewModel.ColorSpots.Value.ColorSpot92));
            colorSpots.Add(new XElement("ColorSpot93", diagramViewModel.ColorSpots.Value.ColorSpot93));
            colorSpots.Add(new XElement("ColorSpot94", diagramViewModel.ColorSpots.Value.ColorSpot94));
            colorSpots.Add(new XElement("ColorSpot95", diagramViewModel.ColorSpots.Value.ColorSpot95));
            colorSpots.Add(new XElement("ColorSpot96", diagramViewModel.ColorSpots.Value.ColorSpot96));
            colorSpots.Add(new XElement("ColorSpot97", diagramViewModel.ColorSpots.Value.ColorSpot97));
            colorSpots.Add(new XElement("ColorSpot98", diagramViewModel.ColorSpots.Value.ColorSpot98));
            colorSpots.Add(new XElement("ColorSpot99", diagramViewModel.ColorSpots.Value.ColorSpot99));
            return new XElement[]
            {
                new XElement("Width", diagramViewModel.Width),
                new XElement("Height", diagramViewModel.Height),
                new XElement("CanvasBackground", diagramViewModel.CanvasBackground.Value),
                new XElement("EnablePointSnap", diagramViewModel.EnablePointSnap.Value),
                new XElement("SnapPower", diagramViewModel.MainWindowVM.SnapPower.Value),
                colorSpots
            };
        }
    }
}
