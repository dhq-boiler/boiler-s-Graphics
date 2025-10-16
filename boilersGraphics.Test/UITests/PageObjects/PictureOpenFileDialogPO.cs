using boilersE2E;
using boilersE2E.NUnit;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class PictureOpenFileDialogPO : PageObjectBase
    {
        public PictureOpenFileDialogPO(WindowsDriver<AppiumElement> session, E2ETestFixture testFixture) : base(session, testFixture)
        { }

        private Actions action;

        public void InitializeActions()
        {
            action = new Actions(Session);
        }

        public void Focus_FileName()
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.SendKeys(Keys.Alt + "N" + Keys.Alt);
        }

        public void Input_FileName(string filename)
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            TestFixture.InputText(GetElementByAutomationID("1148"), filename);
        }

        public void Click_OpenButton()
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.Click(GetElementByAutomationID("1"));
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
