using boilersGraphics.Extensions;
using boilersGraphics.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace boilersGraphics.Helpers
{
    class ObjectSerializer
    {
        public static IEnumerable<XElement> SerializeDesignerItems(DiagramViewModel dialogViewModel, IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            var targets = (from item in items.WithPickupChildren(dialogViewModel.Items).OfType<DesignerItemViewModelBase>()
                    where item.GetType() != typeof(PictureDesignerItemViewModel)
                       && item.GetType() != typeof(LetterDesignerItemViewModel)
                       && item.GetType() != typeof(LetterVerticalDesignerItemViewModel)
                       && item.GetType() != typeof(NPolygonViewModel)
                    select new XElement("DesignerItem",
                              new XElement("ID", item.ID),
                              new XElement("ParentID", item.ParentID),
                              new XElement("Type", item.GetType().FullName),
                              new XElement("Left", item.Left.Value),
                              new XElement("Top", item.Top.Value),
                              new XElement("Width", item.Width.Value),
                              new XElement("Height", item.Height.Value),
                              new XElement("ZIndex", item.ZIndex.Value),
                              new XElement("Matrix", item.Matrix.Value),
                              new XElement("EdgeColor", item.EdgeColor),
                              new XElement("FillColor", item.FillColor),
                              new XElement("EdgeThickness", item.EdgeThickness),
                              new XElement("PathGeometry", item.PathGeometry.Value)
                          ))
                    .Union(
                        from item in items.WithPickupChildren(dialogViewModel.Items).OfType<DesignerItemViewModelBase>()
                        where item.GetType() == typeof(PictureDesignerItemViewModel)
                        select new XElement("DesignerItem",
                                    new XElement("ID", item.ID),
                                    new XElement("ParentID", item.ParentID),
                                    new XElement("Type", item.GetType().FullName),
                                    new XElement("Left", item.Left.Value),
                                    new XElement("Top", item.Top.Value),
                                    new XElement("Width", item.Width.Value),
                                    new XElement("Height", item.Height.Value),
                                    new XElement("ZIndex", item.ZIndex.Value),
                                    new XElement("Matrix", item.Matrix.Value),
                                    new XElement("EdgeColor", item.EdgeColor),
                                    new XElement("FillColor", item.FillColor),
                                    new XElement("EdgeThickness", item.EdgeThickness),
                                    new XElement("FileName", (item as PictureDesignerItemViewModel).FileName)
                        )
                    )
                    .Union(
                        from item in items.WithPickupChildren(dialogViewModel.Items).OfType<DesignerItemViewModelBase>()
                        where item.GetType() == typeof(LetterDesignerItemViewModel)
                        select new XElement("DesignerItem",
                                    new XElement("ID", item.ID),
                                    new XElement("ParentID", item.ParentID),
                                    new XElement("Type", item.GetType().FullName),
                                    new XElement("Left", item.Left.Value),
                                    new XElement("Top", item.Top.Value),
                                    new XElement("Width", item.Width.Value),
                                    new XElement("Height", item.Height.Value),
                                    new XElement("ZIndex", item.ZIndex.Value),
                                    new XElement("Matrix", item.Matrix.Value),
                                    new XElement("EdgeColor", item.EdgeColor),
                                    new XElement("FillColor", item.FillColor),
                                    new XElement("EdgeThickness", item.EdgeThickness),
                                    new XElement("LetterString", (item as LetterDesignerItemViewModel).LetterString),
                                    new XElement("SelectedFontFamily", (item as LetterDesignerItemViewModel).SelectedFontFamily),
                                    new XElement("IsBold", (item as LetterDesignerItemViewModel).IsBold),
                                    new XElement("IsItalic", (item as LetterDesignerItemViewModel).IsItalic),
                                    new XElement("FontSize", (item as LetterDesignerItemViewModel).FontSize),
                                    new XElement("PathGeometry", (item as LetterDesignerItemViewModel).PathGeometry),
                                    new XElement("AutoLineBreak", (item as LetterDesignerItemViewModel).AutoLineBreak)
                        )
                    )
                    .Union(
                        from item in items.WithPickupChildren(dialogViewModel.Items).OfType<DesignerItemViewModelBase>()
                        where item.GetType() == typeof(LetterVerticalDesignerItemViewModel)
                        select new XElement("DesignerItem",
                                    new XElement("ID", item.ID),
                                    new XElement("ParentID", item.ParentID),
                                    new XElement("Type", item.GetType().FullName),
                                    new XElement("Left", item.Left.Value),
                                    new XElement("Top", item.Top.Value),
                                    new XElement("Width", item.Width.Value),
                                    new XElement("Height", item.Height.Value),
                                    new XElement("ZIndex", item.ZIndex.Value),
                                    new XElement("Matrix", item.Matrix.Value),
                                    new XElement("EdgeColor", item.EdgeColor),
                                    new XElement("FillColor", item.FillColor),
                                    new XElement("EdgeThickness", item.EdgeThickness),
                                    new XElement("LetterString", (item as LetterVerticalDesignerItemViewModel).LetterString),
                                    new XElement("SelectedFontFamily", (item as LetterVerticalDesignerItemViewModel).SelectedFontFamily),
                                    new XElement("IsBold", (item as LetterVerticalDesignerItemViewModel).IsBold),
                                    new XElement("IsItalic", (item as LetterVerticalDesignerItemViewModel).IsItalic),
                                    new XElement("FontSize", (item as LetterVerticalDesignerItemViewModel).FontSize),
                                    new XElement("PathGeometry", (item as LetterVerticalDesignerItemViewModel).PathGeometry),
                                    new XElement("AutoLineBreak", (item as LetterVerticalDesignerItemViewModel).AutoLineBreak)
                        )
                    )
                    .Union(
                        from item in items.WithPickupChildren(dialogViewModel.Items).OfType<DesignerItemViewModelBase>()
                        where item.GetType() == typeof(NPolygonViewModel)
                        select new XElement("DesignerItem",
                                new XElement("ID", item.ID),
                                new XElement("ParentID", item.ParentID),
                                new XElement("Type", item.GetType().FullName),
                                new XElement("Left", item.Left.Value),
                                new XElement("Top", item.Top.Value),
                                new XElement("Width", item.Width.Value),
                                new XElement("Height", item.Height.Value),
                                new XElement("ZIndex", item.ZIndex.Value),
                                new XElement("Matrix", item.Matrix.Value),
                                new XElement("EdgeColor", item.EdgeColor),
                                new XElement("FillColor", item.FillColor),
                                new XElement("EdgeThickness", item.EdgeThickness),
                                new XElement("PathGeometry", item.PathGeometry.Value),
                                new XElement("Data", (item as NPolygonViewModel).Data.Value)
                            )
                        );
            return targets;
        }

        public static IEnumerable<XElement> SerializeConnections(DiagramViewModel dialogViewModel, IEnumerable<SelectableDesignerItemViewModelBase> items)
        {
            return (from connection in items.WithPickupChildren(dialogViewModel.Items).OfType<ConnectorBaseViewModel>()
                    where connection.GetType() != typeof(BezierCurveViewModel)
                    select new XElement("Connection",
                               new XElement("ID", connection.ID),
                               new XElement("ParentID", connection.ParentID),
                               new XElement("Type", connection.GetType().FullName),
                               new XElement("BeginPoint", connection.Points[0]),
                               new XElement("EndPoint", connection.Points[1]),
                               new XElement("ZIndex", connection.ZIndex.Value),
                               new XElement("EdgeColor", connection.EdgeColor),
                               new XElement("EdgeThickness", connection.EdgeThickness),
                               new XElement("PathGeometry", connection.PathGeometry.Value)
                    ))
                    .Union(
                        from connection in items.WithPickupChildren(dialogViewModel.Items).OfType<ConnectorBaseViewModel>()
                        where connection.GetType() == typeof(BezierCurveViewModel)
                        select new XElement("Connection",
                                    new XElement("ID", connection.ID),
                                    new XElement("ParentID", connection.ParentID),
                                    new XElement("Type", connection.GetType().FullName),
                                    new XElement("BeginPoint", connection.Points[0]),
                                    new XElement("EndPoint", connection.Points[1]),
                                    new XElement("ZIndex", connection.ZIndex.Value),
                                    new XElement("EdgeColor", connection.EdgeColor),
                                    new XElement("EdgeThickness", connection.EdgeThickness),
                                    new XElement("ControlPoint1", (connection as BezierCurveViewModel).ControlPoint1.Value),
                                    new XElement("ControlPoint2", (connection as BezierCurveViewModel).ControlPoint2.Value),
                                    new XElement("PathGeometry", connection.PathGeometry.Value)
                    ));
        }

        public static IEnumerable<XElement> SerializeConfiguration(DiagramViewModel diagramViewModel)
        {
            var mainWindowVM = (App.Current.MainWindow.DataContext as MainWindowViewModel);
            return new XElement[]
            {
                new XElement("Width", diagramViewModel.Width),
                new XElement("Height", diagramViewModel.Height),
                new XElement("CanvasBackground", diagramViewModel.CanvasBackground.Value),
                new XElement("EnablePointSnap", diagramViewModel.EnablePointSnap.Value),
                new XElement("SnapPower", mainWindowVM.SnapPower.Value)
            };
        }
    }
}
