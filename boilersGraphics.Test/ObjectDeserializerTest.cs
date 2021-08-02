using boilersGraphics.Helpers;
using boilersGraphics.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace boilersGraphics.Test
{
    [TestFixture]
    public class ObjectDeserializerTest
    {
        [Test]
        public void XML文字列を読み取る()
        {
            string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
  <boilersGraphics>
    <Version>2.1</Version>
    <Layers>
      <Layer>
        <IsVisible>true</IsVisible>
        <Name>レイヤー1</Name>
        <Color>#FF044CE6</Color>
        <LayerItems>
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
                    </DesignerItem>
                  </Item>
                <Children />
              </LayerItem>
            </Children>
          </LayerItem>
        </LayerItems>
      </Layer>
    </Layers>
    <Configuration>
      <Width>1000</Width>
      <Height>1000</Height>
      <CanvasBackground>#FFFFFFFF</CanvasBackground>
      <EnablePointSnap>true</EnablePointSnap>
      <SnapPower>10</SnapPower>
    </Configuration>
  </boilersGraphics>";

            var diagramVM = new DiagramViewModel();
            var root = XElement.Parse(xml);
            diagramVM.Layers.Clear();
            ObjectDeserializer.ReadObjectsFromXML(diagramVM, root);
            Assert.That(diagramVM.Layers.Count, Is.EqualTo(1));
        }
    }
}
