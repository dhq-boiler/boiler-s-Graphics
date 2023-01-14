using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    class DetailStraightLineViewModel : DetailViewModelBase<StraightConnectorViewModel>
    {
        public override void SetProperties()
        {
            Properties.Add(new PropertyOptionsValueCombinationStruct<StraightConnectorViewModel, PenLineCap>(ViewModel.Value, "StrokeStartLineCap", new PenLineCap[]
            {
                PenLineCap.Flat,
                PenLineCap.Round,
                PenLineCap.Square,
                PenLineCap.Triangle,
            }));
            Properties.Add(new PropertyOptionsValueCombinationStruct<StraightConnectorViewModel, PenLineCap>(ViewModel.Value, "StrokeEndLineCap", new PenLineCap[]
            {
                PenLineCap.Flat,
                PenLineCap.Round,
                PenLineCap.Square,
                PenLineCap.Triangle,
            }));
            Properties.Add(new PropertyOptionsValueCombinationClass<StraightConnectorViewModel, DoubleCollection>(ViewModel.Value, "StrokeDashArray", HorizontalAlignment.Left));
        }
    }
}
