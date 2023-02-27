﻿using boilersGraphics.Test.UITests.PageObjects;
using NUnit.Framework;
using OpenCvSharp;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace boilersGraphics.Test.UITests
{
    [TestFixture]
    public class LayersWindowTest : E2ETest
    {
        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void レイヤー１のアピアランスが正しく映っている()
        {
            var mainwindowPO = new MainWindowPO(Session);
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\Vincent_Willem_van_Gogh_127.xml";
            var msgboxPO = mainwindowPO.Click_LoadButton();
            var loaddialogPO = msgboxPO.Click_OKButton();
            loaddialogPO.InitializeActions();
            loaddialogPO.Focus_FileName();
            loaddialogPO.Input_FileName(loadFilePath);
            loaddialogPO.Click_OpenButton();
            loaddialogPO.Perform();

            Task.Delay(10000).Wait();

            var layerswindowPO = new LayersWindowPO(Session);

            foreach (var layerItem in layerswindowPO.LayerItems)
            {
                //レイヤーアイテムはデフォルトで展開されるので、トグル不要

                //レイヤー１
                try
                {
                    using (var mat = layerItem.AppearanceImage.ToMat("XXXXXXXXXX.png"))
                    {
                        Assert.That(mat.Mean()[0], Is.EqualTo(72.09263157894736).Within(1));
                        Assert.That(mat.Mean()[1], Is.EqualTo(169.56842105263158).Within(1));
                        Assert.That(mat.Mean()[2], Is.EqualTo(185.15157894736842).Within(1));
                    }
                }
                finally
                {
                    File.Delete("XXXXXXXXXX.png");
                }
            }
        }

        [Test, Apartment(ApartmentState.STA)]
        [Retry(3)]
        public void アイテム１のアピアランスが正しく映っている()
        {
            var mainwindowPO = new MainWindowPO(Session);
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var loadFilePath = $"{dir}\\XmlFiles\\Vincent_Willem_van_Gogh_127.xml";
            var msgboxPO = mainwindowPO.Click_LoadButton();
            var loaddialogPO = msgboxPO.Click_OKButton();
            loaddialogPO.InitializeActions();
            loaddialogPO.Focus_FileName();
            loaddialogPO.Input_FileName(loadFilePath);
            loaddialogPO.Click_OpenButton();
            loaddialogPO.Perform();

            Task.Delay(10000).Wait();

            var layerswindowPO = new LayersWindowPO(Session);

            foreach (var layerItem in layerswindowPO.LayerItems)
            {
                //レイヤーアイテムはデフォルトで展開されるので、トグル不要

                var layerItems = layerItem.LayerItems;
                try
                {
                    //アイテム１
                    using (var mat = layerItems.ElementAt(0).AppearanceImage.ToMat("YYYYYYYYYY.png"))
                    {
                        Assert.That(mat.Mean()[0], Is.EqualTo(72.09263157894736).Within(1));
                        Assert.That(mat.Mean()[1], Is.EqualTo(169.56842105263158).Within(1));
                        Assert.That(mat.Mean()[2], Is.EqualTo(185.15157894736842).Within(1));
                    }
                }
                finally
                {
                    File.Delete("YYYYYYYYYY.png");
                }
            }
        }
    }
}
