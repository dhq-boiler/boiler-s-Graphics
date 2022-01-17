using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public class LetterVerticalDesignerItemViewModel : AbstractLetterDesignerItemViewModel, ILetterDesignerItemViewModel
    {
        public override bool SupportsPropertyDialog => true;

        public LetterVerticalDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public LetterVerticalDesignerItemViewModel()
            : base()
        {
            Init();
        }

        public override void WithLineBreak(GlyphTypeface glyphTypeface)
        {
            //refresh path geometry
            PathGeometry.Value = new PathGeometry();
            var listLineBreak = new List<PathGeometry>();
            double height = 0d;
            double offsetX = 0d;
            int next = 0;
            int allcount = 0;
            while (allcount < LetterString.Count() && Height.Value > 0)
            {
                double maxWidth = 0d;
                var letterString = LetterString.Skip(allcount);

                listLineBreak.Clear();
                height = 0d;
                double heightClone = height;

                for (int i = 0; i < letterString.Count() && height < Height.Value; ++i)
                {
                    var @char = letterString.ElementAt(i);
                    ushort glyphIndex;
                    glyphTypeface.CharacterToGlyphMap.TryGetValue((int)@char, out glyphIndex);
                    Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, FontSize, FontSize);
                    PathGeometry pg = geometry.GetOutlinedPathGeometry();
                    
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
                    {
                        pg.Transform = new RotateTransform(90);
                    }

                    if (double.IsInfinity(pg.Bounds.Height))
                    {
                        var spaceHeight = glyphTypeface.GetAvgHeight(FontSize);
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
                        var spaceHeight = glyphTypeface.GetAvgWidth(FontSize);
                        heightClone += spaceHeight;
                    }
                    else
                    {
                        heightClone += pg.Bounds.Height;
                    }
                    double m11 = 1.0;
                    double m12 = 0;
                    double m21 = 0;
                    double m22 = 1.0;
                    var matrixTransform = new MatrixTransform(m11, m12, m21, m22, 0, 0);
                    var matrix = matrixTransform.Matrix;
                    if (pg.Transform is RotateTransform)
                    {
                        matrix.RotateAt(90, 0.5, 0.5);
                        matrix.Translate(0, -pg.Bounds.Height);
                    }
                    matrix.Translate(Width.Value - maxWidth - offsetX, list.SumHeightExceptInfinity(glyphTypeface, FontSize) + pg.Bounds.Height);
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
            PathGeometry.Value = new PathGeometry();
            double maxWidth = 0d;
            foreach (var @char in LetterString)
            {
                ushort glyphIndex;
                glyphTypeface.CharacterToGlyphMap.TryGetValue((int)@char, out glyphIndex);
                Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, FontSize, FontSize);
                PathGeometry pg = geometry.GetOutlinedPathGeometry();
                
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
                {
                    pg.Transform = new RotateTransform(90);
                }

                maxWidth = Math.Max(maxWidth, pg.Bounds.Width);
                l.Add(pg);
            }

            foreach (var pg in l)
            {
                double m11 = 1.0;
                double m12 = 0;
                double m21 = 0;
                double m22 = 1.0;
                var matrixTransform = new MatrixTransform(m11, m12, m21, m22, 0, 0);
                var matrix = matrixTransform.Matrix;
                if (pg.Transform is RotateTransform)
                {
                    matrix.RotateAt(90, 0.5, 0.5);
                    matrix.Translate(0, -pg.Bounds.Height);
                }
                matrix.Translate(Width.Value - maxWidth, list.SumHeightExceptInfinity(glyphTypeface, FontSize) + pg.Bounds.Height);
                matrixTransform.Matrix = matrix;
                pg.Transform = matrixTransform;
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
            clone.EdgeColor.Value = EdgeColor.Value;
            clone.FillColor.Value = FillColor.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.LetterString = LetterString;
            clone.SelectedFontFamily = SelectedFontFamily;
            clone.IsBold = IsBold;
            clone.IsItalic = IsItalic;
            clone.FontSize = FontSize;
            clone.PathGeometry = PathGeometry;
            clone.AutoLineBreak = AutoLineBreak;
            return clone;
        }

        #endregion //IClonable

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.ShowDialog(nameof(DetailLetter), new DialogParameters() { { "ViewModel", (AbstractLetterDesignerItemViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<AbstractLetterDesignerItemViewModel>("ViewModel");
                this.Left.Value = viewModel.Left.Value;
                this.Top.Value = viewModel.Top.Value;
                this.Width.Value = viewModel.Width.Value;
                this.Height.Value = viewModel.Height.Value;
                this.CenterX.Value = viewModel.CenterX.Value;
                this.CenterY.Value = viewModel.CenterY.Value;
            }
        }

        public override void OpenSettingDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.Show(nameof(LetterVerticalSetting), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            designerCanvas.Focus();
            LetterSettingDialogIsOpen = true;
        }
    }
}
