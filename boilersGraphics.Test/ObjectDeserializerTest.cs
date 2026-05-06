using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ObjectDeserializerTest
    {
        [Test]
        public void XML文字列を読み取る()
        {
            boilersGraphics.App.IsTest = true;
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
  <boilersGraphics>
    <Version>2.1</Version>
    <Layers>
      <Layer>
        <IsVisible>true</IsVisible>
        <Name>レイヤー1</Name>
        <Color>#FF044CE6</Color>
        <Children>
          <LayerItem>
            <IsVisible>true</IsVisible>
            <Name>アイテム6</Name>
            <Color>#FFD7AB14</Color>
            <Item>
                <DesignerItem>
                  <ID>72138b71-0a93-4b1d-865f-b7453fd5e71f</ID>
                  <ParentID>00000000-0000-0000-0000-000000000000</ParentID>
                  <Type>boilersGraphics.ViewModels.GroupItemViewModel</Type>
                  <Left>98</Left>
                  <Top>68</Top>
                  <Width>416</Width>
                  <Height>494.5</Height>
                  <ZIndex>1</ZIndex>
                  <Matrix>Identity</Matrix>
                  <EdgeColor>#00000000</EdgeColor>
                  <FillColor>#00000000</FillColor>
                  <EdgeThickness>0</EdgeThickness>
                  <PathGeometry />
                  <RotationAngle>0</RotationAngle>
                </DesignerItem>
            </Item>
            <Children>
              <LayerItem>
                 <IsVisible>true</IsVisible>
                 <Name>アイテム4</Name>
                 <Color>#FFEE9BA6</Color>
                 <Item>
                    <DesignerItem>
                       <ID>356fa282-1d29-4fa5-beca-0f71a0fcf15a</ID>
                       <ParentID>72138b71-0a93-4b1d-865f-b7453fd5e71f</ParentID>
                       <Type>boilersGraphics.ViewModels.NEllipseViewModel</Type>
                       <Left>98</Left>
                       <Top>68</Top>
                       <Width>416</Width>
                       <Height>238</Height>
                       <ZIndex>-3</ZIndex>
                       <Matrix>Identity</Matrix>
                       <EdgeColor>#FF000000</EdgeColor>
                       <FillColor>#00000000</FillColor>
                       <EdgeThickness>1</EdgeThickness>
                       <PathGeometry>M514,187C514,252.721885229864 420.875227964805,306 306,306 191.124772035195,306 98,252.721885229864 98,187 98,121.278114770136 191.124772035195,68 306,68 420.875227964805,68 514,121.278114770136 514,187z</PathGeometry>
                       <RotationAngle>0</RotationAngle>
                    </DesignerItem>
                 </Item>
                 <Children />
              </LayerItem>
              <LayerItem>
                <IsVisible>true</IsVisible>
                <Name>アイテム5</Name>
                <Color>#FF139263</Color>
                <Item>
                  <DesignerItem>
                    <ID>3e3d3907-770f-404d-82f6-4c852aa11732</ID>
                    <ParentID>72138b71-0a93-4b1d-865f-b7453fd5e71f</ParentID>
                    <Type>boilersGraphics.ViewModels.NEllipseViewModel</Type>
                    <Left>98.5</Left>
                    <Top>363.5</Top>
                    <Width>411</Width>
                    <Height>199</Height>
                    <ZIndex>0</ZIndex>
                    <Matrix>Identity</Matrix>
                    <EdgeColor>#FF000000</EdgeColor>
                    <FillColor>#00000000</FillColor>
                    <EdgeThickness>1</EdgeThickness>
                    <PathGeometry>M509.5,463C509.5,517.952332608164 417.494516090228,562.5 304,562.5 190.505483909772,562.5 98.5,517.952332608164 98.5,463 98.5,408.047667391836 190.505483909772,363.5 304,363.5 417.494516090228,363.5 509.5,408.047667391836 509.5,463z</PathGeometry>
                    <RotationAngle>0</RotationAngle>
                  </DesignerItem>
                </Item>
                <Children />
              </LayerItem>
            </Children>
          </LayerItem>
        </Children>
      </Layer>
    </Layers>
    <Configuration>
      <Width>1000</Width>
      <Height>1000</Height>
      <CanvasFillBrush>#FFFFFFFF</CanvasFillBrush>
      <EnablePointSnap>true</EnablePointSnap>
      <SnapPower>10</SnapPower>
    </Configuration>
  </boilersGraphics>";

            var dlgService = new Mock<IDialogService>();
            MainWindowViewModel mainWindowViewModel = new MainWindowViewModel(dlgService.Object);
            var progressVM = new ProgressBarWithOutputViewModel();
            var diagramVM = new DiagramViewModel(mainWindowViewModel);
            var root = XElement.Parse(xml);
            diagramVM.Layers.Clear();
            ObjectDeserializer.ReadObjectsFromXML(diagramVM, progressVM, root);
            Assert.That(diagramVM.Layers.Count, Is.EqualTo(1));
            var layer = diagramVM.Layers[0];
            Assert.That(layer.Name.Value, Is.EqualTo("レイヤー1"));
            Assert.That(layer.Color.Value, Is.EqualTo((Color)ColorConverter.ConvertFromString("#FF044CE6")));
            Assert.That(layer.Children.Count, Is.EqualTo(1));
            var layerItem = layer.Children[0];
            Assert.That(layerItem.Name.Value, Is.EqualTo("アイテム6"));
            Assert.That(layerItem.Color.Value, Is.EqualTo((Color)ColorConverter.ConvertFromString("#FFD7AB14")));
            var layerItemChildren = layerItem.Children;
            Assert.That(layerItemChildren.Count, Is.EqualTo(2));
            Assert.That(layerItemChildren[0].Name.Value, Is.EqualTo("アイテム4"));
            Assert.That(layerItemChildren[1].Name.Value, Is.EqualTo("アイテム5"));
        }

        [Test]
        public void CountObjectsFromXML_LayersFormat_ChildLayerItem数を返す()
        {
            string xml = @"<boilersGraphics>
              <Layers>
                <Layer>
                  <Name>L1</Name>
                  <Children>
                    <LayerItem><Name>A</Name></LayerItem>
                    <LayerItem><Name>B</Name></LayerItem>
                    <LayerItem><Name>C</Name></LayerItem>
                  </Children>
                </Layer>
              </Layers>
            </boilersGraphics>";
            var root = XElement.Parse(xml);
            Assert.That(ObjectDeserializer.CountObjectsFromXML(root), Is.EqualTo(3));
        }

        [Test]
        public void CountObjectsFromXML_複数レイヤーで合算()
        {
            string xml = @"<boilersGraphics>
              <Layers>
                <Layer>
                  <Children>
                    <LayerItem/>
                    <LayerItem/>
                  </Children>
                </Layer>
                <Layer>
                  <Children>
                    <LayerItem/>
                  </Children>
                </Layer>
              </Layers>
            </boilersGraphics>";
            var root = XElement.Parse(xml);
            Assert.That(ObjectDeserializer.CountObjectsFromXML(root), Is.EqualTo(3));
        }

        [Test]
        public void CountObjectsFromXML_LayersなしならDesignerItemsとConnectionsを合算()
        {
            string xml = @"<boilersGraphics>
              <DesignerItems>
                <DesignerItem><ID>1</ID></DesignerItem>
                <DesignerItem><ID>2</ID></DesignerItem>
              </DesignerItems>
              <Connections>
                <Connection><ID>3</ID></Connection>
              </Connections>
            </boilersGraphics>";
            var root = XElement.Parse(xml);
            Assert.That(ObjectDeserializer.CountObjectsFromXML(root), Is.EqualTo(3));
        }

        [Test]
        public void CountObjectsFromXML_空のXMLでも0を返す()
        {
            var root = XElement.Parse("<boilersGraphics />");
            Assert.That(ObjectDeserializer.CountObjectsFromXML(root), Is.EqualTo(0));
        }

        [Test]
        public void CountObjectsFromXML_LayersありDesignerItemsありはLayers側を採用()
        {
            // Layers がある場合は DesignerItems は無視される（先に return 構造）
            string xml = @"<boilersGraphics>
              <Layers>
                <Layer>
                  <Children>
                    <LayerItem/>
                  </Children>
                </Layer>
              </Layers>
              <DesignerItems>
                <DesignerItem/>
                <DesignerItem/>
              </DesignerItems>
            </boilersGraphics>";
            var root = XElement.Parse(xml);
            Assert.That(ObjectDeserializer.CountObjectsFromXML(root), Is.EqualTo(1));
        }

        [Test]
        public void Base64StringToBitmap_有効なPNGをBitmapImageに変換できる()
        {
            // 1x1 透明 PNG (67 bytes) の Base64
            string pngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";
            var bmp = ObjectDeserializer.Base64StringToBitmap(pngBase64);
            Assert.That(bmp, Is.Not.Null);
            Assert.That(bmp.IsFrozen, Is.True);
            Assert.That(bmp.PixelWidth, Is.EqualTo(1));
            Assert.That(bmp.PixelHeight, Is.EqualTo(1));
        }
    }
}
