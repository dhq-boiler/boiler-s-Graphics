using boilersGraphics.Helpers;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class HelperMiscTest
    {
        [Test]
        public void DragObject_プロパティの読み書き()
        {
            var d = new DragObject
            {
                DesiredSize = new Size(100, 200),
                ContentType = typeof(string),
            };
            Assert.That(d.DesiredSize, Is.EqualTo(new Size(100, 200)));
            Assert.That(d.ContentType, Is.EqualTo(typeof(string)));
        }

        [Test]
        public void DragObject_DesiredSize未設定でnull許容()
        {
            var d = new DragObject();
            Assert.That(d.DesiredSize, Is.Null);
            Assert.That(d.ContentType, Is.Null);
        }

        [Test]
        public void ToolBoxData_ctorで両プロパティが設定される()
        {
            var data = new ToolBoxData("path/to/icon.png", typeof(int));
            Assert.That(data.ImageUrl, Is.EqualTo("path/to/icon.png"));
            Assert.That(data.Type, Is.EqualTo(typeof(int)));
        }

        [Test]
        public void ColorExchange_プロパティの読み書き()
        {
            var oldBrush = new SolidColorBrush(Colors.Red);
            var newBrush = new SolidColorBrush(Colors.Blue);
            var x = new ColorExchange { Old = oldBrush, New = newBrush };
            Assert.That(x.Old, Is.SameAs(oldBrush));
            Assert.That(x.New, Is.SameAs(newBrush));
        }

        [Test]
        public void ClipboardDTO_ctorとRoot()
        {
            var dto = new ClipboardDTO("payload");
            Assert.That(dto.Root, Is.EqualTo("payload"));
            dto.Root = "updated";
            Assert.That(dto.Root, Is.EqualTo("updated"));
        }

        [Test]
        public void ClipboardDTO_ClipboardFormatは決め打ち定数()
        {
            Assert.That(ClipboardDTO.ClipboardFormat, Is.EqualTo("boilersGraphics.ClipboardDTO"));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void BindingProxy_DataDependencyPropertyを介して値を保持()
        {
            var proxy = new BindingProxy();
            proxy.Data = "hello";
            Assert.That(proxy.Data, Is.EqualTo("hello"));
            Assert.That(proxy.GetValue(BindingProxy.DataProperty), Is.EqualTo("hello"));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void BindingProxy_Cloneは別インスタンス()
        {
            var proxy = new BindingProxy { Data = 42 };
            var clone = (BindingProxy)proxy.Clone();
            Assert.That(clone, Is.Not.SameAs(proxy));
            Assert.That(clone.Data, Is.EqualTo(42));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void DebugAction_Invokeは例外を投げない()
        {
            // DebugAction は internal で Invoke は protected なのでリフレクション経由
            var actionType = typeof(boilersGraphics.Helpers.ClipboardDTO).Assembly
                .GetType("boilersGraphics.Helpers.DebugAction");
            Assert.That(actionType, Is.Not.Null);
            var action = Activator.CreateInstance(actionType, nonPublic: true);
            var invoke = actionType.GetMethod("Invoke",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(() => invoke.Invoke(action, new object[] { null }), Throws.Nothing);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void WpfObjectSerializer_Brushのラウンドトリップ()
        {
            var brush = new SolidColorBrush(Color.FromArgb(255, 10, 20, 30));
            var xaml = WpfObjectSerializer.Serialize(brush);
            Assert.That(xaml, Is.Not.Null.And.Not.Empty);

            var restored = WpfObjectSerializer.Deserialize(xaml);
            Assert.That(restored, Is.InstanceOf<SolidColorBrush>());
            Assert.That(((SolidColorBrush)restored).Color,
                Is.EqualTo(Color.FromArgb(255, 10, 20, 30)));
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void WpfObjectSerializer_Deserialize不正XAMLでnullを返す()
        {
            var result = WpfObjectSerializer.Deserialize("not an xml at all");
            Assert.That(result, Is.Null);
        }

        [Test, RequiresThread(ApartmentState.STA)]
        public void WpfObjectSerializer_ファイル経由のSerializeとDeserialize()
        {
            var brush = new SolidColorBrush(Colors.Magenta);
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"wpfobj_{Guid.NewGuid():N}.xaml");
            try
            {
                WpfObjectSerializer.Serialize(brush, path);
                Assert.That(File.Exists(path), Is.True);

                var restored = WpfObjectSerializer.Deserialize(path, null);
                Assert.That(restored, Is.InstanceOf<SolidColorBrush>());
                Assert.That(((SolidColorBrush)restored).Color, Is.EqualTo(Colors.Magenta));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Test]
        public void Path_GetRoamingDirectoryは非空文字列を返す()
        {
            var path = boilersGraphics.Helpers.Path.GetRoamingDirectory();
            Assert.That(path, Is.Not.Null.And.Not.Empty);
        }
    }
}
