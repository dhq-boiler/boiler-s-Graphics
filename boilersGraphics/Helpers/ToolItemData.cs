using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace boilersGraphics.Helpers
{
    public class ToolItemData : BindableBase
    {
        private bool _IsChecked;

        public ToolItemData(string name, string imageUrl, ICommand command)
        {
            this.Name = name;
            this.ImageUrl = imageUrl;
            this.Command = command;
        }

        public string Name { get; private set; }
        public string ImageUrl { get; private set; }
        public ICommand Command { get; private set; }

        public bool IsChecked
        {
            get { return _IsChecked; }
            set { SetProperty(ref _IsChecked, value); }
        }
    }
}
