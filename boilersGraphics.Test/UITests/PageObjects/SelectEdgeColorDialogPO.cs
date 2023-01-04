using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class SelectEdgeColorDialogPO : PageObjectBase
    {
        private DragAndDropAction _ColorMapDADA;

        public SelectEdgeColorDialogPO(WindowsDriver<WindowsElement> session)
            : base(session)
        { }

        public void Click_Solid()
        {
            GetElementByAutomationID("Solid").Click();
            _ColorMapDADA = new DragAndDropAction(ColorMap);
        }
        
        public void Click_Linear()
        {
            GetElementByAutomationID("Linear").Click();
        }
        
        public void Click_Radial()
        {
            GetElementByAutomationID("Radial").Click();
        }

        public void Click_OK()
        {
            GetElementByAutomationID("OK").Click();
        }

        public WindowsElement ColorMap => GetElementByAutomationID("ColorMap");

        public WindowsElement ColorMapThumb => GetElementByAutomationID("ColorMapThumb");

        public DragAndDropAction ColorMapDADA { get { return _ColorMapDADA; } }

        public class DragAndDropAction
        {
            public DragAndDropAction(WindowsElement windowsElement) {
                WindowsElement = windowsElement;
            }

            private Actions actions;

            public WindowsElement WindowsElement { get; }

            public void Initialize()
            {
                actions = new Actions(Session);
            }

            public void Click()
            {
                actions.Click();
            }

            public void ClickAndHold()
            {
                actions.ClickAndHold();
            }

            public void MoveTo(int x, int y)
            {
                actions.MoveToElement(WindowsElement, x, y);
            }

            public void MoveByOffset(int x, int y)
            {
                actions.MoveByOffset(x, y);
            }

            public void Release()
            {
                actions.Release();
            }

            public void Perform()
            {
                actions.Perform();
            }
        }
    }
}
