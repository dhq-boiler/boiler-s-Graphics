using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Helpers;
using boilersGraphics.Views;
using NLog;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.ViewModels
{
    public class MosaicViewModel : DesignerItemViewModelBase
    {
        private static readonly string ENTRY_POINT = "main";
        private static readonly string PIXEL_SHADER_2_0 = "ps_2_0";
        private ReactivePropertySlim<IList<byte>> _bytecode = new ReactivePropertySlim<IList<byte>>();
        private ReactivePropertySlim<string> _errorMessage = new ReactivePropertySlim<string>();

        public ReactivePropertySlim<RenderTargetBitmap> Bitmap { get; } = new ReactivePropertySlim<RenderTargetBitmap>();

        public ReactivePropertySlim<double> ColumnPixels { get; } = new ReactivePropertySlim<double>(30d);
        public ReactivePropertySlim<double> RowPixels { get; } = new ReactivePropertySlim<double>(30d);

        public MosaicViewModel()
        {
            Source = new ReactivePropertySlim<string>(@"
sampler2D input : register(s0);
float width : register(c0);
float height : register(c1);
float cp : register(c2);
float rp : register(c3);

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float x = cp;
    float y = rp;
    float l = 0.0;
    if (width > height)
    {
        x = x * width / height;
        l = 0.5 / y;
    }
    else
    {
        y = y * height / width;
        l = 0.5 / x;
    }
    float2 uv2 = float2(0.5 * (floor(uv.x * x) + ceil(uv.x * x)) / x, 0.5 * (floor(uv.y * y) + ceil(uv.y * y)) / y);
    return tex2D(input, uv2);
}
");
            Bytecode = _bytecode.ToReadOnlyReactivePropertySlim();
            ErrorMessage = _errorMessage.ToReadOnlyReactivePropertySlim();

            Source
                .Delay(TimeSpan.FromMilliseconds(500))
                .Select(value =>
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        try
                        {
                            return ShaderBytecode.Compile(value, ENTRY_POINT, PIXEL_SHADER_2_0);
                        }
                        catch (Exception e)
                        {
                            return new CompilationResult(null, SharpDX.Result.Fail, e.StackTrace);
                        }
                    }
                    return new CompilationResult(null, SharpDX.Result.Ok, string.Empty);
                })
                .ObserveOnDispatcher()
                .Subscribe(value =>
                {
                    if (value != null)
                    {
                        _bytecode.Value = value.Bytecode?.Data;
                        _errorMessage.Value = value.Message;
                    }
                    else
                    {
                        _bytecode.Value = null;
                        _errorMessage.Value = string.Empty;
                    }
                });

            (Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.AllItems.Subscribe(items =>
            {
                foreach (var item in items)
                {
                    item.BeginMonitor(() =>
                    {
                        Render();
                    }).AddTo(_CompositeDisposable);
                }
            }).AddTo(_CompositeDisposable);

            ColumnPixels.Subscribe(_ =>
            {
                CompilationResult result = null;
                try
                {
                    result = ShaderBytecode.Compile(Source.Value, ENTRY_POINT, PIXEL_SHADER_2_0);
                }
                catch (Exception e)
                {
                    result = new CompilationResult(null, SharpDX.Result.Fail, e.StackTrace);
                }

                if (result != null)
                {
                    _bytecode.Value = result.Bytecode?.Data;
                    _errorMessage.Value = result.Message;
                }
                else
                {
                    _bytecode.Value = null;
                    _errorMessage.Value = string.Empty;
                }
                Render();
            }).AddTo(_CompositeDisposable);
            RowPixels.Subscribe(_ =>
            {
                CompilationResult result = null;
                try
                {
                    result = ShaderBytecode.Compile(Source.Value, ENTRY_POINT, PIXEL_SHADER_2_0);
                }
                catch (Exception e)
                {
                    result = new CompilationResult(null, SharpDX.Result.Fail, e.StackTrace);
                }

                if (result != null)
                {
                    _bytecode.Value = result.Bytecode?.Data;
                    _errorMessage.Value = result.Message;
                }
                else
                {
                    _bytecode.Value = null;
                    _errorMessage.Value = string.Empty;
                }
                Render();
            }).AddTo(_CompositeDisposable);
        }

        public void Render()
        {
            if (this.Width.Value <= 0 || this.Height.Value <= 0)
            {
                return;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                RenderTargetBitmap rtb = Renderer.Render(new System.Windows.Rect(Left.Value, Top.Value, Width.Value, Height.Value), App.Current.MainWindow.GetChildOfType<DesignerCanvas>(), (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel, (App.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.BackgroundItem.Value, this);
                Bitmap.Value = rtb;
            });
        }

        public override void OnRectChanged(Rect rect)
        {
            Render();
        }

        public ReactivePropertySlim<string> Source
        {
            get;
        }

        public ReadOnlyReactivePropertySlim<IList<byte>> Bytecode
        {
            get;
        }

        public ReadOnlyReactivePropertySlim<string> ErrorMessage
        {
            get;
        }

        public override bool SupportsPropertyDialog => true;

        public override object Clone()
        {
            var clone = new MosaicViewModel();
            clone.Owner = Owner;
            clone.Left.Value = Left.Value;
            clone.Top.Value = Top.Value;
            clone.Width.Value = Width.Value;
            clone.Height.Value = Height.Value;
            clone.EdgeBrush.Value = EdgeBrush.Value;
            clone.FillBrush.Value = FillBrush.Value;
            clone.EdgeThickness.Value = EdgeThickness.Value;
            clone.RotationAngle.Value = RotationAngle.Value;
            clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
            clone.StrokeDashArray.Value = StrokeDashArray.Value;
            clone.StrokeMiterLimit.Value = StrokeMiterLimit.Value;
            clone.ColumnPixels.Value = ColumnPixels.Value;
            clone.RowPixels.Value = RowPixels.Value;
            clone.Bitmap.Value = Bitmap.Value;
            return clone;
        }

        public override PathGeometry CreateGeometry(bool flag = false)
        {
            return GeometryCreator.CreateRectangle(this, 0, 0, flag);
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            return GeometryCreator.CreateRectangleWithAngle(this, 0, 0, RotationAngle.Value);
        }

        public override Type GetViewType()
        {
            return typeof(Image);
        }

        public override void OpenPropertyDialog()
        {
            var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.Show(nameof(DetailMosaic), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
        }
    }
}
