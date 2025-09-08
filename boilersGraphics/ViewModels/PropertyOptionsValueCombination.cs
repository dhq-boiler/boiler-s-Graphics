using boilersGraphics.Extensions;
using NLog;
using ObservableCollections;
using Prism.Mvvm;
using R3;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using ZLinq;

namespace boilersGraphics.ViewModels;

public abstract class PropertyOptionsValueCombination : BindableBase
{
    protected Logger logger = LogManager.GetCurrentClassLogger();

    public PropertyOptionsValueCombination(string name)
    {
        PropertyName.Value = name;
    }

    public BindableReactiveProperty<string> PropertyName { get; set; } = new();
    public BindableReactiveProperty<int> TabIndex { get; set; } = new();

    public virtual string Type => "NONE";

    public string ShowPropertiesAndFields()
    {
        var ret = $"<{GetType().Name}>{{";

        var properties = GetType().GetProperties(
            BindingFlags.Public
            | BindingFlags.Instance);

        foreach (var property in properties.AsValueEnumerable().Except(new[]
                 {
                     GetType().GetProperty("Parent"),
                     GetType().GetProperty("SelectedItems")
                 }))
            ret += $"{property.Name}={property.GetValue(this)},";

        var fields = GetType().GetFields(
            BindingFlags.Public
            | BindingFlags.Instance);

        foreach (var field in fields) ret += $"{field.Name}={field.GetValue(this)},"; 
        ret = ret.Remove(ret.Length - 1, 1);
        ret += "}";
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

    public PropertyOptionsValueCombinationClass(E obj, string name, HorizontalAlignment horizontalContentAlignment,
        V[] options)
        : this(obj, name, options)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public PropertyOptionsValueCombinationClass(E obj, string name, HorizontalAlignment horizontalContentAlignment)
        : this(obj, name)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public BindableReactiveProperty<E> Object { get; set; } = new();
    public NotifyCollectionChangedSynchronizedViewList<V> Options { get; set; } = new ObservableList<V>().ToWritableNotifyCollectionChanged();
    public BindableReactiveProperty<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new();

    public BindableReactiveProperty<V> PropertyValue
    {
        get
        {
            var splits = PropertyName.Value.Split('.').AsValueEnumerable();
            object obj = Object.Value;
            foreach (var split in splits.Except(new[] { splits.Last() }))
            {
                var property = obj.GetType().GetProperty(split);
                if (property != null)
                {
                    var propertyValue = property.GetValue(obj);
                    // より汎用的なアプローチでReactivePropertyの値を取得
                    if (propertyValue != null && propertyValue.GetType().IsGenericType)
                    {
                        var valueProperty = propertyValue.GetType().GetProperty("Value");
                        if (valueProperty != null)
                        {
                            obj = valueProperty.GetValue(propertyValue);
                        }
                        else
                        {
                            obj = propertyValue;
                        }
                    }
                    else
                    {
                        obj = propertyValue;
                    }
                }
                if (obj is null)
                    return null;
            }

            if (obj is null)
                return null;
            return (BindableReactiveProperty<V>)obj.GetType().GetProperty(splits.Last()).GetValue(obj);
        }
        set
        {
            var splits = PropertyName.Value.Split('.').AsValueEnumerable();
            object obj = Object.Value;
            foreach (var split in splits.Except(new[] { splits.Last() }))
            {
                var property = obj.GetType().GetProperty(split);
                if (property != null)
                {
                    var propertyValue = property.GetValue(obj);
                    // より汎用的なアプローチでReactivePropertyの値を取得
                    if (propertyValue != null && propertyValue.GetType().IsGenericType)
                    {
                        var valueProperty = propertyValue.GetType().GetProperty("Value");
                        if (valueProperty != null)
                        {
                            obj = valueProperty.GetValue(propertyValue);
                        }
                        else
                        {
                            obj = propertyValue;
                        }
                    }
                    else
                    {
                        obj = propertyValue;
                    }
                }
            }

            if (obj is null)
                return;
            obj.GetType().GetProperty(splits.Last()).SetValue(obj, value);
        }
    }

    public override string Type =>
        typeof(V) == typeof(bool) ? "CheckBox" : Options.AsValueEnumerable().Count() > 0 ? "ComboBox" : "TextBox";
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

    public PropertyOptionsValueCombinationReadOnlyClass(E obj, string name,
        HorizontalAlignment horizontalContentAlignment, V[] options)
        : this(obj, name, options)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public PropertyOptionsValueCombinationReadOnlyClass(E obj, string name,
        HorizontalAlignment horizontalContentAlignment)
        : this(obj, name)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public BindableReactiveProperty<E> Object { get; set; } = new();
    public NotifyCollectionChangedSynchronizedViewList<V> Options { get; set; } = new ObservableList<V>().ToWritableNotifyCollectionChanged();
    public BindableReactiveProperty<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new();

    public IReadOnlyBindableReactiveProperty<V> PropertyValue
    {
        get
        {
            var splits = PropertyName.Value.Split('.').AsValueEnumerable();
            object obj = Object.Value;
            foreach (var split in splits.Except(new[] { splits.Last() }))
            {
                var property = obj.GetType().GetProperty(split);
                if (property != null)
                {
                    var propertyValue = property.GetValue(obj);
                    // より汎用的なアプローチでReactivePropertyの値を取得
                    if (propertyValue != null && propertyValue.GetType().IsGenericType)
                    {
                        var valueProperty = propertyValue.GetType().GetProperty("Value");
                        if (valueProperty != null)
                        {
                            obj = valueProperty.GetValue(propertyValue);
                        }
                        else
                        {
                            obj = propertyValue;
                        }
                    }
                    else
                    {
                        obj = propertyValue;
                    }
                }
                if (obj is null)
                    return null;
            }

            if (obj is null)
                return null;
            return (IReadOnlyBindableReactiveProperty<V>)obj.GetType().GetProperty(splits.Last()).GetValue(obj);
        }
        set { }
    }

    public override string Type => typeof(V) == typeof(bool) ? "ReadOnlyCheckBox" :
        Options.AsValueEnumerable().Count() > 0 ? "ReadOnlyComboBox" : "ReadOnlyTextBox";
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

    public PropertyOptionsValueCombinationStruct(E obj, string name, HorizontalAlignment horizontalContentAlignment,
        V[] options)
        : this(obj, name, options)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public PropertyOptionsValueCombinationStruct(E obj, string name, HorizontalAlignment horizontalContentAlignment)
        : this(obj, name)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public BindableReactiveProperty<E> Object { get; } = new();
    public NotifyCollectionChangedSynchronizedViewList<V> Options { get; } = new ObservableList<V>().ToWritableNotifyCollectionChanged();
    public BindableReactiveProperty<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new();

    public BindableReactiveProperty<V> PropertyValue
    {
        get
        {
            var splits = PropertyName.Value.Split('.').AsValueEnumerable();
            object obj = Object.Value;
            foreach (var split in splits.Except(new[] { splits.Last() }))
            {
                var property = obj.GetType().GetProperty(split);
                if (property != null)
                {
                    var propertyValue = property.GetValue(obj);
                    // より汎用的なアプローチでReactivePropertyの値を取得
                    if (propertyValue != null && propertyValue.GetType().IsGenericType)
                    {
                        var valueProperty = propertyValue.GetType().GetProperty("Value");
                        if (valueProperty != null)
                        {
                            obj = valueProperty.GetValue(propertyValue);
                        }
                        else
                        {
                            obj = propertyValue;
                        }
                    }
                    else
                    {
                        obj = propertyValue;
                    }
                }
                if (obj is null)
                    return null;
            }

            if (obj is null)
                return null;
            return (BindableReactiveProperty<V>)obj.GetType().GetProperty(splits.Last()).GetValue(obj);
        }
        set
        {
            var splits = PropertyName.Value.Split('.').AsValueEnumerable();
            object obj = Object.Value;
            foreach (var split in splits.Except(new[] { splits.Last() }))
            {
                var property = obj.GetType().GetProperty(split);
                if (property != null)
                {
                    var propertyValue = property.GetValue(obj);
                    // より汎用的なアプローチでReactivePropertyの値を取得
                    if (propertyValue != null && propertyValue.GetType().IsGenericType)
                    {
                        var valueProperty = propertyValue.GetType().GetProperty("Value");
                        if (valueProperty != null)
                        {
                            obj = valueProperty.GetValue(propertyValue);
                        }
                        else
                        {
                            obj = propertyValue;
                        }
                    }
                    else
                    {
                        obj = propertyValue;
                    }
                }
            }

            if (obj is null)
                return;
            obj.GetType().GetProperty(splits.Last()).SetValue(obj, value);
        }
    }

    public override string Type =>
        typeof(V) == typeof(bool) ? "CheckBox" : Options.AsValueEnumerable().Count() > 0 ? "ComboBox" : "TextBox";
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

    public PropertyOptionsValueCombinationReadOnlyStruct(E obj, string name,
        HorizontalAlignment horizontalContentAlignment, V[] options)
        : this(obj, name, options)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public PropertyOptionsValueCombinationReadOnlyStruct(E obj, string name,
        HorizontalAlignment horizontalContentAlignment)
        : this(obj, name)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public BindableReactiveProperty<E> Object { get; } = new();
    public NotifyCollectionChangedSynchronizedViewList<V> Options { get; } = new ObservableList<V>().ToWritableNotifyCollectionChanged();
    public BindableReactiveProperty<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new();

    public IReadOnlyBindableReactiveProperty<V> PropertyValue
    {
        get
        {
            var splits = PropertyName.Value.Split('.').AsValueEnumerable();
            object obj = Object.Value;
            foreach (var split in splits.Except(new[] { splits.Last() }))
            {
                var property = obj.GetType().GetProperty(split);
                if (property != null)
                {
                    var propertyValue = property.GetValue(obj);
                    // より汎用的なアプローチでReactivePropertyの値を取得
                    if (propertyValue != null && propertyValue.GetType().IsGenericType)
                    {
                        var valueProperty = propertyValue.GetType().GetProperty("Value");
                        if (valueProperty != null)
                        {
                            obj = valueProperty.GetValue(propertyValue);
                        }
                        else
                        {
                            obj = propertyValue;
                        }
                    }
                    else
                    {
                        obj = propertyValue;
                    }
                }
                if (obj is null)
                    return null;
            }

            if (obj is null)
                return null;
            var ret = obj.GetType().GetProperty(splits.Last()).GetValue(obj);
            return ret as IReadOnlyBindableReactiveProperty<V>;
        }
        set { }
    }

    public override string Type => typeof(V) == typeof(bool) ? "ReadOnlyCheckBox" :
        Options.AsValueEnumerable().Count() > 0 ? "ReadOnlyComboBox" : "ReadOnlyTextBox";
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

    public PropertyOptionsValueCombinationStructRP(E obj, string name, HorizontalAlignment horizontalContentAlignment,
        V[] options)
        : this(obj, name, options)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public PropertyOptionsValueCombinationStructRP(E obj, string name, HorizontalAlignment horizontalContentAlignment)
        : this(obj, name)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public BindableReactiveProperty<E> Object { get; } = new();
    public NotifyCollectionChangedSynchronizedViewList<V> Options { get; } = new ObservableList<V>().ToWritableNotifyCollectionChanged();
    public BindableReactiveProperty<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new();

    public ReactiveProperty<V> PropertyValue
    {
        get
        {
            var splits = PropertyName.Value.Split('.').AsValueEnumerable();
            object obj = Object.Value;
            foreach (var split in splits.Except(new[] { splits.Last() }))
            {
                var property = obj.GetType().GetProperty(split);
                if (property != null)
                {
                    var propertyValue = property.GetValue(obj);
                    // より汎用的なアプローチでReactivePropertyの値を取得
                    if (propertyValue != null && propertyValue.GetType().IsGenericType)
                    {
                        var valueProperty = propertyValue.GetType().GetProperty("Value");
                        if (valueProperty != null)
                        {
                            obj = valueProperty.GetValue(propertyValue);
                        }
                        else
                        {
                            obj = propertyValue;
                        }
                    }
                    else
                    {
                        obj = propertyValue;
                    }
                }
                if (obj is null)
                    return null;
            }

            if (obj is null)
                return null;
            return (ReactiveProperty<V>)obj.GetType().GetProperty(splits.Last()).GetValue(obj);
        }
        set
        {
            var splits = PropertyName.Value.Split('.').AsValueEnumerable();
            object obj = Object.Value;
            foreach (var split in splits.Except(new[] { splits.Last() }))
            {
                var property = obj.GetType().GetProperty(split);
                if (property != null)
                {
                    var propertyValue = property.GetValue(obj);
                    // より汎用的なアプローチでReactivePropertyの値を取得
                    if (propertyValue != null && propertyValue.GetType().IsGenericType)
                    {
                        var valueProperty = propertyValue.GetType().GetProperty("Value");
                        if (valueProperty != null)
                        {
                            obj = valueProperty.GetValue(propertyValue);
                        }
                        else
                        {
                            obj = propertyValue;
                        }
                    }
                    else
                    {
                        obj = propertyValue;
                    }
                }
            }

            if (obj is null)
                return;
            obj.GetType().GetProperty(splits.Last()).SetValue(obj, value);
        }
    }

    public override string Type =>
        typeof(V) == typeof(bool) ? "CheckBox" : Options.AsValueEnumerable().Count() > 0 ? "ComboBox" : "TextBox";
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

    public PropertyOptionsValueCombinationReadOnlyStructRP(E obj, string name,
        HorizontalAlignment horizontalContentAlignment, V[] options)
        : this(obj, name, options)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public PropertyOptionsValueCombinationReadOnlyStructRP(E obj, string name,
        HorizontalAlignment horizontalContentAlignment)
        : this(obj, name)
    {
        HorizontalContentAlignment.Value = horizontalContentAlignment;
    }

    public BindableReactiveProperty<E> Object { get; } = new();
    public NotifyCollectionChangedSynchronizedViewList<V> Options { get; } = new ObservableList<V>().ToWritableNotifyCollectionChanged();
    public BindableReactiveProperty<HorizontalAlignment> HorizontalContentAlignment { get; set; } = new();

    public ReadOnlyReactiveProperty<V> PropertyValue
    {
        get
        {
            var splits = PropertyName.Value.Split('.').AsValueEnumerable();
            object obj = Object.Value;
            foreach (var split in splits.Except(new[] { splits.Last() }))
            {
                var property = obj.GetType().GetProperty(split);
                if (property != null)
                {
                    var propertyValue = property.GetValue(obj);
                    // より汎用的なアプローチでReactivePropertyの値を取得
                    if (propertyValue != null && propertyValue.GetType().IsGenericType)
                    {
                        var valueProperty = propertyValue.GetType().GetProperty("Value");
                        if (valueProperty != null)
                        {
                            obj = valueProperty.GetValue(propertyValue);
                        }
                        else
                        {
                            obj = propertyValue;
                        }
                    }
                    else
                    {
                        obj = propertyValue;
                    }
                }
                if (obj is null)
                    return null;
            }

            if (obj is null)
                return null;
            return (ReadOnlyReactiveProperty<V>)obj.GetType().GetProperty(splits.Last()).GetValue(obj);
        }
        set { }
    }

    public override string Type => typeof(V) == typeof(bool) ? "ReadOnlyCheckBox" :
        Options.AsValueEnumerable().Count() > 0 ? "ReadOnlyComboBox" : "ReadOnlyTextBox";
}