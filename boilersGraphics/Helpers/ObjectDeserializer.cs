using boilersGraphics.Exceptions;
using boilersGraphics.Helpers.Parts;
using boilersGraphics.Models;
using boilersGraphics.Models.Connectors;
using boilersGraphics.Models.Parts;
using boilersGraphics.Properties;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Anchors;
using boilersGraphics.ViewModels.ColorCorrect;
using boilersGraphics.ViewModels.Connectors;
using boilersGraphics.ViewModels.Parts;
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
using boilersGraphics.Extensions;
using ObservableCollections;
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

        var partDefinitionsElm = root.Elements()
            .AsValueEnumerable()
            .Where(x => x.Name == "PartDefinitions")
            .FirstOrDefault();
        if (partDefinitionsElm is not null)
        {
            foreach (var defElm in partDefinitionsElm.Elements("PartDefinition"))
            {
                var vm = ReadPartDefinitionFromXML(diagramViewModel, defElm, assignNewId: false);
                diagramViewModel.PartDefinitions.Add(vm);
            }
        }

        FinalizeAnchorsAndFollowers(diagramViewModel);
    }

    /// <summary>
    /// Phase 3-f §6.2: 全 LayerItem のロード完了後に呼ばれるファイナライゼーション。
    /// AnchorViewModel.RebindOwner() を呼んで Owner DesignerItem との R3 Subscribe を確立し、
    /// 新規コネクタ (Orthogonal / AnchorBezier) の StartAnchorFollowers() で AnchorRef 追従を起動する。
    /// 順序を分けているのは、AllItems が全アイテム揃ってからでないと OwnerId 逆引きが失敗するため。
    /// </summary>
    private static void FinalizeAnchorsAndFollowers(DiagramViewModel diagramViewModel)
    {
        if (diagramViewModel is null) return;
        var allItems = diagramViewModel.AllItems.Value;
        if (allItems is null) return;
        foreach (var anchor in allItems.AsValueEnumerable().OfType<AnchorViewModel>())
            anchor.RebindOwner();
        foreach (var ortho in allItems.AsValueEnumerable().OfType<OrthogonalConnectorViewModel>())
            ortho.StartAnchorFollowers();
        foreach (var anchorBezier in allItems.AsValueEnumerable().OfType<AnchorBezierConnectorViewModel>())
            anchorBezier.StartAnchorFollowers();
    }

    /// <summary>
    /// Build a PartDefinitionViewModel from a single &lt;PartDefinition&gt; element.
    /// Used both by the embedded PartDefinitions section in a .bgff document and by
    /// the standalone .bgpart import path. When <paramref name="assignNewId"/> is true
    /// the imported definition gets a fresh Guid, so importing the same file twice
    /// produces two independent PartDefinitions instead of overwriting.
    /// </summary>
    internal static PartDefinitionViewModel ReadPartDefinitionFromXML(
        DiagramViewModel diagramViewModel,
        XElement defElm,
        bool assignNewId)
    {
        if (defElm is null) throw new ArgumentNullException(nameof(defElm));

        var def = PartDeserializer.DeserializeDefinition(defElm);
        if (assignNewId) def.Id = Guid.NewGuid();
        var vm = new PartDefinitionViewModel(def);

        var itemsElm = defElm.Element("Items");
        if (itemsElm is not null)
        {
            foreach (var designerItemElm in itemsElm.Elements("DesignerItem"))
            {
                var item = ExtractDesignerItemViewModelBase(diagramViewModel, designerItemElm);
                if (item is not null)
                    vm.Items.Add(item);
            }
        }
        return vm;
    }

    private static LayerItem ReadLayerItemFromXML(DiagramViewModel diagramViewModel, Layer layerObj, XElement layerItem)
    {
        if (layerItem is null)
            return null;
        DesignerItemViewModelBase designerItemObj = null;
        ConnectorBaseViewModel connectorObj = null;
        SnapPointViewModel snapPointObj = null;
        AnchorViewModel anchorObj = null;
        if (layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("DesignerItem").AsValueEnumerable().Count() >= 1)
            designerItemObj = ExtractDesignerItemViewModelBase(diagramViewModel,
                layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("DesignerItem").AsValueEnumerable().First());
        if (layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("ConnectorItem").AsValueEnumerable().Count() >= 1)
            connectorObj = ExtractConnectorBaseViewModel(diagramViewModel,
                layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("ConnectorItem").AsValueEnumerable().First());
        if (layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("SnapPointItem").AsValueEnumerable().Count() >= 1)
            snapPointObj = ExtractSnapPointViewModel(diagramViewModel,
                layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("SnapPointItem").AsValueEnumerable().First());
        // Phase 3-f §6.2: AnchorItem ノード分岐 (DesignerItem / Connector / SnapPoint いずれにも該当しない)。
        if (layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("AnchorItem").AsValueEnumerable().Count() >= 1)
            anchorObj = ExtractAnchorViewModel(diagramViewModel,
                layerItem.Descendants("Item").AsValueEnumerable().First().Descendants("AnchorItem").AsValueEnumerable().First());
        var item = EitherNotNull(designerItemObj, EitherNotNull(connectorObj, EitherNotNull(snapPointObj, anchorObj)));
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

    internal static ConnectorBaseViewModel ExtractConnectorBaseViewModel(DiagramViewModel diagramViewModel,
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
        if (item is StraightConnectorViewModel
            || item is BezierCurveViewModel
            || item is OrthogonalConnectorViewModel
            || item is AnchorBezierConnectorViewModel)
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

        // Phase 3-f §6.2: OrthogonalConnectorViewModel の固有プロパティ復元。
        // PathGeometryNoRotate は MidPoints / CornerRadius の Subscribe で RefreshPath() が走るため
        // 明示的な保存値は使わずに再計算結果に任せる (StraightConnector / BezierCurve とは異なる方針)。
        if (item is OrthogonalConnectorViewModel ortho)
        {
            if (connectorElm.Element("OrthogonalRoutingMode") is { } rmElm
                && Enum.TryParse<OrthogonalRoutingMode>(rmElm.Value, out var rm))
                ortho.RoutingMode.Value = rm;
            if (connectorElm.Element("OrthogonalCornerRadius") is { } crElm)
                ortho.CornerRadius.Value = double.Parse(crElm.Value, System.Globalization.CultureInfo.InvariantCulture);
            if (connectorElm.Element("OrthogonalMidPoints") is { } midPointsElm)
            {
                ortho.MidPoints.Clear();
                foreach (var mp in midPointsElm.Elements("MidPoint"))
                {
                    var x = double.Parse(mp.Attribute("X").Value, System.Globalization.CultureInfo.InvariantCulture);
                    var y = double.Parse(mp.Attribute("Y").Value, System.Globalization.CultureInfo.InvariantCulture);
                    ortho.MidPoints.Add(new Point(x, y));
                }
            }
            if (connectorElm.Element("OrthogonalBeginAnchorRef") is { } obae)
                ortho.BeginAnchorRef.Value = obae.Value;
            if (connectorElm.Element("OrthogonalEndAnchorRef") is { } oeae)
                ortho.EndAnchorRef.Value = oeae.Value;
            ortho.RefreshPath();
        }

        // Phase 3-f §6.2: AnchorBezierConnectorViewModel の固有プロパティ復元。
        if (item is AnchorBezierConnectorViewModel anchorBezier)
        {
            if (connectorElm.Element("AnchorBezierBeginControl") is { } abbc)
                anchorBezier.BeginControlPoint.Value = Point.Parse(abbc.Value);
            if (connectorElm.Element("AnchorBezierEndControl") is { } abec)
                anchorBezier.EndControlPoint.Value = Point.Parse(abec.Value);
            if (connectorElm.Element("AnchorBezierBeginAnchorRef") is { } abbae)
                anchorBezier.BeginAnchorRef.Value = abbae.Value;
            if (connectorElm.Element("AnchorBezierEndAnchorRef") is { } abeae)
                anchorBezier.EndAnchorRef.Value = abeae.Value;
            anchorBezier.RefreshPath();
        }

        item.InitIsSelectedOnSnapPoints();

        return item;
    }

    /// <summary>
    /// Phase 3-f §6.2: AnchorViewModel の復元。Owner は呼び出し側で設定する前提で
    /// Properties (OwnerId / RelativeX/Y / AnchorName) のみ書き戻す。
    /// RebindOwner() は ReadObjectsFromXML の最後で一括実行される
    /// (AllItems がロード完了するまで Owner DesignerItem が見つからないため)。
    /// </summary>
    internal static AnchorViewModel ExtractAnchorViewModel(DiagramViewModel diagramViewModel, XElement anchorElm)
    {
        if (!(DeserializeInstance(anchorElm) is AnchorViewModel item))
            return null;
        item.ID = Guid.Parse(anchorElm.Element("ID").Value);
        item.ParentID = Guid.Parse(anchorElm.Element("ParentID").Value);
        item.ZIndex.Value = int.Parse(anchorElm.Element("ZIndex").Value);
        item.OwnerId.Value = Guid.Parse(anchorElm.Element("AnchorOwnerId").Value);
        item.RelativeX.Value = double.Parse(anchorElm.Element("AnchorRelativeX").Value,
            System.Globalization.CultureInfo.InvariantCulture);
        item.RelativeY.Value = double.Parse(anchorElm.Element("AnchorRelativeY").Value,
            System.Globalization.CultureInfo.InvariantCulture);
        if (anchorElm.Element("AnchorName") is { } nameElm)
            item.AnchorName.Value = nameElm.Value;
        item.Owner = diagramViewModel;
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

    internal static DesignerItemViewModelBase ExtractDesignerItemViewModelBase(DiagramViewModel diagramViewModel,
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
        // Phase 3-f: Q-11 案 B / Phase 3-g UI 用 IsNode フラグ。デフォルト false なので、ある時だけ書き戻す。
        if (designerItemElm.Element("IsNode") is { } isNodeElm && bool.TryParse(isNodeElm.Value, out var isNode))
            item.IsNode.Value = isNode;
        item.EdgeThickness.Value = double.Parse(designerItemElm.Element("EdgeThickness").Value);
        item.RotationAngle.Value = designerItemElm.Element("RotationAngle") is not null
            ? double.Parse(designerItemElm.Element("RotationAngle").Value)
            : 0;
        item.Owner = diagramViewModel;
        if (item is EffectViewModel effectVM)
        {
            effectVM.Initialize();
        }
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
                                    new ObservableList<ToneCurveViewModel.Point>().ToWritableNotifyCollectionChanged();
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
                                    new ObservableList<InOutPair>().ToWritableNotifyCollectionChanged();
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

        // Phase 2-e: FUI テキスト要素のデシリアライズ (Q-4 案 A: <DesignerItems> 配下に並ぶ)。
        // Activator.CreateInstance で生成済みの VM に対し、保存済みの共通テキスト属性 + 派生固有プロパティを書き戻す。
        // 派生プロパティ (Type / Seed / Count / ... など) を書き戻すと VM 側の Skip(1) Subscribe が走り、
        // 決定論的な Generator で Text が同じ値に再生成されるため、最後に書き戻す Text と一致する。
        if (item is boilersGraphics.ViewModels.Text.TextElementBaseViewModel textElem)
        {
            if (designerItemElm.Element("Text") is { } textEl) textElem.Text.Value = textEl.Value;
            if (designerItemElm.Element("FontFamily") is { } ff) textElem.FontFamily.Value = ff.Value;
            if (designerItemElm.Element("FontSize") is { } fs) textElem.FontSize.Value = int.Parse(fs.Value);
            if (designerItemElm.Element("Foreground") is { } fgElm && fgElm.Nodes().AsValueEnumerable().Any())
                textElem.Foreground.Value =
                    WpfObjectSerializer.Deserialize(fgElm.Nodes().AsValueEnumerable().First().ToString()) as Brush;
            if (designerItemElm.Element("Background") is { } bgElm && bgElm.Nodes().AsValueEnumerable().Any())
                textElem.Background.Value =
                    WpfObjectSerializer.Deserialize(bgElm.Nodes().AsValueEnumerable().First().ToString()) as Brush;
            if (designerItemElm.Element("LineHeight") is { } lh)
                textElem.LineHeight.Value = double.Parse(lh.Value, System.Globalization.CultureInfo.InvariantCulture);
            if (designerItemElm.Element("LetterSpacing") is { } ls)
                textElem.LetterSpacing.Value = double.Parse(ls.Value, System.Globalization.CultureInfo.InvariantCulture);
            if (designerItemElm.Element("TextOpacity") is { } to)
                textElem.TextOpacity.Value = double.Parse(to.Value, System.Globalization.CultureInfo.InvariantCulture);
            if (designerItemElm.Element("IsWordWrap") is { } ww) textElem.IsWordWrap.Value = bool.Parse(ww.Value);
        }

        if (item is boilersGraphics.ViewModels.Text.DataGeneratorTextBlockViewModel datagen)
        {
            if (designerItemElm.Element("DataGenType") is { } t &&
                Enum.TryParse<boilersGraphics.Models.Text.DataGeneratorType>(t.Value, out var dgType))
                datagen.Type.Value = dgType;
            if (designerItemElm.Element("Seed") is { } seed) datagen.Seed.Value = int.Parse(seed.Value);
            if (designerItemElm.Element("IsSeedLocked") is { } locked) datagen.IsSeedLocked.Value = bool.Parse(locked.Value);
            if (designerItemElm.Element("Count") is { } c) datagen.Count.Value = int.Parse(c.Value);
            if (designerItemElm.Element("DataGenSeparator") is { } sep) datagen.Separator.Value = sep.Value;
            if (designerItemElm.Element("DataGenLayout") is { } lay &&
                Enum.TryParse<boilersGraphics.Models.Text.DataGeneratorLayout>(lay.Value, out var dgLay))
                datagen.Layout.Value = dgLay;
            // Text を改めて Generator 結果と同期 (DataGenSeparator/Layout 等の Subscribe 順序差を埋める)
            datagen.Regenerate();
        }

        if (item is boilersGraphics.ViewModels.Text.NumberSequenceBlockViewModel numseq)
        {
            if (designerItemElm.Element("Start") is { } start)
                numseq.Start.Value = double.Parse(start.Value, System.Globalization.CultureInfo.InvariantCulture);
            if (designerItemElm.Element("End") is { } end)
                numseq.End.Value = double.Parse(end.Value, System.Globalization.CultureInfo.InvariantCulture);
            if (designerItemElm.Element("Step") is { } step)
                numseq.Step.Value = double.Parse(step.Value, System.Globalization.CultureInfo.InvariantCulture);
            if (designerItemElm.Element("NumFormat") is { } fmt) numseq.Format.Value = fmt.Value;
            if (designerItemElm.Element("NumSeqSeparator") is { } sep) numseq.Separator.Value = sep.Value;
            if (designerItemElm.Element("Direction") is { } dir &&
                Enum.TryParse<boilersGraphics.Models.Text.NumberSequenceDirection>(dir.Value, out var nsDir))
                numseq.Direction.Value = nsDir;
            if (designerItemElm.Element("GridRows") is { } gr) numseq.GridRows.Value = int.Parse(gr.Value);
            if (designerItemElm.Element("GridColumns") is { } gc) numseq.GridColumns.Value = int.Parse(gc.Value);
            numseq.Regenerate();
        }

        // Phase 2.5-a: TextMatrixBlock デシリアライズ。最後に Regenerate() で Text を再構築。
        if (item is boilersGraphics.ViewModels.Text.TextMatrixBlockViewModel matrix)
        {
            if (designerItemElm.Element("TextMatrixRows") is { } mr) matrix.Rows.Value = int.Parse(mr.Value);
            if (designerItemElm.Element("TextMatrixColumns") is { } mc) matrix.Columns.Value = int.Parse(mc.Value);
            if (designerItemElm.Element("TextMatrixCellMode") is { } mcm &&
                Enum.TryParse<boilersGraphics.Models.Text.TextMatrixCellMode>(mcm.Value, out var cm))
                matrix.CellMode.Value = cm;
            if (designerItemElm.Element("TextMatrixSeparator") is { } msep) matrix.Separator.Value = msep.Value;
            if (designerItemElm.Element("TextMatrixSequenceStart") is { } mss) matrix.SequenceStart.Value = int.Parse(mss.Value);
            if (designerItemElm.Element("TextMatrixSequenceFormat") is { } msf) matrix.SequenceFormat.Value = msf.Value;
            if (designerItemElm.Element("TextMatrixDataGenType") is { } mdt &&
                Enum.TryParse<boilersGraphics.Models.Text.DataGeneratorType>(mdt.Value, out var dgt))
                matrix.DataGenType.Value = dgt;
            if (designerItemElm.Element("TextMatrixDataGenSeed") is { } mds) matrix.DataGenSeed.Value = int.Parse(mds.Value);
            if (designerItemElm.Element("TextMatrixCustomItems") is { } mci) matrix.CustomItems.Value = mci.Value;
            matrix.Regenerate();
        }

        // Phase 2.5-b: TextOnPathBlock デシリアライズ。Regenerate() は PolyBezier が既に
        // AllItems にロード済みでないと参照解決できないので、確定的な再生成は呼び出し側に任せる。
        if (item is boilersGraphics.ViewModels.Text.TextOnPathBlockViewModel onPath)
        {
            if (designerItemElm.Element("TextOnPathReferenceId") is { } topr &&
                Guid.TryParse(topr.Value, out var refId))
                onPath.PathReferenceId.Value = refId;
            if (designerItemElm.Element("TextOnPathStartOffset") is { } tso)
                onPath.StartOffset.Value = double.Parse(tso.Value, System.Globalization.CultureInfo.InvariantCulture);
            if (designerItemElm.Element("TextOnPathSpacing") is { } tsp)
                onPath.Spacing.Value = double.Parse(tsp.Value, System.Globalization.CultureInfo.InvariantCulture);
            if (designerItemElm.Element("TextOnPathSide") is { } tsi &&
                Enum.TryParse<boilersGraphics.Models.Text.TextOnPathSide>(tsi.Value, out var topSide))
                onPath.Side.Value = topSide;
            if (designerItemElm.Element("TextOnPathRotation") is { } tro &&
                Enum.TryParse<boilersGraphics.Models.Text.TextOnPathRotation>(tro.Value, out var topRot))
                onPath.Rotation.Value = topRot;
            onPath.Regenerate();
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

        if (item is PartInstanceViewModel partInstance)
        {
            var defIdText = designerItemElm.Element("DefinitionId")?.Value;
            if (!string.IsNullOrEmpty(defIdText) && Guid.TryParse(defIdText, out var defId))
                partInstance.DefinitionId.Value = defId;

            var pvRoot = designerItemElm.Element("ParameterValues");
            if (pvRoot is not null)
            {
                foreach (var pvElm in pvRoot.Elements("ParameterValue"))
                {
                    var epIdText = pvElm.Attribute("ExposedPropertyId")?.Value;
                    if (string.IsNullOrEmpty(epIdText) || !Guid.TryParse(epIdText, out var epId))
                        continue;
                    var typeAttr = pvElm.Attribute("Type")?.Value;
                    object value = null;
                    if (!string.IsNullOrEmpty(typeAttr) &&
                        Enum.TryParse<ExposedPropertyType>(typeAttr, out var epType))
                        value = PartDeserializer.ParseTypedValue(pvElm, epType);
                    partInstance.GetOrCreateParameterValue(epId, value);
                }
            }

            // Phase 2-f-3: Definition が既に読み込み済みなら即時 Initialize。
            // PartDefinitions セクションが Layers より後の順で読まれるプロジェクトファイルでは、
            // ここでは見つからないが、その場合は DiagramViewModel.PartDefinitions の CollectionChanged 経路で後から Initialize される。
            if (diagramViewModel is not null
                && diagramViewModel.TryGetPartDefinition(partInstance.DefinitionId.Value, out var partDef))
            {
                partInstance.InitializeRenderedItems(partDef);
            }
        }

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