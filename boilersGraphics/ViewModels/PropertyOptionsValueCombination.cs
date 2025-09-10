using boilersGraphics.Extensions;
using NLog;
using ObservableCollections;
using Prism.Mvvm;
using R3;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using ZLinq;

namespace boilersGraphics.ViewModels;

public abstract class PropertyOptionsValueCombination : BindableBase
{
    protected Logger logger = LogManager.GetCurrentClassLogger();
    
    // Cache for reflection results to avoid repeated expensive lookups
    private static readonly ConcurrentDictionary<string, PropertyInfo[]> _propertyPathCache = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _typePropertiesCache = new();

    public PropertyOptionsValueCombination(string name)
    {
        PropertyName.Value = name;
    }

    public BindableReactiveProperty<string> PropertyName { get; set; } = new();
    public BindableReactiveProperty<int> TabIndex { get; set; } = new();

    public virtual string Type => "NONE";

    protected PropertyInfo[] GetCachedPropertyPath(string propertyPath, Type rootType)
    {
        var cacheKey = $"{rootType.FullName}::{propertyPath}";
        return _propertyPathCache.GetOrAdd(cacheKey, _ =>
        {
            var segments = propertyPath.Split('.');
            var properties = new PropertyInfo[segments.Length];
            var currentType = rootType;
            
            for (int i = 0; i < segments.Length; i++)
            {
                var property = GetCachedProperty(currentType, segments[i]);
                if (property == null) return null;
                
                properties[i] = property;
                currentType = property.PropertyType;
            }
            
            return properties;
        });
    }

    protected PropertyInfo GetCachedProperty(Type type, string propertyName)
    {
        var properties = _typePropertiesCache.GetOrAdd(type, t => 
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        
        return Array.Find(properties, p => p.Name == propertyName);
    }

    protected object TraversePropertyPath(object root, PropertyInfo[] propertyPath, int depth = -1)
    {
        if (root == null || propertyPath == null) return null;
        
        var currentObject = root;
        var maxDepth = depth < 0 ? propertyPath.Length : Math.Min(depth, propertyPath.Length);
        
        for (int i = 0; i < maxDepth; i++)
        {
            var property = propertyPath[i];
            var propertyValue = property.GetValue(currentObject);
            
            // Handle ReactiveProperty wrapper types efficiently
            if (propertyValue != null && propertyValue.GetType().IsGenericType)
            {
                var valueProperty = GetCachedProperty(propertyValue.GetType(), "Value");
                if (valueProperty != null)
                {
                    currentObject = valueProperty.GetValue(propertyValue);
                }
                else
                {
                    currentObject = propertyValue;
                }
            }
            else
            {
                currentObject = propertyValue;
            }
            
            if (currentObject == null) return null;
        }
        
        return currentObject;
    }

    public string ShowPropertiesAndFields()
    {
        var ret = $"<{GetType().Name}>{{";

        // Use cached properties to avoid repeated reflection calls
        var properties = _typePropertiesCache.GetOrAdd(GetType(), t => 
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        foreach (var property in properties)
        {
            // Skip expensive properties
            if (property.Name == "Parent" || property.Name == "SelectedItems")
                continue;
                
            try
            {
                ret += $"{property.Name}={property.GetValue(this)},";
            }
            catch
            {
                ret += $"{property.Name}=<error>,";
            }
        }

        var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields) 
        {
            try
            {
                ret += $"{field.Name}={field.GetValue(this)},";
            }
            catch
            {
                ret += $"{field.Name}=<error>,";
            }
        }
        
        if (ret.EndsWith(","))
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
    private PropertyInfo[] _cachedPropertyPath;
    private volatile bool _cacheInitialized = false;
    private readonly object _cacheLock = new object();

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

    private void EnsureCacheInitialized()
    {
        if (!_cacheInitialized)
        {
            lock (_cacheLock)
            {
                if (!_cacheInitialized)
                {
                    _cachedPropertyPath = GetCachedPropertyPath(PropertyName.Value, typeof(E));
                    _cacheInitialized = true;
                }
            }
        }
    }

    public BindableReactiveProperty<V> PropertyValue
    {
        get
        {
            EnsureCacheInitialized();
            
            if (_cachedPropertyPath == null || Object.Value == null)
                return null;

            // Navigate to the parent object (all but last property)
            var parentObject = TraversePropertyPath(Object.Value, _cachedPropertyPath, _cachedPropertyPath.Length - 1);
            if (parentObject == null)
                return null;

            // Get the final property
            var finalProperty = _cachedPropertyPath[_cachedPropertyPath.Length - 1];
            return (BindableReactiveProperty<V>)finalProperty.GetValue(parentObject);
        }
        set
        {
            EnsureCacheInitialized();
            
            if (_cachedPropertyPath == null || Object.Value == null)
                return;

            // Navigate to the parent object (all but last property)
            var parentObject = TraversePropertyPath(Object.Value, _cachedPropertyPath, _cachedPropertyPath.Length - 1);
            if (parentObject == null)
                return;

            // Set the final property
            var finalProperty = _cachedPropertyPath[_cachedPropertyPath.Length - 1];
            finalProperty.SetValue(parentObject, value);
        }
    }

    public override string Type =>
        typeof(V) == typeof(bool) ? "CheckBox" : Options.Count > 0 ? "ComboBox" : "TextBox";
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
            var ret = obj.GetType().GetProperty(splits.Last()).GetValue(obj);
            return ret as IReadOnlyBindableReactiveProperty<V>;
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