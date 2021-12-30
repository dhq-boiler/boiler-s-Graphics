﻿using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
                list.Add(new XElement("EdgeColor", designerItem.EdgeColor.Value));
                list.Add(new XElement("FillColor", designerItem.FillColor.Value));
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
                list.Add(new XElement("BeginPoint", connectorItem.Points[0]));
                list.Add(new XElement("EndPoint", connectorItem.Points[1]));
                list.Add(new XElement("ZIndex", connectorItem.ZIndex.Value));
                list.Add(new XElement("EdgeColor", connectorItem.EdgeColor));
                list.Add(new XElement("EdgeThickness", connectorItem.EdgeThickness));
                list.Add(new XElement("PathGeometry", connectorItem.PathGeometry.Value));
                if (connectorItem is BezierCurveViewModel)
                {
                    list.Add(new XElement("ControlPoint1", (connectorItem as BezierCurveViewModel).ControlPoint1.Value));
                    list.Add(new XElement("ControlPoint2", (connectorItem as BezierCurveViewModel).ControlPoint2.Value));
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
                list.Add(new XElement("EdgeColor", snapPointItem.EdgeColor.Value));
                list.Add(new XElement("FillColor", snapPointItem.FillColor.Value));
                list.Add(new XElement("EdgeThickness", snapPointItem.EdgeThickness.Value));
                list.Add(new XElement("PathGeometry", snapPointItem.PathGeometry.Value));
                var snappointItemXML = new XElement("SnapPointItem", list);
                return snappointItemXML;
            }
            else
                throw new Exception("Neither DesinerItem nor ConnectorItem");
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
                               new XElement("EdgeColor", connection.EdgeColor.Value),
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
                                    new XElement("EdgeColor", connection.EdgeColor.Value),
                                    new XElement("EdgeThickness", connection.EdgeThickness.Value),
                                    new XElement("ControlPoint1", (connection as BezierCurveViewModel).ControlPoint1.Value),
                                    new XElement("ControlPoint2", (connection as BezierCurveViewModel).ControlPoint2.Value),
                                    new XElement("PathGeometry", connection.PathGeometry.Value)
                    ));
        }

        public static IEnumerable<XElement> SerializeConfiguration(DiagramViewModel diagramViewModel)
        {
            return new XElement[]
            {
                new XElement("Width", diagramViewModel.Width),
                new XElement("Height", diagramViewModel.Height),
                new XElement("CanvasBackground", diagramViewModel.CanvasBackground.Value),
                new XElement("EnablePointSnap", diagramViewModel.EnablePointSnap.Value),
                new XElement("SnapPower", diagramViewModel.MainWindowVM.SnapPower.Value)
            };
        }
    }
}
