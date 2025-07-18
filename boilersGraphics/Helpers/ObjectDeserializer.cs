using boilersGraphics.Exceptions;
using boilersGraphics.Models;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.ColorCorrect;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using ZLinq;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Point = System.Windows.Point;

namespace boilersGraphics.Helpers;

public class ObjectDeserializer
{
    private static SelectableDesignerItemViewModelBase DeserializeInstance(XElement designerItemXML)
    {
        var className = designerItemXML.Element("Type").Value;
        return (SelectableDesignerItemViewModelBase)Activator
            .CreateInstance(Assembly.GetExecutingAssembly().GetName().Name, className).Unwrap();
    }

    public static void ReadCopyObjectsFromXML(DiagramViewModel diagramViewModel, XElement root)
    {
        var copyObjs = root.Elements().AsValueEnumerable().Where(x => x.Name == "CopyObjects").FirstOrDefault();
        if (copyObjs is null)
            throw new UnexpectedException("must be copyObjs is not null");
        var layers = copyObjs.Elements().AsValueEnumerable().Where(x => x.Name == "Layers").FirstOrDefault();
        var layerItems = copyObjs.Elements().AsValueEnumerable().Where(x => x.Name == "LayerItems").FirstOrDefault();
        if (layers is null && layerItems is null)
            throw new UnexpectedException("must be layers is not null or items is not null");
        if (layers is not null)
        {
            foreach (var layer in layers.Descendants("Layer"))
            {
                var layerObj = new Layer();
                layerObj.Color.Value = (Color)ColorConverter.ConvertFromString(layer.Element("Color").Value);
                layerObj.IsVisible.Value = bool.Parse(layer.Element("IsVisible").Value);
                layerObj.Name.Value = layer.Element("Name").Value;

                foreach (var layerItemsInternal in layer.Descendants("LayerItems"))
                foreach (var layerItem in layerItemsInternal.Descendants("LayerItem"))
                {
                    var layerItemObj = ReadLayerItemFromXML(diagramViewModel, layerObj, layerItem);
                    if (layerItemObj is null)
                        continue;
                    layerObj.Children.Add(layerItemObj);
                }

                diagramViewModel.Layers.Add(layerObj);
            }
        }
        else if (layerItems is not null)
        {
            var layerObj = diagramViewModel.SelectedLayers.Value.AsValueEnumerable().First();
            foreach (var layerItem in layerItems.Descendants("LayerItem"))
            {
                if (layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("DesignerItem").AsValueEnumerable().Count() == 0)
                    break;
                var designerItemObj = ExtractDesignerItemViewModelBase(diagramViewModel,
                    layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("DesignerItem").AsValueEnumerable().First());
                if (designerItemObj is null)
                    continue;
                var layerItemObj = new LayerItem(designerItemObj, layerObj, layerItem.Element("Name").Value);
                layerItemObj.IsVisible.Value = bool.Parse(layerItem.Element("IsVisible").Value);
                layerObj.Children.Add(layerItemObj);
            }

            foreach (var layerItem in layerItems.Descendants("LayerItem"))
            {
                if (layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("ConnectorItem").AsValueEnumerable().Count() == 0)
                    break;
                var connectorObj = ExtractConnectorBaseViewModel(diagramViewModel,
                    layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("ConnectorItem").AsValueEnumerable().First());
                if (connectorObj is null)
                    continue;
                var layerItemObj = new LayerItem(connectorObj, layerObj, layerItem.Element("Name").Value);
                layerItemObj.IsVisible.Value = bool.Parse(layerItem.Element("IsVisible").Value);
                layerObj.Children.Add(layerItemObj);
            }
        }
    }

