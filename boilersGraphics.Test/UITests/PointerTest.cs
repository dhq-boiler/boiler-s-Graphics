using boilersGraphics.Test.UITests.PageObjects;
using NUnit.Framework;
using System.IO;
using System.Reflection;
using System.Threading;

namespace boilersGraphics.Test.UITests
{
    [TestFixture]
    public class PointerTest : E2ETest
    {
        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void 四角形を選択する()
        {
            var mainwindowPO = new MainWindowPO(Session);
            var msgboxPO = mainwindowPO.Click_LoadButton();
            var loaddialogPO = msgboxPO.Click_OKButton();
            loaddialogPO.InitializeActions();
            loaddialogPO.Focus_FileName();
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\rectangle_ellipse_star.xml";
            loaddialogPO.Input_FileName(loadFilePath);
            loaddialogPO.Click_OpenButton();
            loaddialogPO.Perform();

            mainwindowPO.Click_PointerTool();

            var items = mainwindowPO.Items;
            Assert.That(items, Has.Count.EqualTo(4));

            var actions = items[1].ClickAndHold(Session);
            actions.Perform();
            Assert.That(mainwindowPO.Details, Is.EqualTo("(x, y) = (71, 180) (w, h) = (302, 214)"));
            actions.Release();
            actions.Perform();
        }

        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void 四角形ー楕円ー星型の順で選択する()
        {
            var mainwindowPO = new MainWindowPO(Session);
            var msgboxPO = mainwindowPO.Click_LoadButton();
            var loaddialogPO = msgboxPO.Click_OKButton();
            loaddialogPO.InitializeActions();
            loaddialogPO.Focus_FileName();
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\rectangle_ellipse_star.xml";
            loaddialogPO.Input_FileName(loadFilePath);
            loaddialogPO.Click_OpenButton();
            loaddialogPO.Perform();

            mainwindowPO.Click_PointerTool();

            var items = mainwindowPO.Items;
            Assert.That(items, Has.Count.EqualTo(4));

            var actions = items[1].ClickAndHold(Session);
            actions.Perform();
            Assert.That(mainwindowPO.Details, Is.EqualTo("(x, y) = (71, 180) (w, h) = (302, 214)"));
            actions.Release();
            actions.Perform();

            actions = items[2].ClickAndHold(Session);
            actions.Perform();
            Assert.That(mainwindowPO.Details, Is.EqualTo("(x, y) = (278.5, 523.5) (w, h) = (333, 254)"));
            actions.Release();
            actions.Perform();

            actions = items[3].ClickAndHold(Session);
            actions.Perform();
            Assert.That(mainwindowPO.Details, Is.EqualTo("(x, y) = (520, 128) (w, h) = (359, 345)"));
            actions.Release();
            actions.Perform();
        }

        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void 四角形を選択した後選択解除する()
        {
            var mainwindowPO = new MainWindowPO(Session);
            var msgboxPO = mainwindowPO.Click_LoadButton();
            var loaddialogPO = msgboxPO.Click_OKButton();
            loaddialogPO.InitializeActions();
            loaddialogPO.Focus_FileName();
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\rectangle_ellipse_star.xml";
            loaddialogPO.Input_FileName(loadFilePath);
            loaddialogPO.Click_OpenButton();
            loaddialogPO.Perform();

            mainwindowPO.Click_PointerTool();

            var items = mainwindowPO.Items;
            Assert.That(items, Has.Count.EqualTo(4));

            var actions = items[1].ClickAndHold(Session);
            actions.Perform();
            Assert.That(mainwindowPO.Details, Is.EqualTo("(x, y) = (71, 180) (w, h) = (302, 214)"));
            actions.Release();
            actions.Perform();

            mainwindowPO.InitializeActions();
            mainwindowPO.MoveToElement(500, 500);
            mainwindowPO.Click();
            mainwindowPO.Perform();
            Assert.That(mainwindowPO.Details, Is.Empty);
        }
    }
}
