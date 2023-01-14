﻿using System.Windows;
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
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "Left", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "Top", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "Width", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "Height", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "CenterX", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "CenterY", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<NRectangleViewModel, double>(ViewModel.Value, "RotationAngle", HorizontalAlignment.Right));
        }
    }
}
