using System;
using System.Collections.Generic;

namespace Misaki.GraphView
{
    public class ValueConverterManager : IValueConverterManager
    {
        private readonly Dictionary<(Type source, Type target), Type> _converters = new();

        public void AddConverter<TSource, TTarget, TConverter>()
        {
            _converters[(typeof(TSource), typeof(TTarget))] = typeof(TConverter);
        }

        public bool CanConvert<TSource, TTarget>()
        {
            return _converters.ContainsKey((typeof(TSource), typeof(TTarget))) || _converters.ContainsKey((typeof(TTarget), typeof(TSource)));
        }

        public bool CanConvert(Type sourceType, Type targetType)
        {
            return _converters.ContainsKey((sourceType, targetType)) || _converters.ContainsKey((targetType, sourceType));
        }

        public bool TryConvert<TSource, TTarget>(TSource source, out TTarget target)
        {
            if (_converters.TryGetValue((typeof(TSource), typeof(TTarget)), out var converterType))
            {
                var converter = Activator.CreateInstance(converterType) as IValueConverter;
                target = (TTarget)converter.ConvertTo(source);
                return true;
            }
            else if (_converters.TryGetValue((typeof(TTarget), typeof(TSource)), out var backConverterType))
            {
                var converter = Activator.CreateInstance(backConverterType) as IValueConverter;
                target = (TTarget)converter.ConvertBack(source);
                return true;
            }

            target = default;
            return false;
        }

        public bool TryConvert(Type sourceType, Type targetType, object source, out object target)
        {
            if (_converters.TryGetValue((sourceType, targetType), out var converterType))
            {
                var converter = Activator.CreateInstance(converterType) as IValueConverter;
                target = converter.ConvertTo(source);
                return true;
            }
            else if (_converters.TryGetValue((targetType, sourceType), out var backConverterType))
            {
                var converter = Activator.CreateInstance(backConverterType) as IValueConverter;
                target = converter.ConvertBack(source);
                return true;
            }

            target = default;
            return false;
        }
    }
}