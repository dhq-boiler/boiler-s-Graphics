using boilersE2E.NUnit;
using boilersGraphics.Test.UITests.PageObjects;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace boilersGraphics.Test.UITests
{
    public class E2ETest : E2ETestFixture
    {
        public override string AppPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "boilersGraphics.exe");

        public override Size WindowSize => new Size(1980, 1080);

        static E2ETest()
        {
            EnvironmentVariableNameWhereWinAppDriverRunAutomatically = "BOILERSGRAPHICS_TEST_IS_VALID";
        }

        public override void DoAfterSettingWindowSize()
        {
            var privacyPolicyPO = new PrivacyPolicy(Session);
            var agreeOrOKButtonClicked = privacyPolicyPO.Click_AgreeButton_IfExists();
            agreeOrOKButtonClicked = agreeOrOKButtonClicked | privacyPolicyPO.Click_OKButton_IfExists();
            if (agreeOrOKButtonClicked)
            {
                //プライバシーポリシー画面の同意ボタンかOKボタンを押して画面を閉じた後
                //メインウィンドウを最大化する
                MaximizeWindow();
            }
        }
    }
}
