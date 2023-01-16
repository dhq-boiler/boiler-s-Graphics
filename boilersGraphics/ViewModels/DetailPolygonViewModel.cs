using Prism.Regions;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    class DetailPolygonViewModel : DetailViewModelBase<NPolygonViewModel>
    {
        public DetailPolygonViewModel(IRegionManager regionManager) : base(regionManager)
        {
        }

        public override void SetProperties()
        {
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPolygonViewModel, PenLineJoin>(ViewModel.Value, "StrokeLineJoin", new PenLineJoin[] {
                PenLineJoin.Miter,
                PenLineJoin.Bevel,
                PenLineJoin.Round
            }));
            Properties.Add(new PropertyOptionsValueCombinationClass<NPolygonViewModel, DoubleCollection>(ViewModel.Value, "StrokeDashArray", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPolygonViewModel, double>(ViewModel.Value, "StrokeMiterLimit", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPolygonViewModel, double>(ViewModel.Value, "Left", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPolygonViewModel, double>(ViewModel.Value, "Top", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPolygonViewModel, double>(ViewModel.Value, "Width", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPolygonViewModel, double>(ViewModel.Value, "Height", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPolygonViewModel, double>(ViewModel.Value, "CenterX", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPolygonViewModel, double>(ViewModel.Value, "CenterY", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPolygonViewModel, double>(ViewModel.Value, "RotationAngle", HorizontalAlignment.Right));
        }
    }
}
