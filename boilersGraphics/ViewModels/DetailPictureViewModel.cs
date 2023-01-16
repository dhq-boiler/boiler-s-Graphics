﻿using Prism.Regions;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    class DetailPictureViewModel : DetailViewModelBase<PictureDesignerItemViewModel>
    {
        public DetailPictureViewModel(IRegionManager regionManager) : base(regionManager)
        {
        }

        public override void SetProperties()
        {
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, PenLineJoin>(ViewModel.Value, "StrokeLineJoin", new PenLineJoin[] {
                PenLineJoin.Miter,
                PenLineJoin.Bevel,
                PenLineJoin.Round
            }));
            Properties.Add(new PropertyOptionsValueCombinationClass<PictureDesignerItemViewModel, DoubleCollection>(ViewModel.Value, "StrokeDashArray", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, double>(ViewModel.Value, "StrokeMiterLimit", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, double>(ViewModel.Value, "Left", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, double>(ViewModel.Value, "Top", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, double>(ViewModel.Value, "Width", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, double>(ViewModel.Value, "Height", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, double>(ViewModel.Value, "CenterX", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, double>(ViewModel.Value, "CenterY", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, double>(ViewModel.Value, "RotationAngle", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<PictureDesignerItemViewModel, Thickness>(ViewModel.Value, "Margin", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationReadOnlyStruct<PictureDesignerItemViewModel, Rect>(ViewModel.Value, "ClipObject", "Rect", HorizontalAlignment.Right));
        }
    }
}