    public static int CountObjectsFromXML(XElement root)
    {
        var ret = 0;
        var layers = root.Elements().AsValueEnumerable().FirstOrDefault(x => x.Name == "Layers");
        if (layers is not null)
        {
            foreach (var layer in layers.Elements("Layer"))
            {
                foreach (var layerItemsInternal in layer.Elements("Children"))
                foreach (var layerItem in layerItemsInternal.Elements("LayerItem"))
                {
                    ret++;
                }
            }
        }
        else
        {
            //読み込むファイルにLayers要素がない場合、初期レイヤーに全てのアイテムを突っ込む
            foreach (var designerItems in root.Elements("DesignerItems"))
            foreach (var designerItem in designerItems.Elements("DesignerItem"))
            {
                ret++;
            }

            foreach (var connections in root.Elements("Connections"))
            foreach (var connector in connections.Elements("Connection"))
            {
                ret++;
            }
        }

        return ret;
    }

    public static void ReadObjectsFromXML(DiagramViewModel diagramViewModel,
        ProgressBarWithOutputViewModel progressBarWithOutputViewModel, XElement root, bool isPreview = false)
    {
        var layers = root.Elements().AsValueEnumerable().Where(x => x.Name == "Layers").FirstOrDefault();
        if (layers is not null)
        {
            foreach (var layer in layers.Elements("Layer"))
            {
                var layerObj = new Layer(isPreview);
                layerObj.Color.Value = (Color)ColorConverter.ConvertFromString(layer.Element("Color").Value);
                layerObj.IsVisible.Value = bool.Parse(layer.Element("IsVisible").Value);
                if (layer.Elements("IsExpanded").AsValueEnumerable().Any())
                {
                    layerObj.IsExpanded.Value = bool.Parse(layer.Element("IsExpanded").Value);
                }
                layerObj.Name.Value = layer.Element("Name").Value;

                foreach (var layerItemsInternal in layer.Elements("Children"))
                foreach (var layerItem in layerItemsInternal.Elements("LayerItem"))
                {
                    var layerItemObj = ReadLayerItemFromXML(diagramViewModel, layerObj, layerItem);
                    if (layerItemObj is null)
                        continue;
                    layerObj.Children.Add(layerItemObj);
                    Invoke(() =>
                    {
                        if (progressBarWithOutputViewModel is null)
                            return;
                        progressBarWithOutputViewModel.Output.Value += Environment.NewLine;
                        progressBarWithOutputViewModel.Output.Value += $"{Resources.String_Loaded}：{layerItemObj.Name.Value}";
                        progressBarWithOutputViewModel.Current.Value++;
                    }, DispatcherPriority.ApplicationIdle);
                }

                diagramViewModel.Layers.Add(layerObj);
                Invoke(() =>
                {
                    if (progressBarWithOutputViewModel is null)
                        return;
                    progressBarWithOutputViewModel.Output.Value += Environment.NewLine;
                    progressBarWithOutputViewModel.Output.Value += $"{Resources.String_Loaded}：{layerObj.Name.Value}";
                    progressBarWithOutputViewModel.Current.Value++;
                }, DispatcherPriority.ApplicationIdle);
            }
        }
        else
        {
            var layerObj = new Layer();
            var rand = new Random();
            layerObj.Color.Value = Randomizer.RandomColor(rand);
            layerObj.IsVisible.Value = true;
            layerObj.Name.Value = Name.GetNewLayerName(diagramViewModel);

            //読み込むファイルにLayers要素がない場合、初期レイヤーに全てのアイテムを突っ込む
            foreach (var designerItems in root.Elements("DesignerItems"))
            foreach (var designerItem in designerItems.Elements("DesignerItem"))
            {
                var item = ExtractDesignerItemViewModelBase(diagramViewModel, designerItem);
                var layerItem = new LayerItem(item, layerObj, Name.GetNewLayerItemName(diagramViewModel));
                layerItem.Color.Value = Randomizer.RandomColor(rand);
                layerObj.Children.Add(layerItem);
                Invoke(() =>
                {
                    if (progressBarWithOutputViewModel is null)
                        return;
                    progressBarWithOutputViewModel.Output.Value += Environment.NewLine;
                    progressBarWithOutputViewModel.Output.Value += $"{Resources.String_Loaded}：{layerItem.Name.Value}";
                    progressBarWithOutputViewModel.Current.Value++;
                }, DispatcherPriority.ApplicationIdle);
            }

            foreach (var connections in root.Elements("Connections"))
            foreach (var connector in connections.Elements("Connection"))
            {
                var item = ExtractConnectorBaseViewModel(diagramViewModel, connector);
                var layerItem = new LayerItem(item, layerObj, Name.GetNewLayerItemName(diagramViewModel));
                layerItem.Color.Value = Randomizer.RandomColor(rand);
                layerObj.Children.Add(layerItem);
                Invoke(() =>
                {
                    if (progressBarWithOutputViewModel is null)
                        return;
                    progressBarWithOutputViewModel.Output.Value += Environment.NewLine;
                    progressBarWithOutputViewModel.Output.Value += $"{Resources.String_Loaded}：{layerItem.Name.Value}";
                    progressBarWithOutputViewModel.Current.Value++;
                }, DispatcherPriority.ApplicationIdle);
            }

            diagramViewModel.Layers.Add(layerObj);
            Invoke(() =>
            {
                if (progressBarWithOutputViewModel is null)
                    return;
                progressBarWithOutputViewModel.Output.Value += Environment.NewLine;
                progressBarWithOutputViewModel.Output.Value += $"{Resources.String_Loaded}：{layerObj.Name.Value}";
                progressBarWithOutputViewModel.Current.Value++;
            }, DispatcherPriority.ApplicationIdle);
        }
    }

