using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Xps.Serialization;
using boilersGraphics.Controls;
using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using OpenCvSharp;
using Prism.Services.Dialogs;
using Rect = System.Windows.Rect;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class RendererTest
    {
        [Test, Apartment(ApartmentState.STA)]
        public void 白色のみ全体を撮影()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel);
            viewModel.Initialize();

            var designerCanvas = new DesignerCanvas();

            viewModel.BackgroundItem.Value = new BackgroundViewModel(viewModel);
            viewModel.BackgroundItem.Value.Width.Value = 10;
            viewModel.BackgroundItem.Value.Height.Value = 10;
            viewModel.BackgroundItem.Value.FillBrush.Value = new SolidColorBrush(Colors.White);
            viewModel.BackgroundItem.Value.EdgeThickness.Value = 0;

            var backgroundRect = new Rectangle()
            {
                DataContext = viewModel.BackgroundItem.Value,
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.White)
            };
            Canvas.SetLeft(backgroundRect, 0);
            Canvas.SetTop(backgroundRect, 0);
            designerCanvas.Children.Add(backgroundRect);

            var mock = new Mock<IVisualTreeHelper>();
            mock.Setup(x => x.GetDescendantBounds(backgroundRect)).Returns(new Rect(0, 0, 10, 10));
            var renderer = new Renderer(mock.Object);
            var rtb = renderer.Render(null, designerCanvas, viewModel, viewModel.BackgroundItem.Value, null);

            using var mat = OpenCvSharpHelper.ToMat(rtb);
            for (var y = 0; y < 10; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    Assert.That(mat.At<Vec3b>(y, x), Is.EqualTo(new Vec3b(255, 255, 255)));
                }
            }
        }

        [Test, Apartment(ApartmentState.STA)]
        public void 赤青白全体を撮影()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel);
            viewModel.Initialize();

            var designerCanvas = new DesignerCanvas();

            viewModel.BackgroundItem.Value = new BackgroundViewModel(viewModel);
            viewModel.BackgroundItem.Value.Left.Value = 0;
            viewModel.BackgroundItem.Value.Top.Value = 0;
            viewModel.BackgroundItem.Value.Width.Value = 10;
            viewModel.BackgroundItem.Value.Height.Value = 10;
            viewModel.BackgroundItem.Value.FillBrush.Value = new SolidColorBrush(Colors.White);
            viewModel.BackgroundItem.Value.EdgeThickness.Value = 0;

            var redRectViewModel = new NRectangleViewModel();
            redRectViewModel.Left.Value = 2;
            redRectViewModel.Top.Value = 1;
            redRectViewModel.Width.Value = 3;
            redRectViewModel.Height.Value = 3;
            redRectViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            redRectViewModel.EdgeThickness.Value = 0;
            viewModel.AddItemCommand.Execute(redRectViewModel);

            var blueRectViewModel = new NRectangleViewModel();
            blueRectViewModel.Left.Value = 3;
            blueRectViewModel.Top.Value = 5;
            blueRectViewModel.Width.Value = 6;
            blueRectViewModel.Height.Value = 3;
            blueRectViewModel.FillBrush.Value = new SolidColorBrush(Colors.Blue);
            blueRectViewModel.EdgeThickness.Value = 0;
            viewModel.AddItemCommand.Execute(blueRectViewModel);

            var backgroundRect = new Rectangle()
            {
                DataContext = viewModel.BackgroundItem.Value,
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.White)
            };
            Canvas.SetLeft(backgroundRect, 0);
            Canvas.SetTop(backgroundRect, 0);
            designerCanvas.Children.Add(backgroundRect);

            var redRect = new Rectangle()
            {
                DataContext = redRectViewModel,
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Red)
            };
            Canvas.SetLeft(redRect, 2);
            Canvas.SetTop(redRect, 1);
            designerCanvas.Children.Add(redRect);


            var blueRect = new Rectangle()
            {
                DataContext = blueRectViewModel,
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Blue)
            };
            Canvas.SetLeft(blueRect, 3);
            Canvas.SetTop(blueRect, 5);
            designerCanvas.Children.Add(blueRect);

            var mock = new Mock<IVisualTreeHelper>();
            mock.Setup(x => x.GetDescendantBounds(backgroundRect)).Returns(new Rect(0, 0, 10, 10));
            var renderer = new Renderer(mock.Object);
            var rtb = renderer.Render(null, designerCanvas, viewModel, viewModel.BackgroundItem.Value, null);

            using var mat = OpenCvSharpHelper.ToMat(rtb);

            mat.SaveImage("red_blue_white.png");

            Assert.That(mat.At<Vec3b>(0, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 2), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 3), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 4), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 5), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 6), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 8), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 9), Is.EqualTo(new Vec3b(255, 255, 255)));

            Assert.That(mat.At<Vec3b>(1, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(1, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(1, 2), Is.EqualTo(new Vec3b(0, 0, 255)));
            Assert.That(mat.At<Vec3b>(1, 3), Is.EqualTo(new Vec3b(0, 0, 255)));
            Assert.That(mat.At<Vec3b>(1, 4), Is.EqualTo(new Vec3b(0, 0, 255)));
            Assert.That(mat.At<Vec3b>(1, 5), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(1, 6), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(1, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(1, 8), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(1, 9), Is.EqualTo(new Vec3b(255, 255, 255)));

            Assert.That(mat.At<Vec3b>(2, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(2, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(2, 2), Is.EqualTo(new Vec3b(0, 0, 255)));
            Assert.That(mat.At<Vec3b>(2, 3), Is.EqualTo(new Vec3b(0, 0, 255)));
            Assert.That(mat.At<Vec3b>(2, 4), Is.EqualTo(new Vec3b(0, 0, 255)));
            Assert.That(mat.At<Vec3b>(2, 5), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(2, 6), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(2, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(2, 8), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(2, 9), Is.EqualTo(new Vec3b(255, 255, 255)));

            Assert.That(mat.At<Vec3b>(3, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(3, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(3, 2), Is.EqualTo(new Vec3b(0, 0, 255)));
            Assert.That(mat.At<Vec3b>(3, 3), Is.EqualTo(new Vec3b(0, 0, 255)));
            Assert.That(mat.At<Vec3b>(3, 4), Is.EqualTo(new Vec3b(0, 0, 255)));
            Assert.That(mat.At<Vec3b>(3, 5), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(3, 6), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(3, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(3, 8), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(3, 9), Is.EqualTo(new Vec3b(255, 255, 255)));

            Assert.That(mat.At<Vec3b>(4, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 2), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 3), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 4), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 5), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 6), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 8), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 9), Is.EqualTo(new Vec3b(255, 255, 255)));

            Assert.That(mat.At<Vec3b>(5, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(5, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(5, 2), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(5, 3), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(5, 4), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(5, 5), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(5, 6), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(5, 7), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(5, 8), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(5, 9), Is.EqualTo(new Vec3b(255, 255, 255)));

            Assert.That(mat.At<Vec3b>(6, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(6, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(6, 2), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(6, 3), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(6, 4), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(6, 5), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(6, 6), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(6, 7), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(6, 8), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(6, 9), Is.EqualTo(new Vec3b(255, 255, 255)));

            Assert.That(mat.At<Vec3b>(7, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(7, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(7, 2), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(7, 3), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(7, 4), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(7, 5), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(7, 6), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(7, 7), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(7, 8), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(7, 9), Is.EqualTo(new Vec3b(255, 255, 255)));

            Assert.That(mat.At<Vec3b>(8, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(8, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(8, 2), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(8, 3), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(8, 4), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(8, 5), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(8, 6), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(8, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(8, 8), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(8, 9), Is.EqualTo(new Vec3b(255, 255, 255)));

            Assert.That(mat.At<Vec3b>(9, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(9, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(9, 2), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(9, 3), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(9, 4), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(9, 5), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(9, 6), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(9, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(9, 8), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(9, 9), Is.EqualTo(new Vec3b(255, 255, 255)));
        }

        [Test, Apartment(ApartmentState.STA)]
        public void 赤を撮影()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel);
            viewModel.Initialize();

            var designerCanvas = new DesignerCanvas();

            viewModel.BackgroundItem.Value = new BackgroundViewModel(viewModel);
            viewModel.BackgroundItem.Value.Left.Value = 0;
            viewModel.BackgroundItem.Value.Top.Value = 0;
            viewModel.BackgroundItem.Value.Width.Value = 10;
            viewModel.BackgroundItem.Value.Height.Value = 10;
            viewModel.BackgroundItem.Value.FillBrush.Value = new SolidColorBrush(Colors.White);
            viewModel.BackgroundItem.Value.EdgeThickness.Value = 0;

            var redRectViewModel = new NRectangleViewModel();
            redRectViewModel.Left.Value = 2;
            redRectViewModel.Top.Value = 1;
            redRectViewModel.Width.Value = 3;
            redRectViewModel.Height.Value = 3;
            redRectViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            redRectViewModel.EdgeThickness.Value = 0;
            viewModel.AddItemCommand.Execute(redRectViewModel);

            var blueRectViewModel = new NRectangleViewModel();
            blueRectViewModel.Left.Value = 3;
            blueRectViewModel.Top.Value = 5;
            blueRectViewModel.Width.Value = 6;
            blueRectViewModel.Height.Value = 3;
            blueRectViewModel.FillBrush.Value = new SolidColorBrush(Colors.Blue);
            blueRectViewModel.EdgeThickness.Value = 0;
            viewModel.AddItemCommand.Execute(blueRectViewModel);

            var backgroundRect = new Rectangle()
            {
                DataContext = viewModel.BackgroundItem.Value,
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.White)
            };
            Canvas.SetLeft(backgroundRect, 0);
            Canvas.SetTop(backgroundRect, 0);
            designerCanvas.Children.Add(backgroundRect);

            var redRect = new Rectangle()
            {
                DataContext = redRectViewModel,
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Red)
            };
            Canvas.SetLeft(redRect, 2);
            Canvas.SetTop(redRect, 1);
            designerCanvas.Children.Add(redRect);


            var blueRect = new Rectangle()
            {
                DataContext = blueRectViewModel,
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.Blue)
            };
            Canvas.SetLeft(blueRect, 3);
            Canvas.SetTop(blueRect, 5);
            designerCanvas.Children.Add(blueRect);

            var mock = new Mock<IVisualTreeHelper>();
            mock.Setup(x => x.GetDescendantBounds(redRect)).Returns(new Rect(2, 1, 3, 3));
            var renderer = new Renderer(mock.Object);
            var rtb = renderer.Render(new Rect(2, 1, 3, 3), designerCanvas, viewModel, viewModel.BackgroundItem.Value, null);

            using var mat = OpenCvSharpHelper.ToMat(rtb);
            mat.SaveImage("red.png");
            for (var y = 0; y < 3; y++)
            {
                for (var x = 0; x < 3; x++)
                {
                    Assert.That(mat.At<Vec3b>(y, x), Is.EqualTo(new Vec3b(0, 0, 255)));
                }
            }
        }

        [Test, Apartment(ApartmentState.STA)]
        public void 青を撮影()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel);
            viewModel.Initialize();

            var designerCanvas = new DesignerCanvas();

            viewModel.BackgroundItem.Value = new BackgroundViewModel(viewModel);
            viewModel.BackgroundItem.Value.Left.Value = 0;
            viewModel.BackgroundItem.Value.Top.Value = 0;
            viewModel.BackgroundItem.Value.Width.Value = 10;
            viewModel.BackgroundItem.Value.Height.Value = 10;
            viewModel.BackgroundItem.Value.FillBrush.Value = new SolidColorBrush(Colors.White);
            viewModel.BackgroundItem.Value.EdgeThickness.Value = 0;

            var redRectViewModel = new NRectangleViewModel();
            redRectViewModel.Left.Value = 2;
            redRectViewModel.Top.Value = 1;
            redRectViewModel.Width.Value = 3;
            redRectViewModel.Height.Value = 3;
            redRectViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            redRectViewModel.EdgeThickness.Value = 0;
            viewModel.AddItemCommand.Execute(redRectViewModel);

            var blueRectViewModel = new NRectangleViewModel();
            blueRectViewModel.Left.Value = 3;
            blueRectViewModel.Top.Value = 5;
            blueRectViewModel.Width.Value = 6;
            blueRectViewModel.Height.Value = 3;
            blueRectViewModel.FillBrush.Value = new SolidColorBrush(Colors.Blue);
            blueRectViewModel.EdgeThickness.Value = 0;
            viewModel.AddItemCommand.Execute(blueRectViewModel);

            var backgroundRect = new Rectangle()
            {
                DataContext = viewModel.BackgroundItem.Value,
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.White)
            };
            Canvas.SetLeft(backgroundRect, 0);
            Canvas.SetTop(backgroundRect, 0);
            designerCanvas.Children.Add(backgroundRect);

            var redRect = new Rectangle()
            {
                DataContext = redRectViewModel,
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(Colors.Red)
            };
            Canvas.SetLeft(redRect, 2);
            Canvas.SetTop(redRect, 1);
            designerCanvas.Children.Add(redRect);


            var blueRect = new Rectangle()
            {
                DataContext = blueRectViewModel,
                Width = 6,
                Height = 3,
                Fill = new SolidColorBrush(Colors.Blue)
            };
            Canvas.SetLeft(blueRect, 3);
            Canvas.SetTop(blueRect, 5);
            designerCanvas.Children.Add(blueRect);

            var mock = new Mock<IVisualTreeHelper>();
            mock.Setup(x => x.GetDescendantBounds(blueRect)).Returns(new Rect(3, 5, 6, 3));
            var renderer = new Renderer(mock.Object);
            var rtb = renderer.Render(new Rect(3, 5, 6, 3), designerCanvas, viewModel, viewModel.BackgroundItem.Value, null);

            using var mat = OpenCvSharpHelper.ToMat(rtb);
            mat.SaveImage("red.png");
            for (var y = 0; y < 3; y++)
            {
                for (var x = 0; x < 6; x++)
                {
                    Assert.That(mat.At<Vec3b>(y, x), Is.EqualTo(new Vec3b(255, 0, 0)));
                }
            }
        }


        [Test, Apartment(ApartmentState.STA)]
        public void 青マージン１を撮影()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            var mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var viewModel = new DiagramViewModel(mainWindowViewModel);
            viewModel.Initialize();

            var designerCanvas = new DesignerCanvas();

            viewModel.BackgroundItem.Value = new BackgroundViewModel(viewModel);
            viewModel.BackgroundItem.Value.Left.Value = 0;
            viewModel.BackgroundItem.Value.Top.Value = 0;
            viewModel.BackgroundItem.Value.Width.Value = 10;
            viewModel.BackgroundItem.Value.Height.Value = 10;
            viewModel.BackgroundItem.Value.FillBrush.Value = new SolidColorBrush(Colors.White);
            viewModel.BackgroundItem.Value.EdgeThickness.Value = 0;

            var redRectViewModel = new NRectangleViewModel();
            redRectViewModel.Left.Value = 2;
            redRectViewModel.Top.Value = 1;
            redRectViewModel.Width.Value = 3;
            redRectViewModel.Height.Value = 3;
            redRectViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            redRectViewModel.EdgeThickness.Value = 0;
            viewModel.AddItemCommand.Execute(redRectViewModel);

            var blueRectViewModel = new NRectangleViewModel();
            blueRectViewModel.Left.Value = 3;
            blueRectViewModel.Top.Value = 5;
            blueRectViewModel.Width.Value = 6;
            blueRectViewModel.Height.Value = 3;
            blueRectViewModel.FillBrush.Value = new SolidColorBrush(Colors.Blue);
            blueRectViewModel.EdgeThickness.Value = 0;
            viewModel.AddItemCommand.Execute(blueRectViewModel);

            var backgroundRect = new Rectangle()
            {
                DataContext = viewModel.BackgroundItem.Value,
                Width = 10,
                Height = 10,
                Fill = new SolidColorBrush(Colors.White)
            };
            Canvas.SetLeft(backgroundRect, 0);
            Canvas.SetTop(backgroundRect, 0);
            designerCanvas.Children.Add(backgroundRect);

            var redRect = new Rectangle()
            {
                DataContext = redRectViewModel,
                Width = 3,
                Height = 3,
                Fill = new SolidColorBrush(Colors.Red)
            };
            Canvas.SetLeft(redRect, 2);
            Canvas.SetTop(redRect, 1);
            designerCanvas.Children.Add(redRect);


            var blueRect = new Rectangle()
            {
                DataContext = blueRectViewModel,
                Width = 6,
                Height = 3,
                Fill = new SolidColorBrush(Colors.Blue)
            };
            Canvas.SetLeft(blueRect, 3);
            Canvas.SetTop(blueRect, 5);
            designerCanvas.Children.Add(blueRect);

            var mock = new Mock<IVisualTreeHelper>();
            mock.Setup(x => x.GetDescendantBounds(blueRect)).Returns(new Rect(3, 5, 6, 3));
            var renderer = new Renderer(mock.Object);
            var rtb = renderer.Render(new Rect(2, 4, 8, 5), designerCanvas, viewModel, viewModel.BackgroundItem.Value, null);

            using var mat = OpenCvSharpHelper.ToMat(rtb);
            mat.SaveImage("blue_margin_1.png");
            
            Assert.That(mat.At<Vec3b>(0, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 2), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 3), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 4), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 5), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 6), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(0, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            
            Assert.That(mat.At<Vec3b>(1, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(1, 1), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(1, 2), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(1, 3), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(1, 4), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(1, 5), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(1, 6), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(1, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            
            Assert.That(mat.At<Vec3b>(2, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(2, 1), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(2, 2), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(2, 3), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(2, 4), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(2, 5), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(2, 6), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(2, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            
            Assert.That(mat.At<Vec3b>(3, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(3, 1), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(3, 2), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(3, 3), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(3, 4), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(3, 5), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(3, 6), Is.EqualTo(new Vec3b(255, 0, 0)));
            Assert.That(mat.At<Vec3b>(3, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
            
            Assert.That(mat.At<Vec3b>(4, 0), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 1), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 2), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 3), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 4), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 5), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 6), Is.EqualTo(new Vec3b(255, 255, 255)));
            Assert.That(mat.At<Vec3b>(4, 7), Is.EqualTo(new Vec3b(255, 255, 255)));
        }
    }
}
