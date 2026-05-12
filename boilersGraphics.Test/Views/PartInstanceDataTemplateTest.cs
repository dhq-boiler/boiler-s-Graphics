using boilersGraphics.ViewModels.Parts;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace boilersGraphics.Test.Views;

[TestFixture]
public class PartInstanceDataTemplateTest
{
    [Test, RequiresThread(ApartmentState.STA)]
    public void PartInstanceDataTemplate_リソースが構文エラーなくロードできる()
    {
        var uri = new Uri(
            "/boilersGraphics;component/Resources/DesignerItems/PartInstanceDesignerItemDataTemplate.xaml",
            UriKind.Relative);

        var dict = (ResourceDictionary)Application.LoadComponent(uri);

        Assert.That(dict, Is.Not.Null);
        var template = dict.Values.OfType<DataTemplate>().FirstOrDefault();
        Assert.That(template, Is.Not.Null, "PartInstanceViewModel 用 DataTemplate が無い");
        Assert.That(template!.DataType, Is.EqualTo(typeof(PartInstanceViewModel)));
    }
}
