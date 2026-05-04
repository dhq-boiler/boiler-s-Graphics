using boilersGraphics.Controls;
using boilersGraphics.Exceptions;
using boilersGraphics.Helpers;
using boilersGraphics.Models;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class HelpersTest
    {
        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_AddNewBrushViewModel()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var desingerCanvas = new DesignerCanvas();
            desingerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            diagramViewModel.EdgeBrush.Value = new SolidColorBrush(Colors.Transparent);
            diagramViewModel.EdgeThickness.Value = 1.0;
            diagramViewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramViewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            BrushViewModel vm = new BrushViewModel();

            BrushInternal.AddNewBrushViewModel(desingerCanvas, ref vm, new System.Windows.Point() { X = 100, Y = 100 });

            Assert.That(vm.Width.Value, Is.EqualTo(1000));
            Assert.That(vm.Height.Value, Is.EqualTo(1000));
            Assert.That((vm.EdgeBrush.Value as SolidColorBrush).Color, Is.EqualTo(new SolidColorBrush(Colors.Transparent).Color));
            Assert.That((vm.FillBrush.Value as SolidColorBrush).Color, Is.EqualTo(new SolidColorBrush(Colors.Red).Color));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_Down()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var designerCanvas = new DesignerCanvas();
            designerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            diagramViewModel.EdgeBrush.Value = new SolidColorBrush(Colors.Transparent);
            diagramViewModel.EdgeThickness.Value = 1.0;
            diagramViewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramViewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            BrushViewModel vm = new BrushViewModel();

            BrushInternal.Down(mainWindowViewModel, designerCanvas, ref vm, () => new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left).MouseDevice.Capture(designerCanvas), new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left), new System.Windows.Point() { X = 50, Y = 50 });

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("M51,49C51,50.10456949966159 50.10456949966159,51 49,51 47.89543050033841,51 47,50.10456949966159 47,49 47,47.89543050033841 47.89543050033841,47 49,47 50.10456949966159,47 51,47.89543050033841 51,49z"));
        }

        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BrushInternal_Down_2回目()
        {
            App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = mainWindowViewModel.DiagramViewModel;
            var designerCanvas = new DesignerCanvas();
            designerCanvas.DataContext = diagramViewModel;
            diagramViewModel.FillBrush.Value = new SolidColorBrush(Colors.Red);
            diagramViewModel.EdgeBrush.Value = new SolidColorBrush(Colors.Transparent);
            diagramViewModel.EdgeThickness.Value = 1.0;
            diagramViewModel.Layers.Clear();
            var layer1 = new Layer();
            layer1.Name.Value = "レイヤー1";
            diagramViewModel.Layers.Add(layer1);
            layer1.IsSelected.Value = true;

            BrushViewModel vm = new BrushViewModel();

            BrushInternal.Down(mainWindowViewModel, designerCanvas, ref vm, () => new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left).MouseDevice.Capture(designerCanvas), new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left), new System.Windows.Point() { X = 50, Y = 50 }) ;

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("M51,49C51,50.10456949966159 50.10456949966159,51 49,51 47.89543050033841,51 47,50.10456949966159 47,49 47,47.89543050033841 47.89543050033841,47 49,47 50.10456949966159,47 51,47.89543050033841 51,49z"));

            mainWindowViewModel.DiagramViewModel.AllItems.Value.First().IsSelected.Value = true;

            BrushInternal.Down(mainWindowViewModel, designerCanvas, ref vm, 
                () => new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left).MouseDevice.Capture(designerCanvas), 
                new System.Windows.Input.MouseButtonEventArgs(InputManager.Current.PrimaryMouseDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = Mouse.MouseDownEvent
                }, 
                new System.Windows.Point() { X = 100, Y = 100 });

            Assert.That(vm.PathGeometry.Value.ToString(), Is.EqualTo("M101,99C101,100.1045694996616 100.1045694996616,101 99,101 97.8954305003384,101 97,100.1045694996616 97,99 97,97.8954305003384 97.8954305003384,97 99,97 100.1045694996616,97 101,97.8954305003384 101,99z"));
        }

        [Test]
        public void BrushHelper_ExtractColor_SolidColorBrush()
        {
            var brush = new SolidColorBrush(Color.FromArgb(255, 10, 20, 30));
            var color = BrushHelper.ExtractColor(brush);
            Assert.That(color, Is.EqualTo(Color.FromArgb(255, 10, 20, 30)));
        }

        [Test]
        public void BrushHelper_ExtractColor_LinearGradientBrushはNotSupportedException()
        {
            var brush = new LinearGradientBrush(Colors.Red, Colors.Blue, 0);
            Assert.That(() => BrushHelper.ExtractColor(brush), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void BrushHelper_ExtractColor_RadialGradientBrushはNotSupportedException()
        {
            var brush = new RadialGradientBrush(Colors.Red, Colors.Blue);
            Assert.That(() => BrushHelper.ExtractColor(brush), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void BrushHelper_ExtractColor_未対応BrushはUnexpectedException()
        {
            var brush = new DrawingBrush();
            Assert.That(() => BrushHelper.ExtractColor(brush), Throws.TypeOf<UnexpectedException>());
        }

        [Test]
        public void FileSize_1KB未満はbytes()
        {
            Assert.That(FileSize.ConvertFileSizeUnit(0), Does.EndWith("bytes"));
            Assert.That(FileSize.ConvertFileSizeUnit(1023), Does.EndWith("bytes"));
        }

        [Test]
        public void FileSize_10KB未満は小数点付きKB()
        {
            var s = FileSize.ConvertFileSizeUnit(2048); // 2.0 KB
            Assert.That(s, Does.EndWith("KB"));
            Assert.That(s, Does.Contain("."));
        }

        [Test]
        public void FileSize_100KB未満は整数KB()
        {
            var s = FileSize.ConvertFileSizeUnit(50 * 1024); // 50 KB
            Assert.That(s, Does.EndWith("KB"));
            Assert.That(s, Does.Not.Contain("."));
        }

        [Test]
        public void FileSize_1MB未満はKB単位()
        {
            var s = FileSize.ConvertFileSizeUnit(500 * 1024); // 500 KB
            Assert.That(s, Does.EndWith("KB"));
        }

        [Test]
        public void FileSize_10MB未満は小数点付きMB()
        {
            var s = FileSize.ConvertFileSizeUnit(2L * 1024 * 1024); // 2 MB
            Assert.That(s, Does.EndWith("MB"));
            Assert.That(s, Does.Contain("."));
        }

        [Test]
        public void FileSize_100MB未満は整数MB()
        {
            var s = FileSize.ConvertFileSizeUnit(50L * 1024 * 1024);
            Assert.That(s, Does.EndWith("MB"));
            Assert.That(s, Does.Not.Contain("."));
        }

        [Test]
        public void FileSize_1GB未満はMB単位()
        {
            var s = FileSize.ConvertFileSizeUnit(500L * 1024 * 1024);
            Assert.That(s, Does.EndWith("MB"));
        }

        [Test]
        public void FileSize_10GB未満は小数点付きGB()
        {
            var s = FileSize.ConvertFileSizeUnit(2L * 1024 * 1024 * 1024);
            Assert.That(s, Does.EndWith("GB"));
            Assert.That(s, Does.Contain("."));
        }

        [Test]
        public void FileSize_10GB以上は整数GB()
        {
            var s = FileSize.ConvertFileSizeUnit(50L * 1024 * 1024 * 1024);
            Assert.That(s, Does.EndWith("GB"));
            Assert.That(s, Does.Not.Contain("."));
        }

        [Test]
        public void BezierCurve_直線_t0は始点()
        {
            var pts = new List<Point> { new(0, 0), new(10, 10) };
            var p = BezierCurve.Evaluate(0, pts);
            Assert.That(p.X, Is.EqualTo(0).Within(1e-9));
            Assert.That(p.Y, Is.EqualTo(0).Within(1e-9));
        }

        [Test]
        public void BezierCurve_直線_t1は終点()
        {
            var pts = new List<Point> { new(0, 0), new(10, 20) };
            var p = BezierCurve.Evaluate(1, pts);
            Assert.That(p.X, Is.EqualTo(10).Within(1e-9));
            Assert.That(p.Y, Is.EqualTo(20).Within(1e-9));
        }

        [Test]
        public void BezierCurve_直線_t05は中点()
        {
            var pts = new List<Point> { new(0, 0), new(10, 20) };
            var p = BezierCurve.Evaluate(0.5, pts);
            Assert.That(p.X, Is.EqualTo(5).Within(1e-9));
            Assert.That(p.Y, Is.EqualTo(10).Within(1e-9));
        }

        [Test]
        public void BezierCurve_2次_対称制御点で中点はY最大()
        {
            // (0,0) - (5,10) - (10,0) の二次ベジエ。t=0.5 で X=5, Y=5
            var pts = new List<Point> { new(0, 0), new(5, 10), new(10, 0) };
            var p = BezierCurve.Evaluate(0.5, pts);
            Assert.That(p.X, Is.EqualTo(5).Within(1e-9));
            Assert.That(p.Y, Is.EqualTo(5).Within(1e-9));
        }

        [Test]
        public void BezierCurve_3次_t0t1で始終点()
        {
            var pts = new List<Point> { new(0, 0), new(3, 5), new(7, 5), new(10, 0) };
            var p0 = BezierCurve.Evaluate(0, pts);
            var p1 = BezierCurve.Evaluate(1, pts);
            Assert.That(p0.X, Is.EqualTo(0).Within(1e-9));
            Assert.That(p0.Y, Is.EqualTo(0).Within(1e-9));
            Assert.That(p1.X, Is.EqualTo(10).Within(1e-9));
            Assert.That(p1.Y, Is.EqualTo(0).Within(1e-9));
        }

        [Test]
        public void Randomizer_RandomColor_同じシードは同じ色を返す()
        {
            var c1 = Randomizer.RandomColor(new Random(42));
            var c2 = Randomizer.RandomColor(new Random(42));
            Assert.That(c1, Is.EqualTo(c2));
        }

        [Test]
        public void Randomizer_RandomColorBrush_SolidColorBrushを返す()
        {
            var brush = Randomizer.RandomColorBrush(new Random(0));
            Assert.That(brush, Is.InstanceOf<SolidColorBrush>());
        }

        [Test]
        public void Randomizer_RandomColor_アルファ255()
        {
            var color = Randomizer.RandomColor(new Random(123));
            Assert.That(color.A, Is.EqualTo(255));
        }
    }
}