    private static LayerItem ReadLayerItemFromXML(DiagramViewModel diagramViewModel, Layer layerObj, XElement layerItem)
    {
        if (layerItem is null)
            return null;
        DesignerItemViewModelBase designerItemObj = null;
        ConnectorBaseViewModel connectorObj = null;
        SnapPointViewModel snapPointObj = null;
        if (layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("DesignerItem").AsValueEnumerable().Count() >= 1)
            designerItemObj = ExtractDesignerItemViewModelBase(diagramViewModel,
                layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("DesignerItem").AsValueEnumerable().First());
        if (layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("ConnectorItem").AsValueEnumerable().Count() >= 1)
            connectorObj = ExtractConnectorBaseViewModel(diagramViewModel,
                layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("ConnectorItem").AsValueEnumerable().First());
        if (layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("SnapPointItem").AsValueEnumerable().Count() >= 1)
            snapPointObj = ExtractSnapPointViewModel(diagramViewModel,
                layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("SnapPointItem").AsValueEnumerable().First());
        var item = EitherNotNull(designerItemObj, EitherNotNull(connectorObj, snapPointObj));
        if (item is null)
            throw new UnexpectedException("All of them are null.");
        var layerItemObj = new LayerItem(item, layerObj, layerItem.Element("Name").Value);
        layerItemObj.Color.Value = (Color)ColorConverter.ConvertFromString(layerItem.Element("Color").Value);
        layerItemObj.IsVisible.Value = bool.Parse(layerItem.Element("IsVisible").Value);
        if (layerItem.Elements("IsExpanded").AsValueEnumerable().Any())
        {
            layerItemObj.IsExpanded.Value = bool.Parse(layerItem.Element("IsExpanded").Value);
        }
        var children = layerItem.Elements("Children").Descendants("LayerItem");
        var children_layerItems = children.AsValueEnumerable()
            .Select(child => ReadLayerItemFromXML(diagramViewModel, layerObj, child)).Where(x => x is not null);
        foreach (var c in children_layerItems)
        {
            layerItemObj.Children.Add(c);

            //グループの場合、子をグループに追加する
            if (item is GroupItemViewModel groupItemVM)
                groupItemVM.AddGroup(diagramViewModel.MainWindowVM.Recorder, c.Item.Value);
        }

        return layerItemObj;
    }

