using boilersGraphics.Properties;
using Prism.Mvvm;
using System.Globalization;

namespace boilersGraphics.Helpers;

/// <summary>
///     https://qiita.com/YSRKEN/items/a96bcec8dfb0a8340a5f
/// </summary>
public class ResourceService : BindableBase
{
    public static ResourceService Current { get; } = new();

    public Resources Resources { get; } = new();

    /// <summary>
    ///     リソースのカルチャーを変更
    /// </summary>
    /// <param name="name">カルチャー名</param>
    public void ChangeCulture(string name)
    {
        Resources.Culture = CultureInfo.GetCultureInfo(name);
        RaisePropertyChanged("Resources");
    }
}