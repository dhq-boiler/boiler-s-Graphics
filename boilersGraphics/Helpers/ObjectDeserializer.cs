using boilersGraphics.Exceptions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
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
    class ObjectDeserializer
    {
        private static SelectableDesignerItemViewModelBase DeserializeInstance(XElement designerItemXML)
        {
            var className = designerItemXML.Element("Type").Value;
            return (SelectableDesignerItemViewModelBase)Activator.CreateInstance(Assembly.GetExecutingAssembly().GetName().Name, className).Unwrap();
        }

        public static void ReadObjectFromXML(DiagramViewModel diagramViewModel, XElement root)
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
                            DesignerItemViewModelBase designerItemObj = null;
                            ConnectorBaseViewModel connectorObj = null;
                            if (layerItem.Descendants("Item").First().Descendants("DesignerItem").Count() >= 1)
                            {
                                designerItemObj = ExtractDesignerItemViewModelBase(diagramViewModel, layerItem.Descendants("Item").First().Descendants("DesignerItem").First());
                            }
                            if (layerItem.Descendants("Item").First().Descendants("ConnectorItem").Count() >= 1)
                            {
                                connectorObj = ExtractConnectorBaseViewModel(diagramViewModel, layerItem.Descendants("Item").First().Descendants("ConnectorItem").First());
                            }
                            var item = EitherNotNull(designerItemObj, connectorObj);
                            var layerItemObj = new LayerItem(item, layerObj, layerItem.Element("Name").Value);
                            layerItemObj.Color.Value = (Color)ColorConverter.ConvertFromString(layerItem.Element("Color").Value);
                            layerItemObj.IsVisible.Value = bool.Parse(layerItem.Element("IsVisible").Value);
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

        private static SelectableDesignerItemViewModelBase EitherNotNull(DesignerItemViewModelBase designerItemObj, ConnectorBaseViewModel connectorObj)
        {
            if (designerItemObj != null && connectorObj != null)
                throw new UnexpectedException("Both are not null.");
            else if (designerItemObj != null)
                return designerItemObj;
            else if (connectorObj != null)
                return connectorObj;
            else
                throw new UnexpectedException("Both are null.");
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

                children.ToList().ForEach(x => groupItem.AddGroup(x));
            }

            return list;
        }

        private static ConnectorBaseViewModel ExtractConnectorBaseViewModel(DiagramViewModel diagramViewModel, XElement connectorElm)
        {
            if (!(DeserializeInstance(connectorElm) is ConnectorBaseViewModel))
                return null;
            var item = (ConnectorBaseViewModel)DeserializeInstance(connectorElm);
            item.ID = Guid.Parse(connectorElm.Element("ID").Value);
            item.ParentID = Guid.Parse(connectorElm.Element("ParentID").Value);
            item.Points = new ObservableCollection<Point>();
            item.Points.Add(new Point());
            item.Points.Add(new Point());
            item.Points[0] = Point.Parse(connectorElm.Element("BeginPoint").Value);
            item.Points[1] = Point.Parse(connectorElm.Element("EndPoint").Value);
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
            item.Matrix.Value = new Matrix();
            item.EdgeColor.Value = (Color)ColorConverter.ConvertFromString(designerItemElm.Element("EdgeColor").Value);
            item.FillColor.Value = (Color)ColorConverter.ConvertFromString(designerItemElm.Element("FillColor").Value);
            item.EdgeThickness.Value = double.Parse(designerItemElm.Element("EdgeThickness").Value);
            item.PathGeometry.Value = PathGeometry.CreateFromGeometry(PathGeometry.Parse(designerItemElm.Element("PathGeometry").Value));
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