    private static SelectableDesignerItemViewModelBase EitherNotNull(SelectableDesignerItemViewModelBase left,
        SelectableDesignerItemViewModelBase right)
    {
        if (left is not null && right is not null)
            return null;
        if (left is not null)
            return left;
        if (right is not null)
            return right;
        return null;
    }

    private static List<SelectableDesignerItemViewModelBase> ExtractItems(DiagramViewModel diagramViewModel,
        IEnumerable<XElement> designerItemsElm, IEnumerable<XElement> connectorsElm)
    {
        var list = new List<SelectableDesignerItemViewModelBase>();
        foreach (var designerItemElm in designerItemsElm)
        {
            var item = ExtractDesignerItemViewModelBase(diagramViewModel, designerItemElm);
            list.Add(item);
        }

        foreach (var connectorElm in connectorsElm)
        {
            var item = ExtractConnectorBaseViewModel(diagramViewModel, connectorElm);
            list.Add(item);
        }

        //grouping
        foreach (var groupItem in list.AsValueEnumerable().OfType<GroupItemViewModel>().ToList())
        {
            var children = list.AsValueEnumerable().Where(item => item.ParentID == groupItem.ID);
            children.ToList().ForEach(x => groupItem.AddGroup(diagramViewModel.MainWindowVM.Recorder, x));
        }

        return list;
    }

