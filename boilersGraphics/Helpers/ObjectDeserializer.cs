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
            var list = new List<SelectableDesignerItemViewModelBase>();
            var designerItemsElm = root.Descendants("DesignerItems").Elements("DesignerItem");
            foreach (var designerItemElm in designerItemsElm)
            {
                var item = (DesignerItemViewModelBase)DeserializeInstance(designerItemElm);
                item.Left.Value = double.Parse(designerItemElm.Element("Left").Value);
                item.Top.Value = double.Parse(designerItemElm.Element("Top").Value);
                item.Width.Value = double.Parse(designerItemElm.Element("Width").Value);
                item.Height.Value = double.Parse(designerItemElm.Element("Height").Value);
                item.ID = Guid.Parse(designerItemElm.Element("ID").Value);
                item.ParentID = Guid.Parse(designerItemElm.Element("ParentID").Value);
                item.ZIndex.Value = Int32.Parse(designerItemElm.Element("ZIndex").Value);
                item.Matrix.Value = new Matrix();
                item.EdgeColor = (Color)ColorConverter.ConvertFromString(designerItemElm.Element("EdgeColor").Value);
                item.FillColor = (Color)ColorConverter.ConvertFromString(designerItemElm.Element("FillColor").Value);
                item.EdgeThickness = double.Parse(designerItemElm.Element("EdgeThickness").Value);
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
                    letter.PathGeometry = PathGeometry.CreateFromGeometry(Geometry.Parse(designerItemElm.Element("PathGeometry").Value));
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
                    letter.PathGeometry = PathGeometry.CreateFromGeometry(Geometry.Parse(designerItemElm.Element("PathGeometry").Value));
                    letter.AutoLineBreak = bool.Parse(designerItemElm.Element("AutoLineBreak").Value);
                }
                if (item is NPolygonViewModel)
                {
                    var polygon = item as NPolygonViewModel;
                    polygon.Data.Value = designerItemElm.Element("Data").Value;
                }
                list.Add(item);
            }
            var connectorsElm = root.Descendants("Connections").Elements("Connection");
            foreach (var connectorElm in connectorsElm)
            {
                var item = (ConnectorBaseViewModel)DeserializeInstance(connectorElm);
                item.ID = Guid.Parse(connectorElm.Element("ID").Value);
                item.ParentID = Guid.Parse(connectorElm.Element("ParentID").Value);
                item.Points = new ObservableCollection<Point>();
                item.Points.Add(new Point());
                item.Points.Add(new Point());
                item.Points[0] = Point.Parse(connectorElm.Element("BeginPoint").Value);
                item.Points[1] = Point.Parse(connectorElm.Element("EndPoint").Value);
                item.ZIndex.Value = Int32.Parse(connectorElm.Element("ZIndex").Value);
                item.EdgeColor = (Color)ColorConverter.ConvertFromString(connectorElm.Element("EdgeColor").Value);
                item.EdgeThickness = double.Parse(connectorElm.Element("EdgeThickness").Value);
                item.Owner = diagramViewModel;
                if (item is BezierCurveViewModel)
                {
                    var bezier = item as BezierCurveViewModel;
                    bezier.ControlPoint1.Value = Point.Parse(connectorElm.Element("ControlPoint1").Value);
                    bezier.ControlPoint2.Value = Point.Parse(connectorElm.Element("ControlPoint2").Value);
                }
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

            diagramViewModel.Items.AddRange(list.OrderBy(x => x.ZIndex.Value));
        }
    }
}
