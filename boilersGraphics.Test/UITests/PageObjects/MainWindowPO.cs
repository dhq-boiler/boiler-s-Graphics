using boilersE2E.NUnit;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System.Collections.ObjectModel;
using System.Linq;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class MainWindowPO : PageObjectBase
    {
        public MainWindowPO(WindowsDriver<WindowsElement> session, E2ETestFixture testFixture)
            : base(session, testFixture)
        { 
        }

        public void SaveCanvas(string filename)
        {
            Canvas.GetScreenshot().SaveAsFile(filename);
        }

        public MessageBoxPO Click_LoadButton()
        {
            GetElementByAutomationID("Load", 20).Click();
            return new MessageBoxPO(Session, TestFixture);
        }

        public ExportDialogPO Click_ExportButton()
        {
            GetElementByAutomationID("Export", 20).Click();
            return new ExportDialogPO(Session, TestFixture);
        }

        public void Click_EdgeThicknessComboBox()
        {
            GetElementByAutomationID("EdgeThickness", 20).Click();
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

        public SelectEdgeColorDialogPO Click_SelectEdgeColorButton()
        {
            GetElementByAutomationID("SelectEdgeColor", 20).Click();
            return new SelectEdgeColorDialogPO(Session, TestFixture);
        }

        public SelectFillColorDialogPO Click_SelectFillColorButton()
        {
            GetElementByAutomationID("SelectFillColor", 20).Click();
            return new SelectFillColorDialogPO(Session, TestFixture);
        }

        public WindowsElement Canvas => GetElementBy(By.XPath("//Pane[@Name=\"DesignerScrollViewer\"][@AutomationId=\"DesignerScrollViewer\"]/Thumb[@AutomationId=\"PART_DragThumb\"]"));

        public WindowsElement DesignerScrollViewer => GetElementBy(By.XPath("//Pane[@Name=\"DesignerScrollViewer\"][@AutomationId=\"DesignerScrollViewer\"]"));

        public ReadOnlyCollection<AppiumWebElement> Items => DesignerScrollViewer.FindElements(By.ClassName("Thumb"));

        public void Click_PointerTool()
        {
            GetElementByName("pointer", 20).Click();
        }

        public void Click_LassoTool()
        {
            GetElementByName("lasso", 20).Click();
        }

        public void Click_StraightLineTool()
        {
            GetElementByName("straightline", 20).Click();
        }

        public void Click_RectangleTool()
        {
            GetElementByName("rectangle", 20).Click();
        }

        public void Click_EllipseTool()
        {
            GetElementByName("ellipse", 20).Click();
        }

        public PictureOpenFileDialogPO Click_PictureTool()
        {
            GetElementByName("picture", 20).Click();
            return new PictureOpenFileDialogPO(Session, TestFixture);
        }

        public void Click_LetterTool()
        {
            GetElementByName("letter", 20).Click();
        }

        public void Click_LetterVerticalTool()
        {
            GetElementByName("letter-vertical", 20).Click();
        }

        public void Click_PolygonTool()
        {
            GetElementByName("polygon", 20).Click();
        }

        public void Click_BezierTool()
        {
            GetElementByName("bezier", 20).Click();
        }

        public void Click_SnapPointTool()
        {
            GetElementByName("snappoint", 20).Click();
        }

        public void Click_BrushTool()
        {
            GetElementByName("brush", 20).Click();
        }

        public void Click_EraserTool()
        {
            GetElementByName("eraser", 20).Click();
        }

        public void Click_SliceTool()
        {
            GetElementByName("slice", 20).Click();
        }

        public void Click_PolyBezierTool()
        {
            GetElementByName("polybezier", 20).Click();
        }

        public void Click_PieTool()
        {
            GetElementByName("pie", 20).Click();
        }

        public void Click_DropperTool()
        {
            GetElementByName("dropper", 20).Click();
        }

        public void Click_CanvasModifierTool()
        {
            GetElementByName("canvasModifier", 20).Click();
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

        public string Coordinate => GetElementByAutomationID("Coordinate", 20).Text;
        public string CurrentOperation => GetElementByAutomationID("CurrentOperation", 20).Text;
        public string Details => GetElementByAutomationID("Details", 20).Text;
        public string Message => GetElementByAutomationID("Message", 20).Text;
    }
}
