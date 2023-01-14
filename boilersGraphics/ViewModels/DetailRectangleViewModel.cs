using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public class DetailRectangleViewModel : DetailViewModelBase<NRectangleViewModel>
    {
        public override void SetProperties()
        {
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, PenLineJoin>(ViewModel.Value, "StrokeLineJoin", new PenLineJoin[] {
                PenLineJoin.Miter,
                PenLineJoin.Bevel,
                PenLineJoin.Round
            }));
            Properties.Add(new PropertyOptionsValueCombinationClass<NRectangleViewModel, DoubleCollection>(ViewModel.Value, "StrokeDashArray", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "RadiusX", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "RadiusY", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "StrokeMiterLimit", HorizontalAlignment.Right));
        }
    }
}
