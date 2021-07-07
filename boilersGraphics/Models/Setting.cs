using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace boilersGraphics.Models
{
    class Setting : BindableBase
    {
        public ReactiveProperty<int> Width { get; set; } = new ReactiveProperty<int>();
        public ReactiveProperty<int> Height { get; set; } = new ReactiveProperty<int>();
    }
}
