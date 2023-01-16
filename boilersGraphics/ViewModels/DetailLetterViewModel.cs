using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    class DetailLetterViewModel : DetailViewModelBase<AbstractLetterDesignerItemViewModel>
    {
        public DetailLetterViewModel(IRegionManager regionManager) : base(regionManager)
        {
        }

        public override void SetProperties()
        {
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, PenLineJoin>(ViewModel.Value, "StrokeLineJoin", new PenLineJoin[] {
                PenLineJoin.Miter,
                PenLineJoin.Bevel,
                PenLineJoin.Round
            }));
            Properties.Add(new PropertyOptionsValueCombinationClass<AbstractLetterDesignerItemViewModel, DoubleCollection>(ViewModel.Value, "StrokeDashArray", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, double>(ViewModel.Value, "StrokeMiterLimit", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, double>(ViewModel.Value, "Left", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, double>(ViewModel.Value, "Top", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, double>(ViewModel.Value, "Width", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, double>(ViewModel.Value, "Height", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, double>(ViewModel.Value, "CenterX", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, double>(ViewModel.Value, "CenterY", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, double>(ViewModel.Value, "RotationAngle", HorizontalAlignment.Right));
        }
    }
}
