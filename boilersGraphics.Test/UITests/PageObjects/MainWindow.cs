using OpenQA.Selenium;
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

        public void Click_SliceTool()
        {
            GetElementByName("slice").Click();
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
