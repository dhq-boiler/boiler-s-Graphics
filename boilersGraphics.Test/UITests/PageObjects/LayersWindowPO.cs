using boilersE2E;
using boilersE2E.NUnit;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class LayersWindowPO : PageObjectBase
    {
        public LayersWindowPO(WindowsDriver<WindowsElement> session, E2ETestFixture testFixture)
            : base(session, testFixture)
        { }

        public class LayerItemPO : PageObjectBase
        {
            private readonly WindowsElement x;

            public LayerItemPO(WindowsDriver<WindowsElement> session, E2ETestFixture testFixture, WindowsElement x) : base(session, testFixture)
            {
                this.x = x;
            }

            public void ToggleButton(int count)
            {
                for (int i = 0;i < count; i++)
                {
                    x.GetElementBy(By.XPath("//Button[@AutomationId=\"LayerItem_ToggleButton\"]"), 60 * 5).Click();
                }
            }

            public Screenshot AppearanceImage
            {
                get
                {
                    var targetElement = x.GetElementBy(By.XPath("//Image[@AutomationId=\"Appearance_Image\"]"));
                    var beginDateTime = DateTime.Now;

                    while ((DateTime.Now - beginDateTime).TotalMinutes < TimeoutMinutes)
                    {
                        try
                        {
                            return targetElement.GetScreenshot();
                        }
                        catch (WebDriverException)
                        {
                            Thread.Sleep(100);
                        }
                    }

                    throw new TimeoutException($"処理開始 {beginDateTime} から {(DateTime.Now - beginDateTime).ToStringEx("hhhmmmsss")} 経ちました。");
                }
            }

            public IEnumerable<LayerItemPO> LayerItems => GetElementsBy(By.XPath("//Custom[@AutomationId=\"DiagramControl\"]/Custom[@AutomationId=\"layers\"]/Group[@ClassName=\"Expander\"]/Tree[@AutomationId=\"LayersTreeView\"]/TreeItem"), 60 * 5).Select(x => new LayerItemPO(Session, TestFixture, x));
        }

        public IEnumerable<LayerItemPO> LayerItems => GetElementsBy(By.XPath("//Custom[@AutomationId=\"DiagramControl\"]/Custom[@AutomationId=\"layers\"]/Group[@ClassName=\"Expander\"]/Tree[@AutomationId=\"LayersTreeView\"]/TreeItem"), 60 * 5).Select(x => new LayerItemPO(Session, TestFixture, x));
    }
}
