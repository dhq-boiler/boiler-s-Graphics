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

        public ReactivePropertySlim<E> Object { get; set; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; set; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        public ReactivePropertySlim<V> PropertyValue
        {
            get
            {
                var splits = PropertyName.Value.Split('.');
                object obj = Object.Value;
                foreach (var split in splits.Except(new string[] { splits.Last() }))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(split).GetValue(obj)).Value;
                    obj = a;
                    if (obj is null)
                        return null;
                }
                if (obj is null)
                    return null;
                return ((ReactivePropertySlim<V>)typeof(E).GetProperty(splits.Last()).GetValue(obj));
            }
            set
            {
                var splits = PropertyName.Value.Split('.');
                object obj = Object.Value;
                foreach (var split in splits.Except(new string[] { splits.Last() }))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(split).GetValue(obj)).Value;
                    obj = a;
                }
                if (obj is null)
                    return;
                typeof(E).GetProperty(splits.Last()).SetValue(obj, value);
            }
        }

        public override string Type => typeof(V) == typeof(bool) ? "CheckBox" : (Options.Count() > 0) ? "ComboBox" : "TextBox";
    }

    public class PropertyOptionsValueCombinationReadOnlyClass<E, V> : PropertyOptionsValueCombination where V : class
    {
        public PropertyOptionsValueCombinationReadOnlyClass(E obj, string name) : base(name)
        {
            Object.Value = obj;
        }

        public PropertyOptionsValueCombinationReadOnlyClass(E obj, string name, V[] options)
            : base(name)
        {
            Object.Value = obj;
            Options.Clear();
            Options.AddRange(options);
        }

        public PropertyOptionsValueCombinationReadOnlyClass(E obj, string name, HorizontalAlignment horizontalContentAlignment, V[] options)
            : this(obj, name, options)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public PropertyOptionsValueCombinationReadOnlyClass(E obj, string name, HorizontalAlignment horizontalContentAlignment)
            : this(obj, name)
        {
            HorizontalContentAlignment.Value = horizontalContentAlignment;
        }

        public ReactivePropertySlim<E> Object { get; set; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; set; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        public ReadOnlyReactivePropertySlim<V> PropertyValue
        {
            get
            {
                var splits = PropertyName.Value.Split('.');
                object obj = Object.Value;
                foreach (var split in splits.Except(new string[] { splits.Last() }))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(split).GetValue(obj)).Value;
                    obj = a;
                    if (obj is null)
                        return null;
                }
                if (obj is null)
                    return null;
                return ((ReadOnlyReactivePropertySlim<V>)typeof(E).GetProperty(splits.Last()).GetValue(obj));
            }
            set
            {
            }
        }

        public override string Type => typeof(V) == typeof(bool) ? "ReadOnlyCheckBox" : (Options.Count() > 0) ? "ReadOnlyComboBox" : "ReadOnlyTextBox";
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

        public ReactivePropertySlim<E> Object { get; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        public ReactivePropertySlim<V> PropertyValue
        {
            get
            {
                var splits = PropertyName.Value.Split('.');
                object obj = Object.Value;
                foreach (var split in splits.Except(new string[] { splits.Last() }))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(split).GetValue(obj)).Value;
                    obj = a;
                    if (obj is null)
                        return null;
                }
                if (obj is null)
                    return null;
                return ((ReactivePropertySlim<V>)typeof(E).GetProperty(splits.Last()).GetValue(obj));
            }
            set
            {
                var splits = PropertyName.Value.Split('.');
                object obj = Object.Value;
                foreach (var split in splits.Except(new string[] { splits.Last() }))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(split).GetValue(obj)).Value;
                    obj = a;
                }
                if (obj is null)
                    return;
                typeof(E).GetProperty(splits.Last()).SetValue(obj, value);
            }
        }

        public override string Type => typeof(V) == typeof(bool) ? "CheckBox" : (Options.Count() > 0) ? "ComboBox" : "TextBox";
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

        public ReactivePropertySlim<E> Object { get; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        public ReadOnlyReactivePropertySlim<V> PropertyValue
        {
            get
            {
                var splits = PropertyName.Value.Split('.');
                object obj = Object.Value;
                foreach (var split in splits.Except(new string[] { splits.Last() }))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(split).GetValue(obj)).Value;
                    obj = a;
                    if (obj is null)
                        return null;
                }
                if (obj is null)
                    return null;
                var ret = typeof(E).GetProperty(splits.Last()).GetValue(obj);
                return ret as ReadOnlyReactivePropertySlim<V>;
            }
            set
            {

            }
        }

        public override string Type => typeof(V) == typeof(bool) ? "ReadOnlyCheckBox" : (Options.Count() > 0) ? "ReadOnlyComboBox" : "ReadOnlyTextBox";
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

        public ReactivePropertySlim<E> Object { get; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        public ReactiveProperty<V> PropertyValue
        {
            get
            {
                var splits = PropertyName.Value.Split('.');
                object obj = Object.Value;
                foreach (var split in splits.Except(new string[] { splits.Last() }))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(split).GetValue(obj)).Value;
                    obj = a;
                    if (obj is null)
                        return null;
                }
                if (obj is null)
                    return null;
                return ((ReactiveProperty<V>)typeof(E).GetProperty(splits.Last()).GetValue(obj));
            }
            set
            {
                var splits = PropertyName.Value.Split('.');
                object obj = Object.Value;
                foreach (var split in splits.Except(new string[] { splits.Last() }))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(split).GetValue(obj)).Value;
                    obj = a;
                }
                if (obj is null)
                    return;
                typeof(E).GetProperty(splits.Last()).SetValue(obj, value);
            }
        }

        public override string Type => typeof(V) == typeof(bool) ? "CheckBox" : (Options.Count() > 0) ? "ComboBox" : "TextBox";
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

        public ReactivePropertySlim<E> Object { get; } = new ReactivePropertySlim<E>();
        public ReactiveCollection<V> Options { get; } = new ReactiveCollection<V>();
        public ReactivePropertySlim<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new ReactivePropertySlim<HorizontalAlignment>();
        public ReadOnlyReactiveProperty<V> PropertyValue
        {
            get
            {
                var splits = PropertyName.Value.Split('.');
                object obj = Object.Value;
                foreach (var split in splits.Except(new string[] { splits.Last() }))
                {
                    var a = ((IReactiveProperty)typeof(E).GetProperty(split).GetValue(obj)).Value;
                    obj = a;
                    if (obj is null)
                        return null;
                }
                if (obj is null)
                    return null;
                return ((ReadOnlyReactiveProperty<V>)typeof(E).GetProperty(splits.Last()).GetValue(obj));
            }
            set
            {
            }
        }

        public override string Type => typeof(V) == typeof(bool) ? "ReadOnlyCheckBox" : (Options.Count() > 0) ? "ReadOnlyComboBox" : "ReadOnlyTextBox";
    }
}