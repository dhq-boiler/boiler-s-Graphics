using boilersGraphics.Adorners;
using boilersGraphics.Controls;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class AdornerTest
    {
        [Test, RequiresThread(System.Threading.ApartmentState.STA)]
        public void BezierCurveAdornerを作成()
        {
            boilersGraphics.App.IsTest = true;
            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var diagramViewModel = new DiagramViewModel(mainWindowViewModel);
            var designerCanvas = new DesignerCanvas();
            designerCanvas.DataContext = diagramViewModel;
            diagramViewModel.EdgeBrush.Value = new SolidColorBrush(Colors.Red);
            diagramViewModel.EdgeThickness.Value = 1.0;
            var adorner = new BezierCurveAdorner(designerCanvas, new System.Windows.Point() { X = 100, Y = 100 });
            var po = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(adorner);
            Assert.That(po.GetField("_designerCanvas"), Is.EqualTo(designerCanvas));
            Assert.That(po.GetField("_startPoint"), Is.EqualTo(new System.Windows.Point() { X = 100, Y = 100 }));
        }
    }
}
