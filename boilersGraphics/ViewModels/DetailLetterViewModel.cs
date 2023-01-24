using boilersGraphics.Models;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            Properties.Add(new PropertyOptionsValueCombinationClass<AbstractLetterDesignerItemViewModel, PathGeometry>(ViewModel.Value, "PathGeometryNoRotate", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationReadOnlyClass<AbstractLetterDesignerItemViewModel, PathGeometry>(ViewModel.Value, "PathGeometry", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationClass<AbstractLetterDesignerItemViewModel, string>(ViewModel.Value, "LetterString", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationClass<AbstractLetterDesignerItemViewModel, FontFamilyEx>(ViewModel.Value, "SelectedFontFamily", HorizontalAlignment.Left, Fonts.GetFontFamilies("C:\\Windows\\Fonts").Select(x => new FontFamilyEx(x)).ToArray()));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, bool>(ViewModel.Value, "IsBold", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, bool>(ViewModel.Value, "IsItalic", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, int>(ViewModel.Value, "FontSize", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, bool>(ViewModel.Value, "IsAutoLineBreak", HorizontalAlignment.Left));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, int>(ViewModel.Value, "ZIndex", HorizontalAlignment.Right));
            Properties.Add(new PropertyOptionsValueCombinationStruct<AbstractLetterDesignerItemViewModel, double>(ViewModel.Value, "EdgeThickness", HorizontalAlignment.Stretch,
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
