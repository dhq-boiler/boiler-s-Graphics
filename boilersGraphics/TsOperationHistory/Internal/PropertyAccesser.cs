using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TsOperationHistory.Internal
{
    internal interface IAccessor
    {
        object GetValue(object target);

        object GetValue(object target, int index);

        object GetValue();

        void SetValue(object target, object value);

        void SetValue(object target, int index, object value);

        void SetValue(object value);

        bool HasGetter { get; }
        bool HasSetter { get; }

        Type PropertyType { get; }
    }

    internal interface IMultiLayerAccessor : IAccessor
    {
        List<IAccessor> AccessorChain { get; }
    }

    internal sealed class MultiPropertyAccessor : IMultiLayerAccessor
    {
        public List<IAccessor> AccessorChain { get; } = new List<IAccessor>();

        public bool HasGetter => AccessorChain.All(x => x.HasGetter);

        public bool HasSetter => AccessorChain.All(x => x.HasSetter);

        public Type PropertyType => AccessorChain.Last().PropertyType;

        public MultiPropertyAccessor(IEnumerable<IAccessor> accessors)
        {
            AccessorChain.AddRange(accessors);
        }

        public object GetValue(object target)
        {
            object obj = target;
            foreach (var chain in AccessorChain)
            {
                obj = chain.GetValue(obj);
            }
            return obj;
        }

        public object GetValue(object target, int index)
        {
            object obj = target;
            foreach (var chain in AccessorChain)
            {
                if (chain != AccessorChain.Last())
                {
                    obj = chain.GetValue(obj);
                }
                else
                {
                    obj = chain.GetValue(obj, index);
                }
            }
            return obj;
        }

        public object GetValue()
        {
            return AccessorChain.First().GetValue();
        }

        public void SetValue(object target, object value)
        {
            object obj = target;
            IAccessor accessor = null;
            foreach (var chain in AccessorChain)
            {
                var temp = chain.GetValue(obj);
                if (chain != AccessorChain.Last())
                {
                    obj = temp;
                }
                accessor = chain;
            }
            accessor.SetValue(obj, value);
        }

        public void SetValue(object target, int index, object value)
        {
            object obj = target;
            IAccessor accessor = null;
            foreach (var chain in AccessorChain)
            {
                if (chain != AccessorChain.Last())
                {
                    obj = chain.GetValue(obj);
                }
                accessor = chain;
            }
            accessor.SetValue(obj, index, value);
        }

        public void SetValue(object value)
        {
            AccessorChain.First().SetValue(value);
        }
    }

    internal sealed class PropertyAccessor<TTarget, TProperty> : IAccessor
    {
        private readonly Func<TTarget, TProperty> _getter;
        private readonly Action<TTarget, TProperty> _setter;

        public PropertyAccessor(Func<TTarget, TProperty> getter, Action<TTarget, TProperty> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public object GetValue(object target)
        {
            if (_getter != null)
                return _getter((TTarget)target);

            return default;
        }

        public object GetValue(object target, int index)
        {
            throw new NotSupportedException();
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public void SetValue(object target, object value)
        {
            _setter?.Invoke((TTarget)target, (TProperty)value);
        }

        public void SetValue(object target, int index, object value)
        {
            throw new NotSupportedException();
        }

        public void SetValue(object value)
        {
            throw new NotSupportedException();
        }

        public bool HasGetter => (_getter != null);
        public bool HasSetter => (_setter != null);

        public Type PropertyType { get; } = typeof(TProperty);
    }

    internal sealed class IndexerAccessor<TTarget, TProperty> : IAccessor
    {
        private readonly Func<TTarget, int, TProperty> _getter;
        private readonly Action<TTarget, int, TProperty> _setter;

        public IndexerAccessor(Func<TTarget, int, TProperty> getter, Action<TTarget, int, TProperty> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public object GetValue(object target)
        {
            throw new NotSupportedException();
        }

        public object GetValue(object target, int index)
        {
            if (_getter != null)
                return _getter((TTarget)target, index);

            return default;
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public void SetValue(object target, object value)
        {
            throw new NotSupportedException();
        }

        public void SetValue(object target, int index, object value)
        {
            _setter?.Invoke((TTarget)target, index, (TProperty)value);
        }

        public void SetValue(object value)
        {
            throw new NotSupportedException();
        }

        public bool HasGetter => (_getter != null);
        public bool HasSetter => (_setter != null);

        public Type PropertyType { get; } = typeof(TProperty);
    }


    internal sealed class StructAccessor : IAccessor
    {
        private readonly PropertyInfo _propertyInfo;
        public StructAccessor(PropertyInfo propertyInfo , bool publicOnly)
        {
            _propertyInfo = propertyInfo;
            HasGetter = propertyInfo.GetGetMethod(publicOnly is false) != null;
            HasSetter = propertyInfo.GetSetMethod(publicOnly is false) != null;
            PropertyType = _propertyInfo.PropertyType;
        }

        public object GetValue(object target)
        {
            return _propertyInfo.GetValue(target);
        }

        public void SetValue(object target, object value)
        {
            _propertyInfo.SetValue(target,value);
        }

        public object GetValue(object target, int index)
        {
            throw new NotSupportedException();
        }

        public object GetValue()
        {
            throw new NotSupportedException();
        }

        public void SetValue(object target, int index, object value)
        {
            throw new NotSupportedException();
        }

        public void SetValue(object value)
        {
            throw new NotSupportedException();
        }

        public bool HasGetter { get; }

        public bool HasSetter { get; }

        public Type PropertyType { get; }
    }

    internal sealed class StaticPropertyAccessor<TTarget, TProperty> : IAccessor
    {
        private readonly Func<TProperty> _getter;
        private readonly Action<TProperty> _setter;

        public StaticPropertyAccessor(Func<TProperty> getter, Action<TProperty> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public object GetValue(object target)
        {
            throw new NotSupportedException();
        }

        public object GetValue(object target, int index)
        {
            throw new NotSupportedException();
        }

        public object GetValue()
        {
            return _getter();
        }

        public void SetValue(object target, object value)
        {
            throw new NotSupportedException();
        }

        public void SetValue(object target, int index, object value)
        {
            throw new NotSupportedException();
        }

        public void SetValue(object value)
        {
            _setter((TProperty)value);
        }

        public bool HasGetter => (_getter != null);
        public bool HasSetter => (_setter != null);

        public Type PropertyType { get; } = typeof(TProperty);
    }
}
