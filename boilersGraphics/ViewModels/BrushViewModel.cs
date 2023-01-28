using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace boilersGraphics.ViewModels;

public class BrushViewModel : DesignerItemViewModelBase
{
    private static readonly List<BrushViewModel> list = new();

    public BrushViewModel()
    {
        Thickness.Value = RetainedValue;
        Init();
    }

    public static Thickness RetainedValue { get; private set; } = new(1);

    public ReactiveCommand GotFocusCommand { get; } = new();
    public ReactiveCommand LostFocusCommand { get; } = new();

    public ReactivePropertySlim<Thickness> Thickness { get; } = new();

    public ReactivePropertySlim<bool> ThicknessDialogIsOpen { get; } = new();

    public override bool SupportsPropertyDialog => false;

    public event EventHandler ThicknessDialogClose;

    public static BrushViewModel CreateInstance()
    {
        var obj = new BrushViewModel();
        obj.Thickness.Value = RetainedValue;
        return obj;
    }

    public void SetupTimedMethod()
    {
        Observable.Return(this)
            .Delay(TimeSpan.FromMilliseconds(100))
            .ObserveOn(new DispatcherScheduler(Dispatcher.CurrentDispatcher))
            .Subscribe(x =>
            {
                var views = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>()
                    .GetCorrespondingViews<FrameworkElement>(this).Where(x => x.GetType() == GetViewType());
                views.FirstOrDefault()?.Focus();
            })
            .AddTo(_CompositeDisposable);
    }

    private void Init()
    {
        Thickness.Subscribe(x => { list.ForEach(y => y.Thickness.Value = x); })
            .AddTo(_CompositeDisposable);
        GotFocusCommand.Subscribe(x => { OpenThicknessDialog(); })
            .AddTo(_CompositeDisposable);
    }

    public void OpenThicknessDialog()
    {
        list.Except(new[] { this }).ToList().ForEach(x => x.CloseThicknessDialog());

        if (!(Application.Current.MainWindow.DataContext as MainWindowViewModel).DiagramViewModel.EnableBrushThickness
            .Value)
            return;

        if (!ThicknessDialogIsOpen.Value)
        {
            if (RetainedValue != null)
                Thickness.Value = RetainedValue;
            list.Add(this);
            var dialogService =
                new DialogService((Application.Current as PrismApplication).Container as IContainerExtension);
            IDialogResult result = null;
            dialogService.Show(nameof(Thickness), new DialogParameters { { "ViewModel", this } }, ret => result = ret);
            var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            designerCanvas.Focus();
            ThicknessDialogIsOpen.Value = true;
        }
    }

    public void CloseThicknessDialog()
    {
        if (!ThicknessDialogIsOpen.Value)
            return;
        RetainedValue = Thickness.Value;
        ThicknessDialogClose?.Invoke(this, new EventArgs());
        list.Remove(this);
        ThicknessDialogIsOpen.Value = false;
    }

    public static void CloseAllThicknessDialog()
    {
        list.ToList().ForEach(x => x.CloseThicknessDialog());
    }

    public override object Clone()
    {
        var clone = new BrushViewModel();
        clone.Owner = Owner;
        clone.Left.Value = Left.Value;
        clone.Top.Value = Top.Value;
        clone.Width.Value = Width.Value;
        clone.Height.Value = Height.Value;
        clone.EdgeBrush.Value = EdgeBrush.Value;
        clone.FillBrush.Value = FillBrush.Value;
        clone.EdgeThickness.Value = EdgeThickness.Value;
        clone.RotationAngle.Value = RotationAngle.Value;
        clone.PathGeometryNoRotate.Value = PathGeometryNoRotate.Value.Clone();
        clone.StrokeLineJoin.Value = StrokeLineJoin.Value;
        return clone;
    }

    public override PathGeometry CreateGeometry(bool flag = false)
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

    public override void OpenPropertyDialog()
    {
        throw new NotImplementedException();
    }
}