﻿using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace boilersGraphics.Helpers;

internal class ObjectSerializer
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
        layerXML.Add(new XElement("IsExpanded", layer.IsExpanded.Value));
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
            new XElement("IsExpanded", layerItem.IsExpanded.Value),
            new XElement("Name", layerItem.Name.Value),
            new XElement("Color", layerItem.Color.Value),
            new XElement("Item", ExtractItem(layerItem.Item.Value)),
            new XElement("Children", from child in layerItem.Children
                select ExtractLayerItem(child as LayerItem)
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
            list.Add(new XElement("EdgeBrush",
                XElement.Parse(WpfObjectSerializer.Serialize(designerItem.EdgeBrush.Value))));
            list.Add(new XElement("FillBrush",
                XElement.Parse(WpfObjectSerializer.Serialize(designerItem.FillBrush.Value))));
            list.Add(new XElement("EdgeThickness", designerItem.EdgeThickness.Value));
            list.Add(new XElement("PathGeometryNoRotate", designerItem.PathGeometryNoRotate.Value));
            list.Add(new XElement("PathGeometryRotate", designerItem.PathGeometryRotate.Value));
            list.Add(new XElement("RotationAngle", designerItem.RotationAngle.Value));
            list.Add(new XElement("StrokeLineJoin", designerItem.StrokeLineJoin.Value));
            list.Add(new XElement("StrokeMiterLimit", designerItem.StrokeMiterLimit.Value));
            list.Add(new XElement("StrokeDashArray", designerItem.StrokeDashArray.Value.ToString()));
            if (designerItem is NRectangleViewModel rectangle)
            {
                list.Add(new XElement("RadiusX", rectangle.RadiusX.Value));
                list.Add(new XElement("RadiusY", rectangle.RadiusY.Value));
            }

            if (designerItem is PictureDesignerItemViewModel picture)
            {
                list.Add(new XElement("FileName", picture.FileName));
                var enableImageEmbedding = (App.GetCurrentApp().MainWindow.DataContext as MainWindowViewModel)
                    .DiagramViewModel.EnableImageEmbedding.Value;
                list.Add(new XElement("EnableImageEmbedding", enableImageEmbedding));
                if (enableImageEmbedding)
                {
                    var image = !string.IsNullOrEmpty(picture.FileName)
                        ? new BitmapImage(new Uri(picture.FileName))
                        : picture.EmbeddedImage.Value;
                    var writeableBitmap = new WriteableBitmap(image);
                    using (var memStream = new MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
                        encoder.Save(memStream);

                        using (var br = new BinaryReader(memStream))
                        {
                            memStream.Seek(0, SeekOrigin.Begin);
                            const int CHUNK_SIZE = 1024;
                            byte[] bs = br.ReadBytes(CHUNK_SIZE);
                            StringBuilder base64str = new StringBuilder();

                            while (bs.Length > 0)
                            {

                                base64str.Append(Convert.ToBase64String(bs));

                                bs = br.ReadBytes(CHUNK_SIZE);
                            }

                            list.Add(new XElement("EmbeddedImageBase64", base64str.ToString()));
                        }
                    }
                }
            }

            if (designerItem is CroppedPictureDesignerItemViewModel cropped)
            {
                var enableImageEmbedding = (App.GetCurrentApp().MainWindow.DataContext as MainWindowViewModel)
                    .DiagramViewModel.EnableImageEmbedding.Value;
                list.Add(new XElement("EnableImageEmbedding", enableImageEmbedding));
                if (enableImageEmbedding)
                {
                    var image = !string.IsNullOrEmpty(cropped.FileName)
                        ? new BitmapImage(new Uri(cropped.FileName))
                        : cropped.EmbeddedImage.Value;
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

            if (designerItem is MosaicViewModel mosaic)
            {
                list.Add(new XElement("ColumnPixels", mosaic.ColumnPixels.Value));
                list.Add(new XElement("RowPixels", mosaic.RowPixels.Value));
            }

            if (designerItem is BlurEffectViewModel blurEffect)
            {
                list.Add(new XElement("KernelWidth", blurEffect.KernelWidth.Value));
                list.Add(new XElement("KernelHeight", blurEffect.KernelHeight.Value));
                list.Add(new XElement("Sigma", blurEffect.Sigma.Value));
            }

            if (designerItem is ColorCorrectViewModel colorCorrect)
            {
                list.Add(new XElement("CCType", colorCorrect.CCType.Value.GetType().Name));
                if (colorCorrect.CCType.Value == ColorCorrectType.HSV)
                {
                    list.Add(new XElement("AddHue", colorCorrect.AddHue.Value));
                    list.Add(new XElement("AddSaturation", colorCorrect.AddSaturation.Value));
                    list.Add(new XElement("AddValue", colorCorrect.AddValue.Value));
                }
                else if (colorCorrect.CCType.Value == ColorCorrectType.ToneCurve)
                {
                    list.Add(new XElement("TargetChannel", colorCorrect.TargetChannel.Value.GetType().Name));

                    var curves = new XElement("Curves");

                    foreach (var curve in colorCorrect.Curves)
                    {
                        var _curve = new XElement("Curve");

                        _curve.Add(new XElement("Name", curve.TargetChannel.Value.GetType().Name));

                        var points = new XElement("Points");
                        foreach (var pt in curve.Points)
                        {
                            var point = new XElement("Point");
                            var x = new XElement("X", pt.X);
                            point.Add(x);
                            var y = new XElement("Y", pt.Y);
                            point.Add(y);
                            points.Add(point);
                        }

                        _curve.Add(points);
                        var inOutPairs = new XElement("InOutPairs");
                        foreach (var pair in curve.InOutPairs)
                        {
                            var _pair = new XElement("InOutPair");
                            var @in = new XElement("In", pair.In);
                            _pair.Add(@in);
                            var @out = new XElement("Out", pair.Out);
                            _pair.Add(@out);
                            inOutPairs.Add(_pair);
                        }

                        _curve.Add(inOutPairs);

                        curves.Add(_curve);
                    }

                    list.Add(curves);
                }
                else if (colorCorrect.CCType.Value == ColorCorrectType.NegativePositiveConversion)
                {
                    //Do nothing. No need to do anything.
                }
            }

            if (designerItem is ILetterDesignerItemViewModel letter)
            {
                list.Add(new XElement("LetterString", letter.LetterString.Value));
                list.Add(new XElement("SelectedFontFamily", letter.SelectedFontFamily.Value));
                list.Add(new XElement("IsBold", letter.IsBold.Value));
                list.Add(new XElement("IsItalic", letter.IsItalic.Value));
                list.Add(new XElement("FontSize", letter.FontSize.Value));
                //list.Add(new XElement("PathGeometry", (designerItem as ILetterDesignerItemViewModel).PathGeometry));
                list.Add(new XElement("AutoLineBreak", letter.IsAutoLineBreak.Value));
            }

            if (designerItem is NPolygonViewModel polygon) list.Add(new XElement("Data", polygon.Data.Value));
            var designerItemXML = new XElement("DesignerItem", list);
            return designerItemXML;
        }

        if (connectorItem != null)
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
            list.Add(new XElement("EdgeBrush",
                XElement.Parse(WpfObjectSerializer.Serialize(connectorItem.EdgeBrush.Value))));
            list.Add(new XElement("EdgeThickness", connectorItem.EdgeThickness.Value));
            list.Add(new XElement("StrokeLineJoin", connectorItem.StrokeLineJoin.Value));
            list.Add(new XElement("StrokeMiterLimit", connectorItem.StrokeMiterLimit.Value));
            list.Add(new XElement("StrokeDashArray", connectorItem.StrokeDashArray.Value.ToString()));
            list.Add(new XElement("PathGeometry", connectorItem.PathGeometryNoRotate.Value));
            list.Add(new XElement("LeftTop", connectorItem.LeftTop.Value));
            if (connectorItem is BezierCurveViewModel bezier)
            {
                list.Add(new XElement("ControlPoint1", bezier.ControlPoint1.Value));
                list.Add(new XElement("ControlPoint2", bezier.ControlPoint2.Value));
            }

            if (connectorItem is PolyBezierViewModel polyBezier)
                list.Add(new XElement("Points", PointsToStr(connectorItem.Points)));
            var connectorItemXML = new XElement("ConnectorItem", list);
            return connectorItemXML;
        }

        if (snapPointItem != null)
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
            list.Add(new XElement("EdgeBrush",
                XElement.Parse(WpfObjectSerializer.Serialize(snapPointItem.EdgeBrush.Value))));
            list.Add(new XElement("FillBrush",
                XElement.Parse(WpfObjectSerializer.Serialize(snapPointItem.FillBrush.Value))));
            list.Add(new XElement("EdgeThickness", snapPointItem.EdgeThickness.Value));
            list.Add(new XElement("PathGeometry", snapPointItem.PathGeometry.Value));
            var snappointItemXML = new XElement("SnapPointItem", list);
            return snappointItemXML;
        }

        throw new Exception("Neither DesinerItem nor ConnectorItem");
    }

    private static string PointsToStr(ObservableCollection<Point> points)
    {
        var ret = string.Empty;
        foreach (var point in points)
        {
            ret += $"{point.X},{point.Y}";
            if (point != points.Last())
                ret += " ";
        }

        return ret;
    }

    public static IEnumerable<XElement> SerializeConnections(DiagramViewModel dialogViewModel,
        IEnumerable<SelectableDesignerItemViewModelBase> items)
    {
        return (from connection in items.WithPickupChildren(dialogViewModel.Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .Where(x => x is LayerItem)
                    .Select(x => (x as LayerItem).Item.Value)
                ).OfType<ConnectorBaseViewModel>()
                where connection is not BezierCurveViewModel
                select new XElement("Connection",
                    new XElement("ID", connection.ID),
                    new XElement("ParentID", connection.ParentID),
                    new XElement("Type", connection.GetType().FullName),
                    new XElement("BeginPoint", connection.Points[0]),
                    new XElement("EndPoint", connection.Points[1]),
                    new XElement("ZIndex", connection.ZIndex.Value),
                    new XElement("EdgeBrush",
                        XElement.Parse(WpfObjectSerializer.Serialize(connection.EdgeBrush.Value))),
                    new XElement("EdgeThickness", connection.EdgeThickness.Value),
                    new XElement("PathGeometry", connection.PathGeometry.Value)
                ))
            .Union(
                from connection in items.WithPickupChildren(dialogViewModel.Layers
                    .SelectRecursive<LayerTreeViewItemBase, LayerTreeViewItemBase>(x => x.Children)
                    .Where(x => x is LayerItem)
                    .Select(x => (x as LayerItem).Item.Value)
                ).OfType<BezierCurveViewModel>()
                select new XElement("Connection",
                    new XElement("ID", connection.ID),
                    new XElement("ParentID", connection.ParentID),
                    new XElement("Type", connection.GetType().FullName),
                    new XElement("BeginPoint", connection.Points[0]),
                    new XElement("EndPoint", connection.Points[1]),
                    new XElement("ZIndex", connection.ZIndex.Value),
                    new XElement("EdgeBrush",
                        XElement.Parse(WpfObjectSerializer.Serialize(connection.EdgeBrush.Value))),
                    new XElement("EdgeThickness", connection.EdgeThickness.Value),
                    new XElement("ControlPoint1", connection.ControlPoint1.Value),
                    new XElement("ControlPoint2", connection.ControlPoint2.Value),
                    new XElement("PathGeometry", connection.PathGeometry.Value)
                ));
    }

    public static IEnumerable<XElement> SerializeConfiguration(DiagramViewModel diagramViewModel)
    {
        return new[]
        {
            new("Left", diagramViewModel.BackgroundItem.Value.Left.Value),
            new XElement("Top", diagramViewModel.BackgroundItem.Value.Top.Value),
            new XElement("Width", diagramViewModel.BackgroundItem.Value.Width.Value),
            new XElement("Height", diagramViewModel.BackgroundItem.Value.Height.Value),
            new XElement("CanvasFillBrush",
                XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.CanvasFillBrush.Value))),
            new XElement("EnablePointSnap", diagramViewModel.EnablePointSnap.Value),
            new XElement("SnapPower", diagramViewModel.MainWindowVM.SnapPower.Value),
            XElement_ColorSpots(diagramViewModel)
        };
    }

    private static XElement XElement_ColorSpots(DiagramViewModel diagramViewModel)
    {
        var colorSpots = new XElement("ColorSpots");
        colorSpots.Add(new XElement("ColorSpot0",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot0))));
        colorSpots.Add(new XElement("ColorSpot1",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot1))));
        colorSpots.Add(new XElement("ColorSpot2",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot2))));
        colorSpots.Add(new XElement("ColorSpot3",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot3))));
        colorSpots.Add(new XElement("ColorSpot4",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot4))));
        colorSpots.Add(new XElement("ColorSpot5",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot5))));
        colorSpots.Add(new XElement("ColorSpot6",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot6))));
        colorSpots.Add(new XElement("ColorSpot7",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot7))));
        colorSpots.Add(new XElement("ColorSpot8",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot8))));
        colorSpots.Add(new XElement("ColorSpot9",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot9))));
        colorSpots.Add(new XElement("ColorSpot10",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot10))));
        colorSpots.Add(new XElement("ColorSpot11",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot11))));
        colorSpots.Add(new XElement("ColorSpot12",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot12))));
        colorSpots.Add(new XElement("ColorSpot13",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot13))));
        colorSpots.Add(new XElement("ColorSpot14",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot14))));
        colorSpots.Add(new XElement("ColorSpot15",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot15))));
        colorSpots.Add(new XElement("ColorSpot16",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot16))));
        colorSpots.Add(new XElement("ColorSpot17",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot17))));
        colorSpots.Add(new XElement("ColorSpot18",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot18))));
        colorSpots.Add(new XElement("ColorSpot19",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot19))));
        colorSpots.Add(new XElement("ColorSpot20",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot20))));
        colorSpots.Add(new XElement("ColorSpot21",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot21))));
        colorSpots.Add(new XElement("ColorSpot22",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot22))));
        colorSpots.Add(new XElement("ColorSpot23",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot23))));
        colorSpots.Add(new XElement("ColorSpot24",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot24))));
        colorSpots.Add(new XElement("ColorSpot25",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot25))));
        colorSpots.Add(new XElement("ColorSpot26",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot26))));
        colorSpots.Add(new XElement("ColorSpot27",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot27))));
        colorSpots.Add(new XElement("ColorSpot28",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot28))));
        colorSpots.Add(new XElement("ColorSpot29",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot29))));
        colorSpots.Add(new XElement("ColorSpot30",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot30))));
        colorSpots.Add(new XElement("ColorSpot31",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot31))));
        colorSpots.Add(new XElement("ColorSpot32",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot32))));
        colorSpots.Add(new XElement("ColorSpot33",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot33))));
        colorSpots.Add(new XElement("ColorSpot34",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot34))));
        colorSpots.Add(new XElement("ColorSpot35",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot35))));
        colorSpots.Add(new XElement("ColorSpot36",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot36))));
        colorSpots.Add(new XElement("ColorSpot37",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot37))));
        colorSpots.Add(new XElement("ColorSpot38",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot38))));
        colorSpots.Add(new XElement("ColorSpot39",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot39))));
        colorSpots.Add(new XElement("ColorSpot40",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot40))));
        colorSpots.Add(new XElement("ColorSpot41",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot41))));
        colorSpots.Add(new XElement("ColorSpot42",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot42))));
        colorSpots.Add(new XElement("ColorSpot43",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot43))));
        colorSpots.Add(new XElement("ColorSpot44",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot44))));
        colorSpots.Add(new XElement("ColorSpot45",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot45))));
        colorSpots.Add(new XElement("ColorSpot46",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot46))));
        colorSpots.Add(new XElement("ColorSpot47",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot47))));
        colorSpots.Add(new XElement("ColorSpot48",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot48))));
        colorSpots.Add(new XElement("ColorSpot49",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot49))));
        colorSpots.Add(new XElement("ColorSpot50",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot50))));
        colorSpots.Add(new XElement("ColorSpot51",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot51))));
        colorSpots.Add(new XElement("ColorSpot52",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot52))));
        colorSpots.Add(new XElement("ColorSpot53",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot53))));
        colorSpots.Add(new XElement("ColorSpot54",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot54))));
        colorSpots.Add(new XElement("ColorSpot55",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot55))));
        colorSpots.Add(new XElement("ColorSpot56",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot56))));
        colorSpots.Add(new XElement("ColorSpot57",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot57))));
        colorSpots.Add(new XElement("ColorSpot58",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot58))));
        colorSpots.Add(new XElement("ColorSpot59",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot59))));
        colorSpots.Add(new XElement("ColorSpot60",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot60))));
        colorSpots.Add(new XElement("ColorSpot61",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot61))));
        colorSpots.Add(new XElement("ColorSpot62",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot62))));
        colorSpots.Add(new XElement("ColorSpot63",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot63))));
        colorSpots.Add(new XElement("ColorSpot64",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot64))));
        colorSpots.Add(new XElement("ColorSpot65",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot65))));
        colorSpots.Add(new XElement("ColorSpot66",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot66))));
        colorSpots.Add(new XElement("ColorSpot67",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot67))));
        colorSpots.Add(new XElement("ColorSpot68",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot68))));
        colorSpots.Add(new XElement("ColorSpot69",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot69))));
        colorSpots.Add(new XElement("ColorSpot70",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot70))));
        colorSpots.Add(new XElement("ColorSpot71",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot71))));
        colorSpots.Add(new XElement("ColorSpot72",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot72))));
        colorSpots.Add(new XElement("ColorSpot73",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot73))));
        colorSpots.Add(new XElement("ColorSpot74",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot74))));
        colorSpots.Add(new XElement("ColorSpot75",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot75))));
        colorSpots.Add(new XElement("ColorSpot76",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot76))));
        colorSpots.Add(new XElement("ColorSpot77",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot77))));
        colorSpots.Add(new XElement("ColorSpot78",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot78))));
        colorSpots.Add(new XElement("ColorSpot79",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot79))));
        colorSpots.Add(new XElement("ColorSpot80",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot80))));
        colorSpots.Add(new XElement("ColorSpot81",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot81))));
        colorSpots.Add(new XElement("ColorSpot82",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot82))));
        colorSpots.Add(new XElement("ColorSpot83",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot83))));
        colorSpots.Add(new XElement("ColorSpot84",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot84))));
        colorSpots.Add(new XElement("ColorSpot85",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot85))));
        colorSpots.Add(new XElement("ColorSpot86",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot86))));
        colorSpots.Add(new XElement("ColorSpot87",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot87))));
        colorSpots.Add(new XElement("ColorSpot88",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot88))));
        colorSpots.Add(new XElement("ColorSpot89",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot89))));
        colorSpots.Add(new XElement("ColorSpot90",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot90))));
        colorSpots.Add(new XElement("ColorSpot91",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot91))));
        colorSpots.Add(new XElement("ColorSpot92",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot92))));
        colorSpots.Add(new XElement("ColorSpot93",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot93))));
        colorSpots.Add(new XElement("ColorSpot94",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot94))));
        colorSpots.Add(new XElement("ColorSpot95",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot95))));
        colorSpots.Add(new XElement("ColorSpot96",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot96))));
        colorSpots.Add(new XElement("ColorSpot97",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot97))));
        colorSpots.Add(new XElement("ColorSpot98",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot98))));
        colorSpots.Add(new XElement("ColorSpot99",
            XElement.Parse(WpfObjectSerializer.Serialize(diagramViewModel.ColorSpots.Value.ColorSpot99))));
        return colorSpots;
    }

    internal static IEnumerable<XElement> SerializeAttachments()
    {
        using (var memStream = new MemoryStream())
        {
            var renderer = new Renderer(new WpfVisualTreeHelper());
            var image = renderer.Render(null, DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value, DiagramViewModel.Instance.BackgroundItem.Value);
            var writeableBitmap = new WriteableBitmap(image);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(writeableBitmap));
            encoder.Save(memStream);

            // 入力文字列をASCIIエンコードしたバイト配列
            byte[] bytes = memStream.ToArray();

            // Base64エンコードした文字列
            string base64String = Convert.ToBase64String(bytes);

            // 必要な出力文字数を計算する
            int outputCount = base64String.Length + (base64String.Length / 76) + 1;

            var picture = new XElement("Picture");
            var source = string.Empty;
            // Base64エンコードした文字列を76文字ごとに分割する
            for (int i = 0; i < base64String.Length; i += 76)
            {
                int charCount = Math.Min(76, base64String.Length - i);
                string line = base64String.Substring(i, charCount);
                source += line;
            }
            picture.SetAttributeValue("Source", source);

            return new[]
            {
                picture
            };
        }
    }
}