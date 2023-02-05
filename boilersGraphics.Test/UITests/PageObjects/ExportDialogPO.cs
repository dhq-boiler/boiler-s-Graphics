using boilersE2E;
using NUnit.Framework;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace boilersGraphics.Test.UITests.PageObjects
{
    public class ExportDialogPO : PageObjectBase
    {
        public ExportDialogPO(WindowsDriver<WindowsElement> session)
            : base(session)
        { }

        private Actions action;

        public void InitializeActions()
        {
            action = new Actions(Session);
        }


        public void Input_FileName(string filename)
        {
            if (action is null)
            {
                Assert.Fail($"まずInitializeActionsメソッドでアクションを初期化する必要があります。");
            }
            action.InputText(GetElementByAutomationID("filename"), filename);
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
