using Prism.Mvvm;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace boilersGraphics.ViewModels
{
    public class PropertyOptionsValueCombination : BindableBase
    {
        public ReactivePropertySlim<string> PropertyName { get; set; } = new ReactivePropertySlim<string>();

        public PropertyOptionsValueCombination(string name)
        {
            PropertyName.Value = name;
        }

        public virtual string Type => "NONE";

        public string ShowPropertiesAndFields()
        {
            string ret = $"<{GetType().Name}>{{";

            PropertyInfo[] properties = GetType().GetProperties(
                BindingFlags.Public
                | BindingFlags.Instance);

            foreach (var property in properties.Except(new PropertyInfo[]
                                                           {
                                                               GetType().GetProperty("Parent"),
                                                               GetType().GetProperty("SelectedItems")
                                                           }))
            {
                ret += $"{property.Name}={property.GetValue(this)},";
            }

            FieldInfo[] fields = GetType().GetFields(
                BindingFlags.Public
                | BindingFlags.Instance);

            foreach (var field in fields)
            {
                ret += $"{field.Name}={field.GetValue(this)},";
            }
            ret = ret.Remove(ret.Length - 1, 1);
            ret += $"}}";
            return ret;
        }

        public override string ToString()
        {
            return ShowPropertiesAndFields();
        }
    }

    public class PropertyOptionsValueCombinationClass<E, V> : PropertyOptionsValueCombination where V : class
    {
        public PropertyOptionsValueCombinationClass(E obj, string name) : base(name)
        {
            Object.Value = obj;
        }

        public PropertyOptionsValueCombinationClass(E obj, string name, V[] options)
            : base(name)
        {
            Object.Value = obj;
            Options.Clear();
            Options.AddRange(options);
        }

        public ReactivePropertySlim<E> Object { get; set; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; set; } = new ReactiveCollection<V>();
        public V PropertyValue
        {
            get
            {
                return (typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value) as ReactivePropertySlim<V>).Value as V;
            }

            set
            {
                typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value).GetType().GetProperty("Value").SetValue(typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value), value);
            }
        }

        public override string Type => (Options.Count() > 0) ? "ComboBox" : "TextBox";
    }

    public class PropertyOptionsValueCombinationStruct<E, V> : PropertyOptionsValueCombination where V : struct
    {
        public PropertyOptionsValueCombinationStruct(E obj, string name) : base(name)
        {
            Object.Value = obj;
        }

        public PropertyOptionsValueCombinationStruct(E obj, string name, V[] options)
            : base(name)
        {
            Object.Value = obj;
            Options.Clear();
            Options.AddRange(options);
        }

        public ReactivePropertySlim<E> Object { get; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; } = new ReactiveCollection<V>();
        public V PropertyValue
        {
            get
            {
                return ((ReactivePropertySlim<V>)typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value)).Value;
            }

            set
            {
                (typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value) as ReactivePropertySlim<V>).Value = value;
            }
        }

        public override string Type => (Options.Count() > 0) ? "ComboBox" : "TextBox";
    }
}