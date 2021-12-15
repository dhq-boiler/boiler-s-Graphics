using boilersGraphics.Properties;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Helpers
{
    /// <summary>
    /// https://qiita.com/YSRKEN/items/a96bcec8dfb0a8340a5f
    /// </summary>
    public class ResourceService : BindableBase
    {
        private static readonly ResourceService current = new ResourceService();
        public static ResourceService Current => current;

        private readonly Resources resources = new Resources();
        public Resources Resources => resources;

        /// <summary>
        /// リソースのカルチャーを変更
        /// </summary>
        /// <param name="name">カルチャー名</param>
        public void ChangeCulture(string name)
        {
            Resources.Culture = CultureInfo.GetCultureInfo(name);
            RaisePropertyChanged("Resources");
        }
    }
}
