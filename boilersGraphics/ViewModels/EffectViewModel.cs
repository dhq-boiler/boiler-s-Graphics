using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using boilersGraphics.Controls;
using boilersGraphics.Extensions;

namespace boilersGraphics.ViewModels
{
    public abstract class EffectViewModel : DesignerItemViewModelBase
    {
        public ReactivePropertySlim<WriteableBitmap> Bitmap { get; }
        public abstract void Render();

        internal void UpdateLayout()
        {
            var view = App.Current.MainWindow.GetChildOfType<DesignerCanvas>().EnumVisualChildren<FrameworkElement>().FirstOrDefault(x => x.DataContext == this);
            if (view is null)
            {
                return;
            }
            view.InvalidateMeasure();
            view.InvalidateArrange();
            view.UpdateLayout();
        }
    }
}
