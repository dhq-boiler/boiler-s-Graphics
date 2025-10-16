using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ZLinq;

namespace boilersGraphics.ViewModels;

public class LetterDesignerItemViewModel : AbstractLetterDesignerItemViewModel, ILetterDesignerItemViewModel
{
    public LetterDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
        : base(id, parent, left, top)
    {
        Init();
    }

    public LetterDesignerItemViewModel()
    {
        Init();
    }

    public override bool SupportsPropertyDialog => true;

    public override void WithLineBreak(GlyphTypeface glyphTypeface)
    {
        //refresh path geometry
        PathGeometryNoRotate.Value = new PathGeometry();
        var listLineBreak = new List<PathGeometry>();
        var width = 0d;
        var offsetY = 0d;
        var next = 0;
        var allcount = 0;
        while (allcount < LetterString.Value.AsValueEnumerable().Count() && Width.Value > 0)
        {
            var maxHeight = 0d;
            var letterString = LetterString.Value.AsValueEnumerable().Skip(allcount);

            listLineBreak.Clear();
            width = 0d;
            var widthClone = width;

            for (var i = 0; i < letterString.Count() && width < Width.Value; ++i)
            {
                var @char = letterString.ElementAt(i);
                ushort glyphIndex;
                glyphTypeface.CharacterToGlyphMap.TryGetValue(@char, out glyphIndex);
                var geometry = glyphTypeface.GetGlyphOutline(glyphIndex, FontSize.Value, FontSize.Value);
                var pg = geometry.GetOutlinedPathGeometry();
                if (double.IsInfinity(pg.Bounds.Width))
                {
                    var spaceWidth = glyphTypeface.GetAvgWidth(FontSize.Value);
                    if (width + spaceWidth > Width.Value)
                        break;
                    width += spaceWidth;
                }
                else
                {
                    if (pg.Bounds.Width > Width.Value)
                        return;
                    if (width + pg.Bounds.Width > Width.Value)
                        break;
                    width += pg.Bounds.Width;
                }

                maxHeight = Math.Max(maxHeight, pg.Bounds.Height);
                listLineBreak.Add(pg);
                next = i + 1;
            }

            var list = new List<PathGeometry>();

            foreach (var pg in listLineBreak)
            {
                if (widthClone + pg.Bounds.Width > Width.Value)
                    break;
                if (double.IsInfinity(pg.Bounds.Width))
                {
                    var spaceWidth = glyphTypeface.GetAvgWidth(FontSize.Value);
                    widthClone += spaceWidth;
                }
                else
                {
                    widthClone += pg.Bounds.Width;
                }

                pg.Transform = new MatrixTransform(1.0, 0, 0, 1.0,
                    list.SumWidthExceptInfinity(glyphTypeface, FontSize.Value), maxHeight + offsetY);
                PathGeometry.Value.AddGeometry(pg);
                list.Add(pg);
            }

            offsetY += maxHeight;
            allcount += next;
        }
    }

    public override void WithoutLineBreak(GlyphTypeface glyphTypeface)
    {
        var l = new List<PathGeometry>();
        var list = new List<PathGeometry>();
        //refresh path geometry
        PathGeometryNoRotate.Value = new PathGeometry();
        var maxHeight = 0d;
        foreach (var @char in LetterString.Value)
        {
            ushort glyphIndex;
            glyphTypeface.CharacterToGlyphMap.TryGetValue(@char, out glyphIndex);
            var geometry = glyphTypeface.GetGlyphOutline(glyphIndex, FontSize.Value, FontSize.Value);
            var pg = geometry.GetOutlinedPathGeometry();
            maxHeight = Math.Max(maxHeight, pg.Bounds.Height);
            l.Add(pg);
        }

        foreach (var pg in l)
        {
            pg.Transform = new MatrixTransform(1.0, 0, 0, 1.0,
                list.SumWidthExceptInfinity(glyphTypeface, FontSize.Value), maxHeight);
            PathGeometry.Value.AddGeometry(pg);
            list.Add(pg);
        }
    }

    #region IClonable

    public override object Clone()
    {
        var clone = new LetterDesignerItemViewModel();
        clone.Owner = Owner;
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.RotationAngle.Value = RotationAngle.Value;
        clone.LetterString.Value = LetterString.Value;
        clone.SelectedFontFamily.Value = SelectedFontFamily.Value;
        clone.IsBold.Value = IsBold.Value;
        clone.IsItalic.Value = IsItalic.Value;
        clone.FontSize.Value = FontSize.Value;
        clone.PathGeometry = PathGeometry;
        clone.PathGeometryNoRotate.Value = PathGeometryNoRotate.Value;
        clone.IsAutoLineBreak.Value = IsAutoLineBreak.Value;
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        clone.StrokeDashArray.Value = StrokeDashArray.Value;
        clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
        return clone;
    }

    #endregion //IClonable

    public override void OpenPropertyDialog()
    {
        var dialogService =
            new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
        IDialogResult result = null;
        dialogService.Show(nameof(DetailLetter), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
    }

    public override void OpenSettingDialog()
    {
        var dialogService =
            new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
        IDialogResult result = null;
        dialogService.Show(nameof(LetterSetting), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        designerCanvas.Focus();
        LetterSettingDialogIsOpen.Value = true;
    }
}