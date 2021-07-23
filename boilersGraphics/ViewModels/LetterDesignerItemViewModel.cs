using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.Views;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace boilersGraphics.ViewModels
{
    public class LetterDesignerItemViewModel : DesignerItemViewModelBase
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

        public LetterDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public LetterDesignerItemViewModel()
            : base()
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
            this.ObserveProperty(x => x.IsSelected)
                .Subscribe(isSelected =>
                {
                    if (isSelected)
                    {
                        if (!LetterSettingDialogIsOpen)
                        {
                            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
                            IDialogResult result = null;
                            dialogService.Show(nameof(LetterSetting), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
                            var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                            designerCanvas.Focus();
                            LetterSettingDialogIsOpen = true;
                        }
                    }
                    else
                    {
                        CloseLetterSettingDialog();
                    }
                })
                .AddTo(_CompositeDisposable);
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
        }

        private void WithLineBreak(GlyphTypeface glyphTypeface)
        {
            //refresh path geometry
            PathGeometry.Value = new PathGeometry();
            var listLineBreak = new List<PathGeometry>();
            double width = 0d;
            double offsetY = 0d;
            int next = 0;
            int allcount = 0;
            while (allcount < LetterString.Count() && Width.Value > 0)
            {
                double maxHeight = 0d;
                var letterString = LetterString.Skip(allcount);

                listLineBreak.Clear();
                width = 0d;
                double widthClone = width;

                for (int i = 0; i < letterString.Count() && width < Width.Value; ++i)
                {
                    var @char = letterString.ElementAt(i);
                    ushort glyphIndex;
                    glyphTypeface.CharacterToGlyphMap.TryGetValue((int)@char, out glyphIndex);
                    Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, FontSize, FontSize);
                    PathGeometry pg = geometry.GetOutlinedPathGeometry();
                    if (double.IsInfinity(pg.Bounds.Width))
                    {
                        var spaceWidth = glyphTypeface.GetAvgWidth(FontSize);
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
                        var spaceWidth = glyphTypeface.GetAvgWidth(FontSize);
                        widthClone += spaceWidth;
                    }
                    else
                    {
                        widthClone += pg.Bounds.Width;
                    }
                    pg.Transform = new MatrixTransform(1.0, 0, 0, 1.0, list.SumWidthExceptInfinity(glyphTypeface, FontSize), maxHeight + offsetY);
                    PathGeometry.Value.AddGeometry(pg);
                    list.Add(pg);
                }

                offsetY += maxHeight;
                allcount += next;
            }
        }

        private void WithoutLineBreak(GlyphTypeface glyphTypeface)
        {
            var l = new List<PathGeometry>();
            var list = new List<PathGeometry>();
            //refresh path geometry
            PathGeometry.Value = new PathGeometry();
            double maxHeight = 0d;
            foreach (var @char in LetterString)
            {
                ushort glyphIndex;
                glyphTypeface.CharacterToGlyphMap.TryGetValue((int)@char, out glyphIndex);
                Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, FontSize, FontSize);
                PathGeometry pg = geometry.GetOutlinedPathGeometry();
                maxHeight = Math.Max(maxHeight, pg.Bounds.Height);
                l.Add(pg);
            }

            foreach (var pg in l)
            {
                pg.Transform = new MatrixTransform(1.0, 0, 0, 1.0, list.SumWidthExceptInfinity(glyphTypeface, FontSize), maxHeight);
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
            clone.FillColor = FillColor;
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
