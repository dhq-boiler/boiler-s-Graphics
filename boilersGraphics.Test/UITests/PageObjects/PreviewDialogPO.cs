using boilersE2E;
using boilersE2E.NUnit;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class PreviewDialogPO : PageObjectBase
    {
        public PreviewDialogPO(WindowsDriver<WindowsElement> session, E2ETestFixture testFixture) : base(session, testFixture)
        { }

        public WindowsElement PreviewImage => GetElementBy(By.XPath($"//*/Image[@Name=\"Preview\"][@AutomationId=\"Preview\"]"));


        private Actions action;

        public void InitializeActions()
        {
            action = new Actions(Session);
        }

        public void ScreenShot_PreviewImage(string filename)
        {
            PreviewImage.GetScreenshot().SaveAsFile(filename);
        }

        public void Input_FileName(string filename)
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            TestFixture.InputText(GetElementByAutomationID("filename"), filename);
        }

        public void Click_PerformExportButton()
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.Click(GetElementByAutomationID("PerformExport"));
        }

        public void Perform()
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.Perform();
        }
    }
}