    private static SnapPointViewModel ExtractSnapPointViewModel(DiagramViewModel diagramViewModel,
        XElement snapPointElm)
    {
        if (!(DeserializeInstance(snapPointElm) is SnapPointViewModel item))
            return null;
        item.ID = Guid.Parse(snapPointElm.Element("ID").Value);
        item.ParentID = Guid.Parse(snapPointElm.Element("ParentID").Value);
        item.Left.Value = double.Parse(snapPointElm.Element("Left").Value);
        item.Top.Value = double.Parse(snapPointElm.Element("Top").Value);
        item.Width.Value = double.Parse(snapPointElm.Element("Width").Value);
        item.Height.Value = double.Parse(snapPointElm.Element("Height").Value);
        item.ZIndex.Value = int.Parse(snapPointElm.Element("ZIndex").Value);
        item.Matrix.Value = new Matrix();
        if (snapPointElm.Element("EdgeColor") is not null)
            item.EdgeBrush.Value =
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(snapPointElm.Element("EdgeColor").Value));
        else
            item.EdgeBrush.Value =
                WpfObjectSerializer.Deserialize(snapPointElm.Element("EdgeBrush").Nodes().AsValueEnumerable().First().ToString()) as Brush;
        if (snapPointElm.Element("FillColor") is not null)
            item.FillBrush.Value =
                new SolidColorBrush((Color)ColorConverter.ConvertFromString(snapPointElm.Element("FillColor").Value));
        else
            item.FillBrush.Value =
                WpfObjectSerializer.Deserialize(snapPointElm.Element("FillBrush").Nodes().AsValueEnumerable().First().ToString()) as Brush;
        item.EdgeThickness.Value = double.Parse(snapPointElm.Element("EdgeThickness").Value);
        item.PathGeometryNoRotate.Value =
            PathGeometry.CreateFromGeometry(Geometry.Parse(snapPointElm.Element("PathGeometry").Value));
        item.Opacity.Value = 0.5;
        item.Owner = diagramViewModel;
        return item;
    }

    private static ConnectorBaseViewModel ExtractConnectorBaseViewModel(DiagramViewModel diagramViewModel,
        XElement connectorElm)
    {
        var instance = DeserializeInstance(connectorElm);
        if (instance is not ConnectorBaseViewModel)
            return null;
        var item = instance as ConnectorBaseViewModel;
        item.IsHitTestVisible.Value = true;
        item.ID = Guid.Parse(connectorElm.Element("ID").Value);
        item.ParentID = Guid.Parse(connectorElm.Element("ParentID").Value);
        item.Points = new ObservableCollection<Point>();
        if (item is StraightConnectorViewModel || item is BezierCurveViewModel)
            item.AddPoints(diagramViewModel, Point.Parse(connectorElm.Element("BeginPoint").Value),
                Point.Parse(connectorElm.Element("EndPoint").Value));
        item.ZIndex.Value = int.Parse(connectorElm.Element("ZIndex").Value);
        item.EdgeBrush.Value =
            WpfObjectSerializer.Deserialize(connectorElm.Element("EdgeBrush").Nodes().AsValueEnumerable().First().ToString()) as Brush;
        item.EdgeThickness.Value = double.Parse(connectorElm.Element("EdgeThickness").Value);
        if (connectorElm.Elements("StrokeLineJoin").AsValueEnumerable().Any())
            item.StrokeLineJoin.Value = Enum.Parse<PenLineJoin>(connectorElm.Element("StrokeLineJoin").Value);
        if (connectorElm.Elements("StrokeMiterLimit").AsValueEnumerable().Any())
            item.StrokeMiterLimit.Value = double.Parse(connectorElm.Element("StrokeMiterLimit").Value);
        if (connectorElm.Elements("StrokeDashArray").AsValueEnumerable().Any())
            item.StrokeDashArray.Value = DoubleCollection.Parse(connectorElm.Element("StrokeDashArray").Value);
        item.LeftTop.Value = Point.Parse(connectorElm.Element("LeftTop").Value);
        if (item is StraightConnectorViewModel || item is BezierCurveViewModel)
        {
            item.PathGeometryNoRotate.Value =
                PathGeometry.CreateFromGeometry(Geometry.Parse(connectorElm.Element("PathGeometry").Value));
        }
        else if (item is PolyBezierViewModel poly)
        {
            poly.Points = StrToPoints(connectorElm.Element("Points").Value);
            poly.InitializeSnapPoints(poly.Points.AsValueEnumerable().First(), poly.Points.AsValueEnumerable().Last());
            item.PathGeometryNoRotate.Value = GeometryCreator.CreatePolyBezier(poly);
        }

        item.Owner = diagramViewModel;
        if (item is BezierCurveViewModel bezier)
        {
            bezier.ControlPoint1.Value = Point.Parse(connectorElm.Element("ControlPoint1").Value);
            bezier.ControlPoint2.Value = Point.Parse(connectorElm.Element("ControlPoint2").Value);
        }

        item.InitIsSelectedOnSnapPoints();

        return item;
    }

    private static ObservableCollection<Point> StrToPoints(string value)
    {
        var points = new ObservableCollection<Point>();
        foreach (var point in value.Split(' '))
        {
            var splits = point.Split(',');
            points.Add(new Point(double.Parse(splits[0]), double.Parse(splits[1])));
        }

        return points;
    }

    private static DesignerItemViewModelBase ExtractDesignerItemViewModelBase(DiagramViewModel diagramViewModel,
        XElement designerItemElm)
    {
        if (!(DeserializeInstance(designerItemElm) is DesignerItemViewModelBase))
            return null;
        var item = (DesignerItemViewModelBase)DeserializeInstance(designerItemElm);
        if (designerItemElm.Element("PathGeometry") is not null)
            item.PathGeometryNoRotate.Value =
                PathGeometry.CreateFromGeometry(Geometry.Parse(designerItemElm.Element("PathGeometry").Value));
        if (designerItemElm.Element("PathGeometryNoRotate") is not null)
            item.PathGeometryNoRotate.Value =
                PathGeometry.CreateFromGeometry(Geometry.Parse(designerItemElm.Element("PathGeometryNoRotate").Value));
        if (designerItemElm.Element("PathGeometryRotate") is not null)
            item.PathGeometryRotate.Value =
                PathGeometry.CreateFromGeometry(Geometry.Parse(designerItemElm.Element("PathGeometryRotate").Value));
        item.RenderingEnabled.Value = false;
        item.IsHitTestVisible.Value = true;
        item.Left.Value = double.Parse(designerItemElm.Element("Left").Value);
        item.Top.Value = double.Parse(designerItemElm.Element("Top").Value);
        item.Width.Value = double.Parse(designerItemElm.Element("Width").Value);
        item.Height.Value = double.Parse(designerItemElm.Element("Height").Value);
        item.ID = Guid.Parse(designerItemElm.Element("ID").Value);
        item.ParentID = Guid.Parse(designerItemElm.Element("ParentID").Value);
        item.ZIndex.Value = int.Parse(designerItemElm.Element("ZIndex").Value);
        //item.Matrix.Value = Matrix.Parse(designerItemElm.Element("Matrix").Value);
        if (designerItemElm.Element("EdgeColor") is not null)
            item.EdgeBrush.Value =
                new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(designerItemElm.Element("EdgeColor").Value));
        else
            item.EdgeBrush.Value =
                WpfObjectSerializer.Deserialize(designerItemElm.Element("EdgeBrush").Nodes().AsValueEnumerable().First().ToString()) as
                    Brush;
        if (designerItemElm.Element("FillColor") is not null)
            item.FillBrush.Value =
                new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(designerItemElm.Element("FillColor").Value));
        else
            item.FillBrush.Value =
                WpfObjectSerializer.Deserialize(designerItemElm.Element("FillBrush").Nodes().AsValueEnumerable().First().ToString()) as
                    Brush;
        if (designerItemElm.Elements("StrokeLineJoin").AsValueEnumerable().Any())
            item.StrokeLineJoin.Value = Enum.Parse<PenLineJoin>(designerItemElm.Element("StrokeLineJoin").Value);
        if (designerItemElm.Elements("StrokeMiterLimit").AsValueEnumerable().Any())
            item.StrokeMiterLimit.Value = double.Parse(designerItemElm.Element("StrokeMiterLimit").Value);
        if (designerItemElm.Elements("StrokeDashArray").AsValueEnumerable().Any())
            item.StrokeDashArray.Value = DoubleCollection.Parse(designerItemElm.Element("StrokeDashArray").Value);
        item.EdgeThickness.Value = double.Parse(designerItemElm.Element("EdgeThickness").Value);
        item.RotationAngle.Value = designerItemElm.Element("RotationAngle") is not null
            ? double.Parse(designerItemElm.Element("RotationAngle").Value)
            : 0;
        item.Owner = diagramViewModel;
        if (item is NRectangleViewModel rectangle)
        {
            if (designerItemElm.Elements("RadiusX").AsValueEnumerable().Any())
                rectangle.RadiusX.Value = double.Parse(designerItemElm.Element("RadiusX").Value);
            if (designerItemElm.Elements("RadiusY").AsValueEnumerable().Any())
                rectangle.RadiusY.Value = double.Parse(designerItemElm.Element("RadiusY").Value);
        }

        if (item is PictureDesignerItemViewModel picture)
        {
            if (designerItemElm.Elements("EnableImageEmbedding").AsValueEnumerable().Any() &&
                bool.TryParse(designerItemElm.Element("EnableImageEmbedding").Value, out var enableImageEmbedding))
                picture.EmbeddedImage.Value =
                    Base64StringToBitmap(designerItemElm.Element("EmbeddedImageBase64").Value);
            else
                picture.FileName = designerItemElm.Element("FileName").Value;
        }

        if (item is CroppedPictureDesignerItemViewModel cropped)
            if (designerItemElm.Elements("EnableImageEmbedding").AsValueEnumerable().Any() &&
                bool.TryParse(designerItemElm.Element("EnableImageEmbedding").Value, out var enableImageEmbedding))
                cropped.EmbeddedImage.Value =
                    Base64StringToBitmap(designerItemElm.Element("EmbeddedImageBase64").Value);
        if (item is MosaicViewModel mosaic)
        {
            if (designerItemElm.Elements("ColumnPixels").AsValueEnumerable().Any())
                mosaic.ColumnPixels.Value = double.Parse(designerItemElm.Element("ColumnPixels").Value);
            if (designerItemElm.Elements("RowPixels").AsValueEnumerable().Any())
                mosaic.RowPixels.Value = double.Parse(designerItemElm.Element("RowPixels").Value);
        }

        if (item is BlurEffectViewModel blurEffect)
        {
            if (designerItemElm.Elements("KernelWidth").AsValueEnumerable().Any())
                blurEffect.KernelWidth.Value = double.Parse(designerItemElm.Element("KernelWidth").Value);
            if (designerItemElm.Elements("KernelHeight").AsValueEnumerable().Any())
                blurEffect.KernelHeight.Value = double.Parse(designerItemElm.Element("KernelHeight").Value);
            if (designerItemElm.Elements("Sigma").AsValueEnumerable().Any())
                blurEffect.Sigma.Value = double.Parse(designerItemElm.Element("Sigma").Value);
        }

        if (item is ColorCorrectViewModel colorCorrect)
        {
            if (designerItemElm.Elements("CCType").AsValueEnumerable().Any())
                colorCorrect.CCType.Value = GetCorrespondingStaticValue<ColorCorrectType>(designerItemElm.Element("CCType").Value);

            if (colorCorrect.CCType.Value == ColorCorrectType.HSV)
            {
                if (designerItemElm.Elements("AddHue").AsValueEnumerable().Any())
                    colorCorrect.AddHue.Value = int.Parse(designerItemElm.Element("AddHue").Value);
                if (designerItemElm.Elements("AddSaturation").AsValueEnumerable().Any())
                    colorCorrect.AddSaturation.Value = int.Parse(designerItemElm.Element("AddSaturation").Value);
                if (designerItemElm.Elements("AddValue").AsValueEnumerable().Any())
                    colorCorrect.AddValue.Value = int.Parse(designerItemElm.Element("AddValue").Value);
            }
            else if (colorCorrect.CCType.Value == ColorCorrectType.ToneCurve)
            {
                if (designerItemElm.Elements("TargetChannel").AsValueEnumerable().Any())
                {
                    colorCorrect.TargetChannel.Value = GetCorrespondingStaticValue<Channel>(designerItemElm.Element("TargetChannel").Value);
                }

                if (designerItemElm.Elements("Curves").AsValueEnumerable().Any())
                {
                    var curvesElm = designerItemElm.Element("Curves");

                    if (curvesElm.Elements("Curve").AsValueEnumerable().Any())
                    {

                        foreach (var curveElm in curvesElm.Elements("Curve"))
                        {
                            var curve = new ToneCurveViewModel.Curve();
                            if (curveElm.Elements("Points").AsValueEnumerable().Any())
                            {
                                curve.Points =
                                    new ReactiveCollection<ToneCurveViewModel.Point>();
                                curve.Points.AddRange(curveElm.Elements("Points").AsValueEnumerable()
                                    .SelectMany(x => x.Elements("Point")).Select(x =>
                                        new ToneCurveViewModel.Point(
                                            int.Parse(x.Elements("X").AsValueEnumerable().Any()
                                                ? CastToDoubleRound(x.Element("X").Value)
                                                : "0"),
                                            int.Parse(x.Elements("Y").AsValueEnumerable().Any()
                                                ? CastToDoubleRound(x.Element("Y").Value)
                                                : "0"))
                                    ).ToArray());
                            }

                            if (curveElm.Elements("InOutPairs").AsValueEnumerable().Any())
                            {
                                curve.InOutPairs =
                                    new ReactiveCollection<InOutPair>();
                                curve.InOutPairs.AddRange(curveElm.Elements("InOutPairs").AsValueEnumerable()
                                    .SelectMany(x => x.Elements("InOutPair")).Select(x =>
                                        new InOutPair(
                                            int.Parse(x.Elements("In").AsValueEnumerable().Any()
                                                ? CastToDoubleRound(x.Element("In").Value)
                                                : "0"),
                                            int.Parse(x.Elements("Out").AsValueEnumerable().Any()
                                                ? CastToDoubleRound(x.Element("Out").Value)
                                                : "0"))
                                    ).ToArray());
                            }
                            colorCorrect.Curves.Add(curve);
                        }
                    }
                }
            }
            else if (colorCorrect.CCType.Value == ColorCorrectType.NegativePositiveConversion)
            {
                //Do nothing. No need to do anything.
            }
        }

        if (item is LetterDesignerItemViewModel letter)
        {
            letter.LetterString.Value = designerItemElm.Element("LetterString").Value;
            letter.SelectedFontFamily.Value = new FontFamilyEx(designerItemElm.Element("SelectedFontFamily").Value);
            letter.IsBold.Value = bool.Parse(designerItemElm.Element("IsBold").Value);
            letter.IsItalic.Value = bool.Parse(designerItemElm.Element("IsItalic").Value);
            letter.FontSize.Value = int.Parse(designerItemElm.Element("FontSize").Value);
            letter.IsAutoLineBreak.Value = bool.Parse(designerItemElm.Element("AutoLineBreak").Value);
        }

        if (item is LetterVerticalDesignerItemViewModel letterV)
        {
            letterV.LetterString.Value = designerItemElm.Element("LetterString").Value;
            letterV.SelectedFontFamily.Value = new FontFamilyEx(designerItemElm.Element("SelectedFontFamily").Value);
            letterV.IsBold.Value = bool.Parse(designerItemElm.Element("IsBold").Value);
            letterV.IsItalic.Value = bool.Parse(designerItemElm.Element("IsItalic").Value);
            letterV.FontSize.Value = int.Parse(designerItemElm.Element("FontSize").Value);
            letterV.IsAutoLineBreak.Value = bool.Parse(designerItemElm.Element("AutoLineBreak").Value);
        }

        if (item is NPolygonViewModel polygon) polygon.Data.Value = designerItemElm.Element("Data").Value;
        item.UpdatePathGeometryIfEnable(string.Empty, 0, 0, true);
        item.RenderingEnabled.Value = true;
        return item;
    }

    private static string CastToDoubleRound(string value)
    {
        var doubleValue = double.Parse(value);
        doubleValue = double.Round(doubleValue);
        return doubleValue.ToString();
    }

    private static T GetCorrespondingStaticValue<T>(object str) where T : class
    {
        Type type = typeof(T);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
        foreach (var field in fields)
        {
            var fieldValue = field.GetValue(null);
            var fieldValueType = fieldValue.GetType();
            if (str.Equals(fieldValueType.Name))
            {
                return fieldValue as T;
            }
        }
        throw new UnexpectedException("Corresponding property not found.");
    }

    public static BitmapImage Base64StringToBitmap(string base64String)
    {
        var byteBuffer = new List<byte>();
        int length = base64String.Length;
        int index = 0;
        while (length > 0)
        {
            //4文字ずつデコードする
            byteBuffer.AddRange(Convert.FromBase64String(base64String.Substring(index, 4)));
            index += 4;
            length -= 4;
        }

        var bitmapImage = new BitmapImage();
        using (var memStream = new MemoryStream(byteBuffer.ToArray()))
        using (var memStream2 = new MemoryStream())
        {
            var image = Image.FromStream(memStream);
            image.Save(memStream2, ImageFormat.Png);

            memStream2.Position = 0;
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memStream2;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
        }

        return bitmapImage;
    }

    private static void Invoke(Action action, DispatcherPriority priority)
    {
        if (App.IsTest || App.Current is null)
        {
            action();
        }
        else
        {
            App.Current.Dispatcher.Invoke(action, priority);
        }
    }
}