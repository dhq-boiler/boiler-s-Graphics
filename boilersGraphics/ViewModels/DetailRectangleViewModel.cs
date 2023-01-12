using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public class DetailRectangleViewModel : DetailViewModelBase<NRectangleViewModel>
    {
        public override void SetProperties()
        {
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, System.Windows.Media.PenLineJoin>(ViewModel.Value, "PenLineJoin", new System.Windows.Media.PenLineJoin[] {
                System.Windows.Media.PenLineJoin.Miter,
                System.Windows.Media.PenLineJoin.Bevel,
                System.Windows.Media.PenLineJoin.Round
            }));
            Properties.Add(new PropertyOptionsValueCombinationClass<NRectangleViewModel, DoubleCollection>(ViewModel.Value, "StrokeDashArray"));
        }
    }
}
