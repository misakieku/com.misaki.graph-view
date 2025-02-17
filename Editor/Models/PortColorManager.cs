using System;
using System.Collections.Generic;
using UnityEngine;

namespace Misaki.GraphView.Editor
{
    public class PortColorManager : IPortColorManager
    {
        private readonly Dictionary<Type, Color> _colors = new();

        public void SetColor<T>(Color color)
        {
            _colors[typeof(T)] = color;
        }

        public bool TryGetColor(Type valueType, out Color color)
        {
            if (_colors.TryGetValue(valueType, out color))
            {
                return true;
            }

            if (valueType.BaseType != null)
            {
                return _colors.TryGetValue(valueType.BaseType, out color);
            }

            color = default;
            return false;
        }
    }
}