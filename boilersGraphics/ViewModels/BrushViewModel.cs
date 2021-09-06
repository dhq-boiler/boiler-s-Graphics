using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace boilersGraphics.ViewModels
{
    public class BrushViewModel : DesignerItemViewModelBase
    {
        public ReactiveCommand GotFocusCommand { get; } = new ReactiveCommand();
        public ReactiveCommand LostFocusCommand { get; } = new ReactiveCommand();

        public ReactivePropertySlim<Thickness> Thickness { get; } = new ReactivePropertySlim<Thickness>();

        public ReactivePropertySlim<bool> ThicknessDialogIsOpen { get; } = new ReactivePropertySlim<bool>();

        public event EventHandler ThicknessDialogClose;

        public BrushViewModel()
            : base()
        {
            Init();
        }

        public void SetupTimedMethod()
        {
            Observable.Return(this)
                      .Delay(TimeSpan.FromMilliseconds(100))
                      .ObserveOn(new DispatcherScheduler(Dispatcher.CurrentDispatcher))
                      .Subscribe(x =>
                      {
                          var views = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().GetCorrespondingViews<FrameworkElement>(this).Where(x => x.GetType() == GetViewType());
                          views.First().Focus();
                      })
                      .AddTo(_CompositeDisposable);
        }

        private void Init()
        {
            GotFocusCommand.Subscribe(x =>
            {
                OpenThicknessDialog();
            })
            .AddTo(_CompositeDisposable);
            LostFocusCommand.Subscribe(x =>
            {
                if (ThicknessDialogIsOpen.Value)
                {
                    CloseThicknessDialog();
                }
            })
            .AddTo(_CompositeDisposable);
        }

        public void OpenThicknessDialog()
        {
            if (!ThicknessDialogIsOpen.Value)
            {
                var dialogService = new DialogService((App.Current as PrismApplication).Container as IContainerExtension);
                IDialogResult result = null;
                dialogService.Show(nameof(Thickness), new DialogParameters() { { "ViewModel", this } }, ret => result = ret);
                var designerCanvas = App.Current.MainWindow.GetChildOfType<DesignerCanvas>();
                designerCanvas.Focus();
                ThicknessDialogIsOpen.Value = true;
            }
        }

        public void CloseThicknessDialog()
        {
            ThicknessDialogClose?.Invoke(this, new EventArgs());
            ThicknessDialogIsOpen.Value = false;
        }

        public override object Clone()
        {
            var clone = new BrushViewModel();
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
            clone.PathGeometry.Value = PathGeometry.Value.Clone();
            return clone;
        }

        public override PathGeometry CreateGeometry()
        {
            throw new NotSupportedException("brush is not supported.");
        }

        public override PathGeometry CreateGeometry(double angle)
        {
            throw new NotSupportedException("brush is not supported.");
        }

        public override Type GetViewType()
        {
            return typeof(Path);
        }
    }
}
