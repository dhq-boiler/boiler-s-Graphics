using grapher.Models;
using grapher.Views.Behaviors;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows;
using System.Windows.Media;

namespace grapher.ViewModels
{
    class RectangleViewModel : RenderItemViewModel
    {
        private DropAcceptDescription _description;

        public ReactiveProperty<Brush> Stroke { get; set; }

        public ReactiveProperty<Brush> Fill { get; set; }

        public DropAcceptDescription Description
        {
            get { return this._description; }
            set { SetProperty(ref _description, value); }
        }

        public RectangleViewModel(Rectangle model)
            : base(model)
        {
            Stroke = model
                .ObserveProperty(x => x.Stroke)
                .ToReactiveProperty();
            Fill = model
                .ObserveProperty(x => x.Fill)
                .ToReactiveProperty();
            Description = new DropAcceptDescription();
            Description.DragOver += Description_DragOver;
            Description.DragDrop += Description_DragDrop;
        }

        private void Description_DragOver(System.Windows.DragEventArgs obj)
        {
            if (obj.AllowedEffects.HasFlag(DragDropEffects.Copy))
            {
                if (obj.Data.GetDataPresent(typeof(RectangleViewModel)))
                {
                    return;
                }
            }
            obj.Effects = DragDropEffects.None;
        }

        private void Description_DragDrop(System.Windows.DragEventArgs obj)
        {
            throw new System.NotImplementedException();
        }
    }
}
