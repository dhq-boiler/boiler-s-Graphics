using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Reactive.Bindings;

namespace boilersGraphics.Helpers
{
    public class ToolItemData : BindableBase
    {
        private bool _IsChecked;

        public ToolItemData(string name, string imageUrl, ICommand command)
        {
            this.Name.Value = name;
            this.ImageUrl = imageUrl;
            this.Command = command;
        }
        public ToolItemData(string name, string imageUrl, string tooltip, ICommand command)
        {
            this.Name.Value = name;
            this.ImageUrl = imageUrl;
            this.Command = command;
            this.Tooltip.Value = tooltip;
        }

        //public string Name { get; private set; }
        public ReactivePropertySlim<string> Name { get; private set; } = new ReactivePropertySlim<string>();
        public string ImageUrl { get; private set; }
        public ICommand Command { get; private set; }
        public ReactivePropertySlim<string> Tooltip { get; private set; } = new ReactivePropertySlim<string>();

        public bool IsChecked
        {
            get { return _IsChecked; }
            set { SetProperty(ref _IsChecked, value); }
        }
    }
}
