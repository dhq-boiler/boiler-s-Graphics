using NLog;
using Prism.Mvvm;
using Reactive.Bindings;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace boilersGraphics.ViewModels
{
    public abstract class PropertyOptionsValueCombination : BindableBase
    {
        protected Logger logger = LogManager.GetCurrentClassLogger();
        public ReactivePropertySlim<string> PropertyName { get; set; } = new ReactivePropertySlim<string>();
        public ReactivePropertySlim<int> TabIndex { get; set; } = new ReactivePropertySlim<int>();

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

        public PropertyOptionsValueCombinationClass(E obj, string name, HorizontalAlignment horizontalContentAlignment, V[] options)
            : this(obj, name, options)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationClass(E obj, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationClass(E obj, string parent, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name, horizontalContentAlignment)
        {
            Parent = new ReactivePropertySlim<string>(parent);
        }

        public ReactivePropertySlim<E> Object { get; set; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; set; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        public ReactivePropertySlim<V> PropertyValue
        {
            get
            {
                if (Parent is not null && !string.IsNullOrEmpty(Parent.Value))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(Parent.Value).GetValue(Object.Value)).Value;
                    return a is null ? null : ((ReactivePropertySlim<V>)a.GetType().GetProperty(PropertyName.Value).GetValue(a));
                }
                return ((ReactivePropertySlim<V>)typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value));
            }
            set
            {
                if (Parent is not null && !string.IsNullOrEmpty(Parent.Value))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(Parent.Value).GetValue(Object.Value)).Value;
                    if (a is null)
                    {

                    }
                    else
                    {
                        a.GetType().GetProperty(PropertyName.Value).SetValue(a, value);
                    }
                }
                typeof(E).GetProperty(PropertyName.Value).SetValue(Object.Value, value);
            }
        }

        public override string Type => (Options.Count() > 0) ? "ComboBox" : "TextBox";

        public ReactivePropertySlim<string> Parent { get; }
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

        public PropertyOptionsValueCombinationStruct(E obj, string name, HorizontalAlignment horizontalContentAlignment, V[] options)
            : this(obj, name, options)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationStruct(E obj, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationStruct(E obj, string parent, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name, horizontalContentAlignment)
        {
            Parent = new ReactivePropertySlim<string>(parent);
        }

        public ReactivePropertySlim<E> Object { get; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        public ReactivePropertySlim<V> PropertyValue
        {
            get
            {
                if (Parent is not null && !string.IsNullOrEmpty(Parent.Value))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(Parent.Value).GetValue(Object.Value)).Value;
                    return a is null ? default : ((ReactivePropertySlim<V>)a.GetType().GetProperty(PropertyName.Value).GetValue(a));
                }
                return ((ReactivePropertySlim<V>)typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value));
            }
            set
            {
                if (Parent is not null && !string.IsNullOrEmpty(Parent.Value))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(Parent.Value).GetValue(Object.Value)).Value;
                    if (a is null)
                    {

                    }
                    else
                    {
                        a.GetType().GetProperty(PropertyName.Value).SetValue(a, value);
                    }
                }
                typeof(E).GetProperty(PropertyName.Value).SetValue(Object.Value, value);
            }
        }

        public override string Type => (Options.Count() > 0) ? "ComboBox" : "TextBox";

        public ReactivePropertySlim<string> Parent { get; }
    }

    public class PropertyOptionsValueCombinationReadOnlyStruct<E, V> : PropertyOptionsValueCombination where V : struct
    {
        public PropertyOptionsValueCombinationReadOnlyStruct(E obj, string name) : base(name)
        {
            Object.Value = obj;
        }

        public PropertyOptionsValueCombinationReadOnlyStruct(E obj, string name, V[] options)
            : base(name)
        {
            Object.Value = obj;
            Options.Clear();
            Options.AddRange(options);
        }

        public PropertyOptionsValueCombinationReadOnlyStruct(E obj, string name, HorizontalAlignment horizontalContentAlignment, V[] options)
            : this(obj, name, options)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationReadOnlyStruct(E obj, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationReadOnlyStruct(E obj, string parent, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name, horizontalContentAlignment)
        {
            Parent = new ReactivePropertySlim<string>(parent);
        }

        public ReactivePropertySlim<E> Object { get; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        public ReadOnlyReactivePropertySlim<V> PropertyValue
        {
            get
            {
                if (Parent is not null && !string.IsNullOrEmpty(Parent.Value))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(Parent.Value).GetValue(Object.Value)).Value;
                    return a is null ? default : ((ReadOnlyReactivePropertySlim<V>)a.GetType().GetProperty(PropertyName.Value).GetValue(a));
                }
                return ((ReadOnlyReactivePropertySlim<V>)typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value));
            }
            set
            {

            }
        }

        public override string Type => (Options.Count() > 0) ? "ReadOnlyComboBox" : "ReadOnlyTextBox";

        public ReactivePropertySlim<string> Parent { get; }
    }

    public class PropertyOptionsValueCombinationStructRP<E, V> : PropertyOptionsValueCombination where V : struct
    {
        public PropertyOptionsValueCombinationStructRP(E obj, string name) : base(name)
        {
            Object.Value = obj;
        }

        public PropertyOptionsValueCombinationStructRP(E obj, string name, V[] options)
            : base(name)
        {
            Object.Value = obj;
            Options.Clear();
            Options.AddRange(options);
        }

        public PropertyOptionsValueCombinationStructRP(E obj, string name, HorizontalAlignment horizontalContentAlignment, V[] options)
            : this(obj, name, options)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationStructRP(E obj, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationStructRP(E obj, string parent, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name, horizontalContentAlignment)
        {
            Parent = new ReactivePropertySlim<string>(parent);
        }

        public ReactivePropertySlim<E> Object { get; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        //public ReactiveProperty<V> PropertyValue
        //{
        //    get
        //    {
        //        return ((ReactiveProperty<V>)typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value));
        //    }
        //}
        public ReactiveProperty<V> PropertyValue
        {
            get
            {
                if (Parent is not null && !string.IsNullOrEmpty(Parent.Value))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(Parent.Value).GetValue(Object.Value)).Value;
                    return a is null ? default : ((ReactiveProperty<V>)a.GetType().GetProperty(PropertyName.Value).GetValue(a));
                }
                return ((ReactiveProperty<V>)typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value));
            }
            set
            {
                if (Parent is not null && !string.IsNullOrEmpty(Parent.Value))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(Parent.Value).GetValue(Object.Value)).Value;
                    if (a is null)
                    {

                    }
                    else
                    {
                        a.GetType().GetProperty(PropertyName.Value).SetValue(a, value);
                    }
                }
                typeof(E).GetProperty(PropertyName.Value).SetValue(Object.Value, value);
            }
        }

        public override string Type => (Options.Count() > 0) ? "ComboBox" : "TextBox";

        public ReactivePropertySlim<string> Parent { get; }
    }

    public class PropertyOptionsValueCombinationReadOnlyStructRP<E, V> : PropertyOptionsValueCombination where V : struct
    {
        public PropertyOptionsValueCombinationReadOnlyStructRP(E obj, string name) : base(name)
        {
            Object.Value = obj;
        }

        public PropertyOptionsValueCombinationReadOnlyStructRP(E obj, string name, V[] options)
            : base(name)
        {
            Object.Value = obj;
            Options.Clear();
            Options.AddRange(options);
        }

        public PropertyOptionsValueCombinationReadOnlyStructRP(E obj, string name, HorizontalAlignment horizontalContentAlignment, V[] options)
            : this(obj, name, options)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationReadOnlyStructRP(E obj, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationReadOnlyStructRP(E obj, string parent, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name, horizontalContentAlignment)
        {
            Parent = new ReactivePropertySlim<string>(parent);
        }

        public ReactivePropertySlim<E> Object { get; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        //public ReactiveProperty<V> PropertyValue
        //{
        //    get
        //    {
        //        return ((ReactiveProperty<V>)typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value));
        //    }
        //}
        public ReadOnlyReactiveProperty<V> PropertyValue
        {
            get
            {
                if (Parent is not null && !string.IsNullOrEmpty(Parent.Value))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(Parent.Value).GetValue(Object.Value)).Value;
                    return a is null ? default : ((ReadOnlyReactiveProperty<V>)a.GetType().GetProperty(PropertyName.Value).GetValue(a));
                }
                return ((ReadOnlyReactiveProperty<V>)typeof(E).GetProperty(PropertyName.Value).GetValue(Object.Value));
            }
            set
            {
            }
        }

        public override string Type => (Options.Count() > 0) ? "ReadOnlyComboBox" : "ReadOnlyTextBox";

        public ReactivePropertySlim<string> Parent { get; }
    }
}