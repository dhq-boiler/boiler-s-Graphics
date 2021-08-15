using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace boilersGraphics.ViewModels
{
    public class LetterVerticalDesignerItemViewModel : DesignerItemViewModelBase, ILetterDesignerItemViewModel
    {
        private bool _LetterSettingDialogIsOpen = false;
        private string _LetterString = string.Empty;
        private FontFamilyEx _SelectedFontFamily;
        private bool _IsBold;
        private bool _IsItalic;
        private int _FontSize;
        private bool _AutoLineBreak;

        public bool LetterSettingDialogIsOpen
        {
            get { return _LetterSettingDialogIsOpen; }
            set { SetProperty(ref _LetterSettingDialogIsOpen, value); }
        }

        public string LetterString
        {
            get { return _LetterString; }
            set { SetProperty(ref _LetterString, value); }
        }

        public FontFamilyEx SelectedFontFamily
        {
            get { return _SelectedFontFamily; }
            set { SetProperty(ref _SelectedFontFamily, value); }
        }

        public bool IsBold
        {
            get { return _IsBold; }
            set { SetProperty(ref _IsBold, value); }
        }

        public bool IsItalic
        {
            get { return _IsItalic; }
            set { SetProperty(ref _IsItalic, value); }
        }

        public int FontSize
        {
            get { return _FontSize; }
            set { SetProperty(ref _FontSize, value); }
        }

        public bool AutoLineBreak
        {
            get { return _AutoLineBreak; }
            set { SetProperty(ref _AutoLineBreak, value); }
        }

        public event EventHandler LetterSettingDialogClose;

        public ReactiveCommand GotFocusCommand { get; } = new ReactiveCommand();
        public ReactiveCommand LostFocusCommand { get; } = new ReactiveCommand();

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

        private void Init()
        {
            GotFocusCommand.Subscribe(x =>
            {
                if (!LetterSettingDialogIsOpen)
                {
                    var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
                    IDialogResult result = null;
                    dialogService.Show(nameof(LetterVerticalSetting), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
                    var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                    designerCanvas.Focus();
                    LetterSettingDialogIsOpen = true;
                }
            })
            .AddTo(_CompositeDisposable);
            LostFocusCommand.Subscribe(x =>
            {
                if (LetterSettingDialogIsOpen)
                {
                    CloseLetterSettingDialog();
                }
            })
            .AddTo(_CompositeDisposable);
            this.ShowConnectors = false;
            this.ObserveProperty(x => x.LetterString)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.SelectedFontFamily)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.IsBold)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.IsItalic)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.FontSize)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.Width.Value)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            this.ObserveProperty(x => x.AutoLineBreak)
                .Subscribe(_ => RenderLetter())
                .AddTo(_CompositeDisposable);
            EnablePathGeometryUpdate.Value = true;
        }

        public void CloseLetterSettingDialog()
        {
            if (LetterSettingDialogIsOpen)
            {
                LetterSettingDialogClose?.Invoke(this, new EventArgs());
                LetterSettingDialogIsOpen = false;
            }
        }

        private void RenderLetter()
        {
            if (SelectedFontFamily != null && FontSize > 0)
            {
                var fontStyle = IsItalic ? FontStyles.Italic : FontStyles.Normal;
                var fontWeight = IsBold ? FontWeights.Bold : FontWeights.Normal;
                var typeface = new Typeface(new FontFamilyEx(SelectedFontFamily.FamilyName), fontStyle, fontWeight, FontStretches.Normal);
                GlyphTypeface glyphTypeface;
                if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
                    return;
                if (AutoLineBreak)
                    WithLineBreak(glyphTypeface);
                else
                    WithoutLineBreak(glyphTypeface);
            }
            else
            {
                PathGeometry.Value = new PathGeometry();
            }
        }

        private void WithLineBreak(GlyphTypeface glyphTypeface)
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

        private void WithoutLineBreak(GlyphTypeface glyphTypeface)
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

        public override PathGeometry CreateGeometry()
        {
            RenderLetter();
            return PathGeometry.Value;
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            RenderLetter();
            var rotatePathGeometry = PathGeometry.Value.Clone();
            rotatePathGeometry.Transform = new RotateTransform(angle, this.CenterPoint.Value.X, this.CenterPoint.Value.Y);
            return rotatePathGeometry;
        }

        public override Type GetViewType()
        {
            return typeof(Path);
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
            clone.Matrix.Value = Matrix.Value;
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
    }
}
