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

public class LetterVerticalDesignerItemViewModel : AbstractLetterDesignerItemViewModel, ILetterDesignerItemViewModel
{
    public LetterVerticalDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
        : base(id, parent, left, top)
    {
        Init();
    }

    public LetterVerticalDesignerItemViewModel()
    {
        Init();
    }

    public override bool SupportsPropertyDialog => true;

    public override void WithLineBreak(GlyphTypeface glyphTypeface)
    {
        //refresh path geometry
        PathGeometryNoRotate.Value = new PathGeometry();
        var listLineBreak = new List<PathGeometry>();
        var height = 0d;
        var offsetX = 0d;
        var next = 0;
        var allcount = 0;
        while (allcount < LetterString.Value.AsValueEnumerable().Count() && Height.Value > 0)
        {
            var maxWidth = 0d;
            var letterString = LetterString.Value.AsValueEnumerable().Skip(allcount);

            listLineBreak.Clear();
            height = 0d;
            var heightClone = height;

            for (var i = 0; i < letterString.Count() && height < Height.Value; ++i)
            {
                var @char = letterString.ElementAt(i);
                ushort glyphIndex;
                glyphTypeface.CharacterToGlyphMap.TryGetValue(@char, out glyphIndex);
                var geometry = glyphTypeface.GetGlyphOutline(glyphIndex, FontSize.Value, FontSize.Value);
                var pg = geometry.GetOutlinedPathGeometry();

                if (@char == '-' || @char == 'ー'
                                 || @char == '='
                                 || @char == '＝'
                                 || @char == '～'
                                 || @char == '~'
                                 || @char == ':'
                                 || @char == ';'
                                 || @char == '('
                                 || @char == ')'
                                 || @char == '['
                                 || @char == ']'
                                 || @char == '{'
                                 || @char == '}')
                    pg.Transform = new RotateTransform(90);

                if (double.IsInfinity(pg.Bounds.Height))
                {
                    var spaceHeight = glyphTypeface.GetAvgHeight(FontSize.Value);
                    if (height + spaceHeight > Height.Value)
                        break;
                    height += spaceHeight;
                }
                else
                {
                    if (pg.Bounds.Height > Height.Value)
                        return;
                    if (height + pg.Bounds.Height > Height.Value)
                        break;
                    height += pg.Bounds.Height;
                }

                maxWidth = Math.Max(maxWidth, pg.Bounds.Width);
                listLineBreak.Add(pg);
                next = i + 1;
            }

            var list = new List<PathGeometry>();

            foreach (var pg in listLineBreak)
            {
                if (heightClone + pg.Bounds.Height > Height.Value)
                    break;
                if (double.IsInfinity(pg.Bounds.Height))
                {
                    var spaceHeight = glyphTypeface.GetAvgWidth(FontSize.Value);
                    heightClone += spaceHeight;
                }
                else
                {
                    heightClone += pg.Bounds.Height;
                }

                var m11 = 1.0;
                double m12 = 0;
                double m21 = 0;
                var m22 = 1.0;
                var matrixTransform = new MatrixTransform(m11, m12, m21, m22, 0, 0);
                var matrix = matrixTransform.Matrix;
                if (pg.Transform is RotateTransform)
                {
                    matrix.RotateAt(90, 0.5, 0.5);
                    matrix.Translate(0, -pg.Bounds.Height);
                }

                matrix.Translate(Width.Value - maxWidth - offsetX,
                    list.SumHeightExceptInfinity(glyphTypeface, FontSize.Value) + pg.Bounds.Height);
                matrixTransform.Matrix = matrix;
                pg.Transform = matrixTransform;
                PathGeometry.Value.AddGeometry(pg);
                list.Add(pg);
            }

            offsetX += maxWidth;
            allcount += next;
        }
    }

    public override void WithoutLineBreak(GlyphTypeface glyphTypeface)
    {
        var l = new List<PathGeometry>();
        var list = new List<PathGeometry>();
        //refresh path geometry
        PathGeometryNoRotate.Value = new PathGeometry();
        var maxWidth = 0d;
        foreach (var @char in LetterString.Value)
        {
            ushort glyphIndex;
            glyphTypeface.CharacterToGlyphMap.TryGetValue(@char, out glyphIndex);
            var geometry = glyphTypeface.GetGlyphOutline(glyphIndex, FontSize.Value, FontSize.Value);
            var pg = geometry.GetOutlinedPathGeometry();

            if (@char == '-' || @char == 'ー'
                             || @char == '='
                             || @char == '＝'
                             || @char == '～'
                             || @char == '~'
                             || @char == ':'
                             || @char == ';'
                             || @char == '('
                             || @char == ')'
                             || @char == '['
                             || @char == ']'
                             || @char == '{'
                             || @char == '}')
                pg.Transform = new RotateTransform(90);

            maxWidth = Math.Max(maxWidth, pg.Bounds.Width);
            l.Add(pg);
        }

        foreach (var pg in l)
        {
            var m11 = 1.0;
            double m12 = 0;
            double m21 = 0;
            var m22 = 1.0;
            var matrixTransform = new MatrixTransform(m11, m12, m21, m22, 0, 0);
            var matrix = matrixTransform.Matrix;
            if (pg.Transform is RotateTransform)
            {
                matrix.RotateAt(90, 0.5, 0.5);
                matrix.Translate(0, -pg.Bounds.Height);
            }

            matrix.Translate(Width.Value - maxWidth,
                list.SumHeightExceptInfinity(glyphTypeface, FontSize.Value) + pg.Bounds.Height);
            matrixTransform.Matrix = matrix;
            pg.Transform = matrixTransform;
            PathGeometry.Value.AddGeometry(pg);
            list.Add(pg);
        }
    }

    #region IClonable

    public override object Clone()
    {
        var clone = new LetterVerticalDesignerItemViewModel();
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
        dialogService.Show(nameof(LetterVerticalSetting), new DialogParameters { { "ViewModel", this } },
            ret => result = ret);
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
        designerCanvas.Focus();
        LetterSettingDialogIsOpen.Value = true;
    }
}