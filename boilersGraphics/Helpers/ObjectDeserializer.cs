﻿using boilersGraphics.Exceptions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace boilersGraphics.Helpers
{
    public class ObjectDeserializer
    {
        private static SelectableDesignerItemViewModelBase DeserializeInstance(XElement designerItemXML)
        {
            var className = designerItemXML.Element("Type").Value;
            return (SelectableDesignerItemViewModelBase)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetName().Name, className).Unwrap();
        }

        public static void ReadCopyObjectsFromXML(DiagramViewModel diagramViewModel, XElement root)
        {
            var copyObjs = root.Elements().Where(x => x.Name == "CopyObjects").FirstOrDefault();
            if (copyObjs == null)
                throw new UnexpectedException("must be copyObjs != null");
            var layers = copyObjs.Elements().Where(x => x.Name == "Layers").FirstOrDefault();
            var layerItems = copyObjs.Elements().Where(x => x.Name == "LayerItems").FirstOrDefault();
            if (layers == null && layerItems == null)
                throw new UnexpectedException("must be layers != null or items != null");
            if (layers != null)
            {
                foreach (var layer in layers.Descendants("Layer"))
                {
                    var layerObj = new Layer();
                    layerObj.Color.Value = (Color)ColorConverter.ConvertFromString(layer.Element("Color").Value);
                    layerObj.IsVisible.Value = bool.Parse(layer.Element("IsVisible").Value);
                    layerObj.Name.Value = layer.Element("Name").Value;

                    foreach (var layerItemsInternal in layer.Descendants("LayerItems"))
                    {
                        foreach (var layerItem in layerItemsInternal.Descendants("LayerItem"))
                        {
                            LayerItem layerItemObj = ReadLayerItemFromXML(diagramViewModel, layerObj, layerItem);
                            layerObj.Children.Add(layerItemObj);
                        }
                    }

                    diagramViewModel.Layers.Add(layerObj);
                }
            }
            else if (layerItems != null)
            {
                var layerObj = diagramViewModel.SelectedLayers.Value.First();
                foreach (var layerItem in layerItems.Descendants("LayerItem"))
                {
                    if (layerItem.Descendants("Item").First().Descendants("DesignerItem").Count() == 0)
                        break;
                    var designerItemObj = ExtractDesignerItemViewModelBase(diagramViewModel, layerItem.Descendants("Item").First().Descendants("DesignerItem").First());
                    if (designerItemObj == null)
                        continue;
                    var layerItemObj = new LayerItem(designerItemObj, layerObj, layerItem.Element("Name").Value);
                    layerItemObj.IsVisible.Value = bool.Parse(layerItem.Element("IsVisible").Value);
                    layerObj.Children.Add(layerItemObj);
                }
                foreach (var layerItem in layerItems.Descendants("LayerItem"))
                {
                    if (layerItem.Descendants("Item").First().Descendants("ConnectorItem").Count() == 0)
                        break;
                    var connectorObj = ExtractConnectorBaseViewModel(diagramViewModel, layerItem.Descendants("Item").First().Descendants("ConnectorItem").First());
                    if (connectorObj == null)
                        continue;
                    var layerItemObj = new LayerItem(connectorObj, layerObj, layerItem.Element("Name").Value);
                    layerItemObj.IsVisible.Value = bool.Parse(layerItem.Element("IsVisible").Value);
                    layerObj.Children.Add(layerItemObj);
                }
            }
        }

        public static void ReadObjectsFromXML(DiagramViewModel diagramViewModel, XElement root)
        {
            var layers = root.Elements().Where(x => x.Name == "Layers").FirstOrDefault();
            if (layers != null)
            {
                foreach (var layer in layers.Elements("Layer"))
                {
                    var layerObj = new Layer();
                    layerObj.Color.Value = (Color)ColorConverter.ConvertFromString(layer.Element("Color").Value);
                    layerObj.IsVisible.Value = bool.Parse(layer.Element("IsVisible").Value);
                    layerObj.Name.Value = layer.Element("Name").Value;

                    foreach (var layerItemsInternal in layer.Elements("Children"))
                    {
                        foreach (var layerItem in layerItemsInternal.Elements("LayerItem"))
                        {
                            LayerItem layerItemObj = ReadLayerItemFromXML(diagramViewModel, layerObj, layerItem);
                            layerObj.Children.Add(layerItemObj);
                        }
                    }

                    diagramViewModel.Layers.Add(layerObj);
                }
            }
            else
            {
                var layerObj = new Layer();
                Random rand = new Random();
                layerObj.Color.Value = Randomizer.RandomColor(rand);
                layerObj.IsVisible.Value = true;
                layerObj.Name.Value = Name.GetNewLayerName(diagramViewModel);

                //読み込むファイルにLayers要素がない場合、初期レイヤーに全てのアイテムを突っ込む
                foreach (var designerItems in root.Elements("DesignerItems"))
                {
                    foreach (var designerItem in designerItems.Elements("DesignerItem"))
                    {
                        var item = ExtractDesignerItemViewModelBase(diagramViewModel, designerItem);
                        var layerItem = new LayerItem(item, layerObj, Name.GetNewLayerItemName(diagramViewModel));
                        layerItem.Color.Value = Randomizer.RandomColor(rand);
                        layerObj.Children.Add(layerItem);
                    }
                }
                foreach (var connections in root.Elements("Connections"))
                {
                    foreach (var connector in connections.Elements("Connection"))
                    {
                        var item = ExtractConnectorBaseViewModel(diagramViewModel, connector);
                        var layerItem = new LayerItem(item, layerObj, Name.GetNewLayerItemName(diagramViewModel));
                        layerItem.Color.Value = Randomizer.RandomColor(rand);
                        layerObj.Children.Add(layerItem);
                    }
                }

                diagramViewModel.Layers.Add(layerObj);
            }
        }

        private static LayerItem ReadLayerItemFromXML(DiagramViewModel diagramViewModel, Layer layerObj, XElement layerItem)
        {
            if (layerItem == null)
                return null;
            DesignerItemViewModelBase designerItemObj = null;
            ConnectorBaseViewModel connectorObj = null;
            SnapPointViewModel snapPointObj = null;
            if (layerItem.Descendants("Item").First().Descendants("DesignerItem").Count() >= 1)
            {
                designerItemObj = ExtractDesignerItemViewModelBase(diagramViewModel, layerItem.Descendants("Item").First().Descendants("DesignerItem").First());
            }
            if (layerItem.Descendants("Item").First().Descendants("ConnectorItem").Count() >= 1)
            {
                connectorObj = ExtractConnectorBaseViewModel(diagramViewModel, layerItem.Descendants("Item").First().Descendants("ConnectorItem").First());
            }
            if (layerItem.Descendants("Item").First().Descendants("SnapPointItem").Count() >= 1)
            {
                snapPointObj = ExtractSnapPointViewModel(diagramViewModel, layerItem.Descendants("Item").First().Descendants("SnapPointItem").First());
            }
            var item = EitherNotNull(designerItemObj, EitherNotNull(connectorObj, snapPointObj));
            if (item == null)
                throw new UnexpectedException("All of them are null.");
            var layerItemObj = new LayerItem(item, layerObj, layerItem.Element("Name").Value);
            layerItemObj.Color.Value = (Color)ColorConverter.ConvertFromString(layerItem.Element("Color").Value);
            layerItemObj.IsVisible.Value = bool.Parse(layerItem.Element("IsVisible").Value);
            var children = layerItem.Elements("Children").Descendants("LayerItem");
            var children_layerItems = (from child in children
                                       let li = ReadLayerItemFromXML(diagramViewModel, layerObj, child)
                                       select li);
            foreach (var c in children_layerItems)
            {
                layerItemObj.Children.Add(c);

                //グループの場合、子をグループに追加する
                if (item is GroupItemViewModel groupItemVM)
                {
                    groupItemVM.AddGroup(diagramViewModel.MainWindowVM.Recorder, c.Item.Value);
                }
            }
            return layerItemObj;
        }

        private static SelectableDesignerItemViewModelBase EitherNotNull(SelectableDesignerItemViewModelBase left, SelectableDesignerItemViewModelBase right)
        {
            if (left != null && right != null)
                return null;
            else if (left != null)
                return left;
            else if (right != null)
                return right;
            else
                return null;
        }

        private static List<SelectableDesignerItemViewModelBase> ExtractItems(DiagramViewModel diagramViewModel, IEnumerable<XElement> designerItemsElm, IEnumerable<XElement> connectorsElm)
        {
            var list = new List<SelectableDesignerItemViewModelBase>();
            foreach (var designerItemElm in designerItemsElm)
            {
                DesignerItemViewModelBase item = ExtractDesignerItemViewModelBase(diagramViewModel, designerItemElm);
                list.Add(item);
            }
            foreach (var connectorElm in connectorsElm)
            {
                ConnectorBaseViewModel item = ExtractConnectorBaseViewModel(diagramViewModel, connectorElm);
                list.Add(item);
            }

            //grouping
            foreach (var groupItem in list.OfType<GroupItemViewModel>().ToList())
            {
                var children = from item in list
                               where item.ParentID == groupItem.ID
                               select item;

                children.ToList().ForEach(x => groupItem.AddGroup(diagramViewModel.MainWindowVM.Recorder, x));
            }

            return list;
        }

        private static SnapPointViewModel ExtractSnapPointViewModel(DiagramViewModel diagramViewModel, XElement snapPointElm)
        {
            if (!(DeserializeInstance(snapPointElm) is SnapPointViewModel item))
                return null;
            item.ID = Guid.Parse(snapPointElm.Element("ID").Value);
            item.ParentID = Guid.Parse(snapPointElm.Element("ParentID").Value);
            item.Left.Value = double.Parse(snapPointElm.Element("Left").Value);
            item.Top.Value = double.Parse(snapPointElm.Element("Top").Value);
            item.Width.Value = double.Parse(snapPointElm.Element("Width").Value);
            item.Height.Value = double.Parse(snapPointElm.Element("Height").Value);
            item.ZIndex.Value = Int32.Parse(snapPointElm.Element("ZIndex").Value);
            item.Matrix.Value = new Matrix();
            item.EdgeColor.Value = (Color)ColorConverter.ConvertFromString(snapPointElm.Element("EdgeColor").Value);
            item.FillColor.Value = (Color)ColorConverter.ConvertFromString(snapPointElm.Element("FillColor").Value);
            item.EdgeThickness.Value = double.Parse(snapPointElm.Element("EdgeThickness").Value);
            item.PathGeometry.Value = PathGeometry.CreateFromGeometry(PathGeometry.Parse(snapPointElm.Element("PathGeometry").Value));
            item.Opacity.Value = 0.5;
            item.Owner = diagramViewModel;
            return item;
        }

        private static ConnectorBaseViewModel ExtractConnectorBaseViewModel(DiagramViewModel diagramViewModel, XElement connectorElm)
        {
            if (!(DeserializeInstance(connectorElm) is ConnectorBaseViewModel))
                return null;
            var item = (ConnectorBaseViewModel)DeserializeInstance(connectorElm);
            item.ID = Guid.Parse(connectorElm.Element("ID").Value);
            item.ParentID = Guid.Parse(connectorElm.Element("ParentID").Value);
            item.Points = new ObservableCollection<Point>();
            item.AddPoints(diagramViewModel, Point.Parse(connectorElm.Element("BeginPoint").Value), Point.Parse(connectorElm.Element("EndPoint").Value));
            item.InitIsSelectedOnSnapPoints();
            item.ZIndex.Value = Int32.Parse(connectorElm.Element("ZIndex").Value);
            item.EdgeColor.Value = (Color)ColorConverter.ConvertFromString(connectorElm.Element("EdgeColor").Value);
            item.EdgeThickness.Value = double.Parse(connectorElm.Element("EdgeThickness").Value);
            item.PathGeometry.Value = PathGeometry.CreateFromGeometry(PathGeometry.Parse(connectorElm.Element("PathGeometry").Value));
            item.Owner = diagramViewModel;
            if (item is BezierCurveViewModel)
            {
                var bezier = item as BezierCurveViewModel;
                bezier.ControlPoint1.Value = Point.Parse(connectorElm.Element("ControlPoint1").Value);
                bezier.ControlPoint2.Value = Point.Parse(connectorElm.Element("ControlPoint2").Value);
            }

            return item;
        }

        private static DesignerItemViewModelBase ExtractDesignerItemViewModelBase(DiagramViewModel diagramViewModel, XElement designerItemElm)
        {
            if (!(DeserializeInstance(designerItemElm) is DesignerItemViewModelBase))
                return null;
            var item = (DesignerItemViewModelBase)DeserializeInstance(designerItemElm);
            item.Left.Value = double.Parse(designerItemElm.Element("Left").Value);
            item.Top.Value = double.Parse(designerItemElm.Element("Top").Value);
            item.Width.Value = double.Parse(designerItemElm.Element("Width").Value);
            item.Height.Value = double.Parse(designerItemElm.Element("Height").Value);
            item.ID = Guid.Parse(designerItemElm.Element("ID").Value);
            item.ParentID = Guid.Parse(designerItemElm.Element("ParentID").Value);
            item.ZIndex.Value = Int32.Parse(designerItemElm.Element("ZIndex").Value);
            //item.Matrix.Value = Matrix.Parse(designerItemElm.Element("Matrix").Value);
            item.EdgeColor.Value = (Color)ColorConverter.ConvertFromString(designerItemElm.Element("EdgeColor").Value);
            item.FillColor.Value = (Color)ColorConverter.ConvertFromString(designerItemElm.Element("FillColor").Value);
            item.EdgeThickness.Value = double.Parse(designerItemElm.Element("EdgeThickness").Value);
            item.PathGeometry.Value = PathGeometry.CreateFromGeometry(PathGeometry.Parse(designerItemElm.Element("PathGeometry").Value));
            item.RotationAngle.Value = double.Parse(designerItemElm.Element("RotationAngle").Value);
            item.Owner = diagramViewModel;
            if (item is PictureDesignerItemViewModel)
            {
                var picture = item as PictureDesignerItemViewModel;
                picture.FileName = designerItemElm.Element("FileName").Value;
            }
            if (item is LetterDesignerItemViewModel)
            {
                var letter = item as LetterDesignerItemViewModel;
                letter.LetterString = designerItemElm.Element("LetterString").Value;
                letter.SelectedFontFamily = new FontFamilyEx(designerItemElm.Element("SelectedFontFamily").Value);
                letter.IsBold = bool.Parse(designerItemElm.Element("IsBold").Value);
                letter.IsItalic = bool.Parse(designerItemElm.Element("IsItalic").Value);
                letter.FontSize = int.Parse(designerItemElm.Element("FontSize").Value);
                letter.AutoLineBreak = bool.Parse(designerItemElm.Element("AutoLineBreak").Value);
            }
            if (item is LetterVerticalDesignerItemViewModel)
            {
                var letter = item as LetterVerticalDesignerItemViewModel;
                letter.LetterString = designerItemElm.Element("LetterString").Value;
                letter.SelectedFontFamily = new FontFamilyEx(designerItemElm.Element("SelectedFontFamily").Value);
                letter.IsBold = bool.Parse(designerItemElm.Element("IsBold").Value);
                letter.IsItalic = bool.Parse(designerItemElm.Element("IsItalic").Value);
                letter.FontSize = int.Parse(designerItemElm.Element("FontSize").Value);
                letter.AutoLineBreak = bool.Parse(designerItemElm.Element("AutoLineBreak").Value);
            }
            if (item is NPolygonViewModel)
            {
                var polygon = item as NPolygonViewModel;
                polygon.Data.Value = designerItemElm.Element("Data").Value;
            }

            return item;
        }
    }
}
