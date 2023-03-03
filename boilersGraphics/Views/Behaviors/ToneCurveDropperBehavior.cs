using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.ColorCorrect;
using Microsoft.Xaml.Behaviors;
using NLog;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using boilersGraphics.Helpers;
using Prism.Services.Dialogs;

namespace boilersGraphics.Views.Behaviors
{
    public class ToneCurveDropperBehavior : Behavior<DesignerCanvas>
    {
        private ToneCurveViewModel _viewModel;
        private Color _color;
        private Cursor _cursor;

        public ToneCurveDropperBehavior(ToneCurveViewModel viewModel, Color color, Cursor cursor)
        {
            _cursor = cursor;
            _color = color;
            _viewModel = viewModel;
        }

        protected override void OnAttached()
        {
            AssociatedObject.StylusDown += AssociatedObject_StylusDown;
            AssociatedObject.StylusMove += AssociatedObject_StylusMove;
            AssociatedObject.TouchDown += AssociatedObject_TouchDown;
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.Cursor = _cursor;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.StylusDown -= AssociatedObject_StylusDown;
            AssociatedObject.StylusMove -= AssociatedObject_StylusMove;
            AssociatedObject.TouchDown -= AssociatedObject_TouchDown;
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.Cursor = Cursors.Arrow;
            base.OnDetaching();
        }

        private void AssociatedObject_TouchDown(object sender, TouchEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("TouchDown");
            SetColor(e);
        }

        private void AssociatedObject_StylusMove(object sender, StylusEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("StylusMove");
            if (e.StylusDevice.InAir)
            {
                e.Handled = true;
                return;
            }

            SetColor(e);
            e.Handled = true;
        }

        private void AssociatedObject_StylusDown(object sender, StylusDownEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("StylusDown");
            SetColor(e);
            e.Handled = true;
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("MouseMove");
            if (e.StylusDevice != null && e.StylusDevice.IsValid)
                return;
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            SetColor(e);
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Debug("MouseDown");
            if (e.StylusDevice != null && e.StylusDevice.IsValid)
                return;

            SetColor(e);
        }

        private void SetColor(MouseEventArgs e)
        {
            var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(_viewModel.ViewModel.Value.Rect.Value,
                DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
                _viewModel.ViewModel.Value, 0, _viewModel.ViewModel.Value.ZIndex.Value - 1);
            OpenCvSharpHelper.ImShow("TEST", rtb);
            var writeableBitmap = new WriteableBitmap(rtb);
            var position = e.GetPosition(designerCanvas);
            var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);

            var window = System.Windows.Window.GetWindow(App.Current.Windows.OfType<DialogWindow>().FirstOrDefault());
            if (window is null)
                return;
            var landmarks = window.EnumerateChildOfType<LandmarkControl>().Distinct();
            var landmark = landmarks.First(x => x.PathColor == _viewModel.Curves[1].Color.Value);

            _viewModel.RemoveAllPointsInInterval(_viewModel.Curves[1]);
            _viewModel.AddNewPoint(_viewModel.Curves[1],
                new ToneCurveViewModel.Point(color.B, byte.MaxValue - _color.B));
            _viewModel.Curves[1].InOutPairs = landmark.AllScales;

            landmark = landmarks.First(x => x.PathColor == _viewModel.Curves[2].Color.Value);
            _viewModel.RemoveAllPointsInInterval(_viewModel.Curves[2]);
            _viewModel.AddNewPoint(_viewModel.Curves[2],
                new ToneCurveViewModel.Point(color.G, byte.MaxValue - _color.G));
            _viewModel.Curves[2].InOutPairs = landmark.AllScales;

            landmark = landmarks.First(x => x.PathColor == _viewModel.Curves[3].Color.Value);
            _viewModel.RemoveAllPointsInInterval(_viewModel.Curves[3]);
            _viewModel.AddNewPoint(_viewModel.Curves[3],
                new ToneCurveViewModel.Point(color.R, byte.MaxValue - _color.R));
            _viewModel.Curves[3].InOutPairs = landmark.AllScales;

