using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;
using System.Linq;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class MainWindow : PageObjectBase
    {
        public MainWindow(WindowsDriver<WindowsElement> session)
            : base(session)
        { }

        public void SaveCanvas(string filename)
        {
            Canvas.GetScreenshot().SaveAsFile(filename);
        }

        public MessageBox Click_LoadButton()
        {
            GetElementByAutomationID("Load").Click();
            return new MessageBox(Session);
        }

        public ExportDialog Click_ExportButton()
        {
            GetElementByAutomationID("Export").Click();
            return new ExportDialog(Session);
        }

        public void Click_EdgeThicknessComboBox()
        {
            GetElementByAutomationID("EdgeThickness").Click();
        }

        //public enum EdgeThicknessComboBoxItem
        //{
        //    ET0 = 0,
        //    ET1 = 1,
        //    ET2 = 2,
        //    ET3 = 3,
        //    ET4 = 4,
        //    ET5 = 5,
        //    ET10 = 10,
        //    ET15 = 15,
        //    ET20 = 20,
        //    ET25 = 25,
        //    ET30 = 30,
        //    ET35 = 35,
        //    ET40 = 40,
        //    ET45 = 45,
        //    ET50 = 50,
        //    ET100 = 100,
        //}

        public ReadOnlyCollection<WindowsElement> EdgeThicknessComboBoxItem => Session.FindElements(By.XPath("//Window[@ClassName=\"Popup\"]/ListItem[@ClassName=\"ListBoxItem\"]"));

        public void Click_EdgeThicknessComboBoxItem(int index)
        {
            EdgeThicknessComboBoxItem.ElementAt(index).Click();
        }

        public SelectEdgeColorDialog Click_SelectEdgeColorButton()
        {
            GetElementByAutomationID("SelectEdgeColor").Click();
            return new SelectEdgeColorDialog(Session);
        }

        public SelectFillColorDialog Click_SelectFillColorButton()
        {
            GetElementByAutomationID("SelectFillColor").Click();
            return new SelectFillColorDialog(Session);
        }

        public WindowsElement Canvas => GetElementBy(By.XPath("//Pane[@Name=\"DesignerScrollViewer\"][@AutomationId=\"DesignerScrollViewer\"]/Thumb[@AutomationId=\"PART_DragThumb\"]"));

        public WindowsElement DesignerScrollViewer => GetElementBy(By.XPath("//Pane[@Name=\"DesignerScrollViewer\"][@AutomationId=\"DesignerScrollViewer\"]"));

        public ReadOnlyCollection<AppiumWebElement> Items => DesignerScrollViewer.FindElements(By.ClassName("Thumb"));

        public void Click_PointerTool()
        {
            GetElementByName("pointer").Click();
        }

        public void Click_LassoTool()
        {
            GetElementByName("lasso").Click();
        }

        public void Click_StraightLineTool()
        {
            GetElementByName("straightline").Click();
        }

        public void Click_RectangleTool()
        {
            GetElementByName("rectangle").Click();
        }

        public void Click_EllipseTool()
        {
            GetElementByName("ellipse").Click();
        }

        public PictureOpenFileDialog Click_PictureTool()
        {
            GetElementByName("picture").Click();
            return new PictureOpenFileDialog(Session);
        }

        public void Click_LetterTool()
        {
            GetElementByName("letter").Click();
        }

        public void Click_LetterVerticalTool()
        {
            GetElementByName("letter-vertical").Click();
        }

        public void Click_PolygonTool()
        {
            GetElementByName("polygon").Click();
        }

        public void Click_BezierTool()
        {
            GetElementByName("bezier").Click();
        }

        public void Click_SnapPointTool()
        {
            GetElementByName("snappoint").Click();
        }

        public void Click_BrushTool()
        {
            GetElementByName("brush").Click();
        }

        public void Click_EraserTool()
        {
            GetElementByName("eraser").Click();
        }

        public void Click_SliceTool()
        {
            GetElementByName("slice").Click();
        }

        public void Click_PolyBezierTool()
        {
            GetElementByName("polybezier").Click();
        }

        public void Click_PieTool()
        {
            GetElementByName("pie").Click();
        }

        public void Click_DropperTool()
        {
            GetElementByName("dropper").Click();
        }

        public void Click_CanvasModifierTool()
        {
            GetElementByName("canvasModifier").Click();
        }

        private Actions action;

        public void InitializeActions()
        {
            action = new Actions(Session);
        }

        public void MoveToElement(int x, int y)
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.MoveToElement(Canvas, x, y);
        }

        public void Click()
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.Click();
        }

        public void ClickAndHold()
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.ClickAndHold();
        }

        public void Release()
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.Release();
        }

        public void MoveByOffset(int x, int y)
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.MoveByOffset(x, y);
        }

        public void Perform()
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.Perform();
        }

        public string Coordinate => GetElementByAutomationID("Coordinate").Text;
        public string CurrentOperation => GetElementByAutomationID("CurrentOperation").Text;
        public string Details => GetElementByAutomationID("Details").Text;
        public string Message => GetElementByAutomationID("Message").Text;
    }
}
