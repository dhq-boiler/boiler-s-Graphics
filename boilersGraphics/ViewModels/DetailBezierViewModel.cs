using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    class DetailBezierViewModel : DetailViewModelBase<BezierCurveViewModel>
    {
        public override void SetProperties()
        {
            Properties.Add(new PropertyOptionsValueCombinationStruct<BezierCurveViewModel, PenLineCap>(ViewModel.Value, "StrokeStartLineCap", new PenLineCap[]
            {
                PenLineCap.Flat,
                PenLineCap.Round,
                PenLineCap.Square,
                PenLineCap.Triangle,
            }));
            Properties.Add(new PropertyOptionsValueCombinationStruct<BezierCurveViewModel, PenLineCap>(ViewModel.Value, "StrokeEndLineCap", new PenLineCap[]
            {
                PenLineCap.Flat,
                PenLineCap.Round,
                PenLineCap.Square,
                PenLineCap.Triangle,
            }));
            Properties.Add(new PropertyOptionsValueCombinationClass<BezierCurveViewModel, DoubleCollection>(ViewModel.Value, "StrokeDashArray", HorizontalAlignment.Left));
        }
    }
}
