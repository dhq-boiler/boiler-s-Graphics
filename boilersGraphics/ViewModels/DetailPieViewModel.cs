using Prism.Regions;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    class DetailPieViewModel : DetailViewModelBase<NPieViewModel>
    {
        public DetailPieViewModel(IRegionManager regionManager) : base(regionManager)
        {
        }

        public override void SetProperties()
        {
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, PenLineJoin>(ViewModel.Value, "StrokeLineJoin", new PenLineJoin[] {
                PenLineJoin.Miter,
                PenLineJoin.Bevel,
                PenLineJoin.Round
            }));
            Properties.Add(new PropertyOptionsValueCombinationClass<NPieViewModel, DoubleCollection>(ViewModel.Value, "StrokeDashArray", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, double>(ViewModel.Value, "StrokeMiterLimit", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, double>(ViewModel.Value, "Left", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, double>(ViewModel.Value, "Top", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, double>(ViewModel.Value, "Width", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, double>(ViewModel.Value, "Height", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, double>(ViewModel.Value, "CenterX", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, double>(ViewModel.Value, "CenterY", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, double>(ViewModel.Value, "RotationAngle", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationClass<NPieViewModel, PathGeometry>(ViewModel.Value, "PathGeometryNoRotate", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationReadOnlyClass<NPieViewModel, PathGeometry>(ViewModel.Value, "PathGeometry", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, int>(ViewModel.Value, "ZIndex", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NPieViewModel, double>(ViewModel.Value, "EdgeThickness", HorizontalAlignment.Stretch,
                new double[]
                {
                    0.0,
                    1.0,
                    2.0,
                    3.0,
                    4.0,
                    5.0,
                    10.0,
                    15.0,
                    20.0,
                    25.0,
                    30.0,
                    35.0,
                    40.0,
                    45.0,
                    50.0,
                    100.0
                }));
        }
    }
}
