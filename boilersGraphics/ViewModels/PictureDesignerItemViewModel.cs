using boilersGraphics.Helpers;
using boilersGraphics.Views;
using NLog;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class PictureDesignerItemViewModel : DesignerItemViewModelBase
    {
        private string _FileName;
        private double _FileWidth;
        private double _FileHeight;

        public string FileName
        {
            get { return _FileName; }
            set { SetProperty(ref _FileName, value); }
        }

        public double FileWidth
        {
            get { return _FileWidth; }
            set { SetProperty(ref _FileWidth, value); }
        }

        public double FileHeight
        {
            get { return _FileHeight; }
            set { SetProperty(ref _FileHeight, value); }
        }

        public ReactivePropertySlim<Rect> ClippingOriginRect { get; } = new ReactivePropertySlim<Rect>();

        //public ReactivePropertySlim<double> WidthInClipping { get; } = new ReactivePropertySlim<double>();

        //public ReactivePropertySlim<double> HeightInClipping { get; } = new ReactivePropertySlim<double>();

        public ReactivePropertySlim<System.Windows.Thickness> Margin { get; } = new ReactivePropertySlim<System.Windows.Thickness>();

        public ReactivePropertySlim<SelectableDesignerItemViewModelBase> ClipObject { get; set; } = new ReactivePropertySlim<SelectableDesignerItemViewModelBase>();

        public override bool SupportsPropertyDialog => true;

        public ReactivePropertySlim<BitmapImage> EmbeddedImage { get; } = new ReactivePropertySlim<BitmapImage>();

        public PictureDesignerItemViewModel(int id, DiagramViewModel parent, double left, double top)
            : base(id, parent, left, top)
        {
            Init();
        }

        public PictureDesignerItemViewModel()
        {
            Init();
        }

        private void Init()
        {
            this.ShowConnectors = false;
            EnablePathGeometryUpdate.Value = true;
            //Width.Zip(Width.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            //     .Subscribe(x =>
            //     {
            //        if (Margin.Value == new System.Windows.Thickness())
            //        {
            //            WidthInClipping.Value = x.NewItem;
            //        }
            //        if (Pool.Value != "Left")
            //        {
            //             if (x.OldItem == 0)
            //                 WidthInClipping.Value = x.NewItem;
            //             else
            //                 WidthInClipping.Value += x.NewItem - x.OldItem;
            //        }
            //     })
            //     .AddTo(_CompositeDisposable);
            //Height.Zip(Height.Skip(1), (Old, New) => new { OldItem = Old, NewItem = New })
            //      .Subscribe(x =>
            //      {
            //          if (Margin.Value == new System.Windows.Thickness())
            //          {
            //              HeightInClipping.Value = x.NewItem;
            //          }
            //          if (Pool.Value != "Top")
            //          {
            //              if (x.OldItem == 0)
            //                  HeightInClipping.Value = x.NewItem;
            //              else
            //                  HeightInClipping.Value += x.NewItem - x.OldItem;
            //          }
            //      })
            //      .AddTo(_CompositeDisposable);
        }

        public override void UpdatePathGeometryIfEnable(string propertyName, object oldValue, object newValue, bool flag = false)
        {
            if (EnablePathGeometryUpdate.Value)
            {
                if (!flag)
                {
                    try
                    {
                        var geometry = CreateGeometry(flag);
                        PathGeometryNoRotate.Value = geometry;
                        LogManager.GetCurrentClassLogger().Trace($"PathGeometryNoRotate:{PathGeometryNoRotate.Value.ToString()}");
                        if (Margin.Value != new System.Windows.Thickness())
                        {
                            var left = Margin.Value.Left;
                            var top = Margin.Value.Top;
                            var right = Margin.Value.Right;
                            var bottom = Margin.Value.Bottom;
                            if ("IsInitializing" != Pool.Value)
                            {
                                switch (propertyName)
                                {
                                    case "Left":
                                        left -= PathGeometryNoRotate.Value.Bounds.X - (double)left;
                                        //left = -((double)newValue - ClippingOriginRect.Value.X);
                                        break;
                                    case "Top":
                                        top -= PathGeometryNoRotate.Value.Bounds.Y - (double)top;
                                        //top = -((double)newValue - ClippingOriginRect.Value.Y);
                                        break;
                                    case "Width":
                                        var α = (double)newValue - (double)oldValue;
                                        var newWidth = Width.Value + α;
                                        if (Width.Value <= 0)
                                        {
                                            break;
                                        }
                                        var coefficient = newWidth / Width.Value;
                                        if (coefficient <= 0d)
                                        {
                                            break;
                                            coefficient = 1d;
                                        }
                                        //if (coefficient > 2d)
                                        //{
                                        //    break;
                                        //}
                                        var newRect = new Rect(ClippingOriginRect.Value.Left,
                                                                ClippingOriginRect.Value.Top,
                                                                ClippingOriginRect.Value.Width * coefficient,
                                                                ClippingOriginRect.Value.Height);
                                        //if (Math.Abs(ClippingOriginRect.Value.Width - newRect.Width) > 100)
                                        //{
                                        //    newRect = new Rect(ClippingOriginRect.Value.Left,
                                        //                       ClippingOriginRect.Value.Top,
                                        //                       ClippingOriginRect.Value.Width * coefficient * 0.99,
                                        //                       ClippingOriginRect.Value.Height);
                                        //}
                                        ClippingOriginRect.Value = newRect;
                                        left -= PathGeometryNoRotate.Value.Bounds.Right - ((double)newValue - left);
                                        right = -(ClippingOriginRect.Value.Width - newWidth + left);
                                        break;
                                    case "Height":
                                        var β = (double)newValue - (double)oldValue;
                                        var newHeight = Height.Value + β;
                                        if (Height.Value <= 0)
                                        {
                                            break;
                                        }
                                        coefficient = newHeight / Height.Value;
                                        if (coefficient <= 0d)
                                        {
                                            break;
                                            coefficient = 1d;
                                        }
                                        //if (coefficient > 2d)
                                        //{
                                        //    break;
                                        //}
                                        newRect = new Rect(ClippingOriginRect.Value.Left,
                                                                ClippingOriginRect.Value.Top,
                                                                ClippingOriginRect.Value.Width,
                                                                ClippingOriginRect.Value.Height * coefficient);
                                        //if (Math.Abs(ClippingOriginRect.Value.Height - newRect.Height) > 100)
                                        //{
                                        //    newRect = new Rect(ClippingOriginRect.Value.Left,
                                        //                       ClippingOriginRect.Value.Top,
                                        //                       ClippingOriginRect.Value.Width,
                                        //                       ClippingOriginRect.Value.Height * coefficient * 0.99);
                                        //}
                                        ClippingOriginRect.Value = newRect;
                                        top -= PathGeometryNoRotate.Value.Bounds.Bottom - ((double)newValue - top);
                                        bottom = -(ClippingOriginRect.Value.Height - newHeight + top);
                                        break;
                                }
                            }
                            Margin.Value = new System.Windows.Thickness(
                                left,
                                top,
                                right,
                                bottom);
                            LogManager.GetCurrentClassLogger().Trace($"Margin:{Margin.Value.ToString()}");
                            LogManager.GetCurrentClassLogger().Trace($"ClippingOriginRect:{ClippingOriginRect.Value.ToString()}");
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

                if (RotationAngle.Value != 0)
                {
                    PathGeometryRotate.Value = CreateGeometry(RotationAngle.Value);
                }
            }
        }

        public override void UpdateMargin(string propertyName, object oldValue, object newValue)
        {
            if (Margin.Value == new System.Windows.Thickness())
                return;
            
            //switch (propertyName)
            //{
            //    case "Left":
            //        if (Pool.Value == "Left")
            //        {
            //            var thickness1 = Margin.Value;
            //            thickness1.Left = (double)newValue;
            //            Margin.Value = thickness1;
            //        }
            //        break;
            //    case "Top":
            //        if (Pool.Value == "Top")
            //        {
            //            var thickness2 = Margin.Value;
            //            thickness2.Top = (double)newValue;
            //            Margin.Value = thickness2;
            //        }
            //        break;
            //    case "Width":
            //        var thickness3 = Margin.Value;
            //        thickness3.Right = thickness3.Left + (double)newValue;
            //        Margin.Value = thickness3;
            //        if (Pool.Value == "Left")
            //        {
            //            thickness3.Left = thickness3.Right - (double)newValue;
            //            Margin.Value = thickness3;
            //        }
            //        break;
            //    case "Height":
            //        var thickness4 = Margin.Value;
            //        thickness4.Bottom = thickness4.Top + (double)newValue;
            //        Margin.Value = thickness4;
            //        if (Pool.Value == "Top")
            //        {
            //            thickness3.Top = thickness3.Bottom - (double)newValue;
            //            Margin.Value = thickness3;
            //        }
            //        break;
            //}
        }

        public override PathGeometry CreateGeometry(bool flag = false)
        {
            return GeometryCreator.CreateRectangle(this, flag);
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            return GeometryCreator.CreateRectangle(this, angle);
        }

        public override Type GetViewType()
        {
            return typeof(Image);
        }

        #region IClonable

        public override object Clone()
        {
            var clone = new PictureDesignerItemViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeBrush.Value = EdgeBrush.Value;
            clone.FillBrush.Value = FillBrush.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.PathGeometryNoRotate.Value = PathGeometryNoRotate.Value;
            clone.PathGeometryRotate.Value = PathGeometryRotate.Value;
            clone.FileName = FileName;
            clone.FileWidth = FileWidth;
            clone.FileHeight = FileHeight;
            clone.PenLineJoin.Value = PenLineJoin.Value;
            clone.StrokeDashArray.Value = StrokeDashArray.Value;
            return clone;
        }

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.ShowDialog(nameof(DetailPicture), new DialogParameters() { { "ViewModel", (PictureDesignerItemViewModel)this.Clone() } }, ret => result = ret);
            if (result != null && result.Result == ButtonResult.OK)
            {
                var viewModel = result.Parameters.GetValue<PictureDesignerItemViewModel>("ViewModel");
                this.Left.Value = viewModel.Left.Value;
                this.Top.Value = viewModel.Top.Value;
                this.Width.Value = viewModel.Width.Value;
                this.Height.Value = viewModel.Height.Value;
                this.CenterX.Value = viewModel.CenterX.Value;
                this.CenterY.Value = viewModel.CenterY.Value;
                this.RotationAngle.Value = viewModel.RotationAngle.Value;
                this.PenLineJoin.Value = viewModel.PenLineJoin.Value;
                this.StrokeDashArray.Value = viewModel.StrokeDashArray.Value;
            }
        }

        #endregion //IClonable
    }
}
