using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class SelectEdgeColorDialogPO : PageObjectBase
    {
        private DragAndDropAction _ColorMapDADA;
        private DragAndDropAction _HueDADA;
        private DragAndDropAction _AlphaDADA;
        private DragAndDropAction _RedDADA;
        private DragAndDropAction _GreenDADA;
        private DragAndDropAction _BlueDADA;

        public SelectEdgeColorDialogPO(WindowsDriver<WindowsElement> session)
            : base(session)
        { }

        public void Click_Solid()
        {
            GetElementByAutomationID("Solid").Click();
            _ColorMapDADA = new DragAndDropAction(ColorMap);
            _HueDADA = new DragAndDropAction(Hue);
            _AlphaDADA = new DragAndDropAction(Alpha);
            _RedDADA = new DragAndDropAction(Red);
            _GreenDADA = new DragAndDropAction(Green);
            _BlueDADA = new DragAndDropAction(Blue);
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
        public WindowsElement Hue => GetElementByAutomationID("Hue");
        public WindowsElement Alpha => GetElementByAutomationID("Alpha");
        public WindowsElement Red => GetElementByAutomationID("Red");
        public WindowsElement Green => GetElementByAutomationID("Green");
        public WindowsElement Blue => GetElementByAutomationID("Blue");

        public DragAndDropAction ColorMapDADA { get { return _ColorMapDADA; } }
        public DragAndDropAction HueDADA { get { return _HueDADA; } }
        public DragAndDropAction AlphaDADA { get { return _AlphaDADA; } }
        public DragAndDropAction RedDADA { get { return _RedDADA; } }
        public DragAndDropAction GreenDADA { get { return _GreenDADA; } }
        public DragAndDropAction BlueDADA { get { return _BlueDADA; } }

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
