﻿using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class MainWindow : PageObjectBase
    {
        public MainWindow(WindowsDriver<WindowsElement> session)
            : base(session)
        { }

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

        public WindowsElement Canvas => GetElementBy(By.XPath("//Pane[@Name=\"DesignerScrollViewer\"][@AutomationId=\"DesignerScrollViewer\"]/Thumb[@AutomationId=\"PART_DragThumb\"]"));

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
            action.MoveToElement(Canvas, x, y);
        }

        public void Click()
        {
            action.Click();
        }

        public void ClickAndHold()
        {
            action.ClickAndHold();
        }

        public void Release()
        {
            action.Release();
        }

        public void MoveByOffset(int x, int y)
        {
            action.MoveByOffset(x, y);
        }

        public void Perform()
        {
            action.Perform();
        }
    }
}
