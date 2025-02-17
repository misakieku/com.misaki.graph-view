using System;

namespace Misaki.GraphView
{
    public interface IValueConverterManager
    {
        public void AddConverter<TSource, TTarget, TConverter>();
        public bool CanConvert<TSource, TTarget>();
        public bool CanConvert(Type sourceType, Type targetType);

        public bool TryConvert<TSource, TTarget>(TSource source, out TTarget target);
        public bool TryConvert(Type sourceType, Type targetType, object source, out object target);
    }
}