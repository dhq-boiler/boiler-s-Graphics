using boilersE2E;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class LoadDialogPO : PageObjectBase
    {
        public LoadDialogPO(WindowsDriver<WindowsElement> session)
            : base(session)
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
            action.InputText(GetElementByAutomationID("1148"), filename);
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