            _viewModel.ViewModel.Value.Render();
        }

        private void SetColor(StylusEventArgs e)
        {
            var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(_viewModel.ViewModel.Value.Rect.Value,
                DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
                _viewModel.ViewModel.Value);
            var writeableBitmap = new WriteableBitmap(rtb);
            var position = e.GetPosition(designerCanvas);
            var color = writeableBitmap.GetPixel((int)position.X, (int)position.Y);

            var window = System.Windows.Window.GetWindow(App.Current.Windows.OfType<DialogWindow>().First());
            var landmarks = window.EnumerateChildOfType<LandmarkControl>().Distinct();
            var landmark = landmarks.First(x => x.PathColor == _viewModel.Curves[1].Color.Value);

            _viewModel.RemoveAllPointsInInterval(_viewModel.Curves[1]);
            _viewModel.AddNewPoint(_viewModel.Curves[1],
                new ToneCurveViewModel.Point(color.B, byte.MaxValue - _color.B));
            _viewModel.Curves[1].InOutPairs = landmark.AllScales;

            landmark = landmarks.First(x => x.PathColor == _viewModel.Curves[2].Color.Value);
            _viewModel.RemoveAllPointsInInterval(_viewModel.Curves[2]);
            _viewModel.AddNewPoint(_viewModel.Curves[2],
                new ToneCurveViewModel.Point(color.G, byte.MaxValue - _color.G));
            _viewModel.Curves[2].InOutPairs = landmark.AllScales;

            landmark = landmarks.First(x => x.PathColor == _viewModel.Curves[3].Color.Value);
            _viewModel.RemoveAllPointsInInterval(_viewModel.Curves[3]);
            _viewModel.AddNewPoint(_viewModel.Curves[3],
                new ToneCurveViewModel.Point(color.R, byte.MaxValue - _color.R));
            _viewModel.Curves[3].InOutPairs = landmark.AllScales;

            _viewModel.ViewModel.Value.Render();
        }

        private void SetColor(TouchEventArgs e)
        {
            var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();
            var rtb = new EffectRenderer(new WpfVisualTreeHelper()).Render(_viewModel.ViewModel.Value.Rect.Value,
                DesignerCanvas.GetInstance(), DiagramViewModel.Instance, DiagramViewModel.Instance.BackgroundItem.Value,
                _viewModel.ViewModel.Value);
            var writeableBitmap = new WriteableBitmap(rtb);
            var position = e.GetTouchPoint(designerCanvas);
            var color = writeableBitmap.GetPixel((int)position.Position.X, (int)position.Position.Y);

            var window = System.Windows.Window.GetWindow(App.Current.Windows.OfType<DialogWindow>().First());
            var landmarks = window.EnumerateChildOfType<LandmarkControl>().Distinct();
            var landmark = landmarks.First(x => x.PathColor == _viewModel.Curves[1].Color.Value);

            _viewModel.RemoveAllPointsInInterval(_viewModel.Curves[1]);
            _viewModel.AddNewPoint(_viewModel.Curves[1],
                new ToneCurveViewModel.Point(color.B, byte.MaxValue - _color.B));
            _viewModel.Curves[1].InOutPairs = landmark.AllScales;

            landmark = landmarks.First(x => x.PathColor == _viewModel.Curves[2].Color.Value);
            _viewModel.RemoveAllPointsInInterval(_viewModel.Curves[2]);
            _viewModel.AddNewPoint(_viewModel.Curves[2],
                new ToneCurveViewModel.Point(color.G, byte.MaxValue - _color.G));
            _viewModel.Curves[2].InOutPairs = landmark.AllScales;

            landmark = landmarks.First(x => x.PathColor == _viewModel.Curves[3].Color.Value);
            _viewModel.RemoveAllPointsInInterval(_viewModel.Curves[3]);
            _viewModel.AddNewPoint(_viewModel.Curves[3],
                new ToneCurveViewModel.Point(color.R, byte.MaxValue - _color.R));
            _viewModel.Curves[3].InOutPairs = landmark.AllScales;

            _viewModel.ViewModel.Value.Render();
        }
    }
}