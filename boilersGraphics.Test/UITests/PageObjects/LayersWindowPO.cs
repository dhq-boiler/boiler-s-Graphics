using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class LayersWindowPO : PageObjectBase
    {
        public LayersWindowPO(WindowsDriver<WindowsElement> session) : base(session)
        {
        }

        public class LayerItemPO : PageObjectBase
        {
            private readonly WindowsElement x;

            public LayerItemPO(WindowsDriver<WindowsElement> session, WindowsElement x) : base(session)
            {
                this.x = x;
            }

            public void ToggleButton(int count)
            {
                for (int i = 0;i < count; i++)
                {
                    x.GetElementBy(By.XPath("//Button[@AutomationId=\"LayerItem_ToggleButton\"]")).Click();
                }
            }

            public Screenshot AppearanceImage => x.GetElementBy(By.XPath("//Image[@AutomationId=\"Appearance_Image\"]")).GetScreenshot();

            public IEnumerable<LayerItemPO> LayerItems => GetElementsBy(By.XPath("//Custom[@AutomationId=\"DiagramControl\"]/Custom[@AutomationId=\"layers\"]/Group[@ClassName=\"Expander\"]/Tree[@AutomationId=\"LayersTreeView\"]/TreeItem")).Select(x => new LayerItemPO(Session, x));
        }

        public IEnumerable<LayerItemPO> LayerItems => GetElementsBy(By.XPath("//Custom[@AutomationId=\"DiagramControl\"]/Custom[@AutomationId=\"layers\"]/Group[@ClassName=\"Expander\"]/Tree[@AutomationId=\"LayersTreeView\"]/TreeItem")).Select(x => new LayerItemPO(Session, x));
    }
}
