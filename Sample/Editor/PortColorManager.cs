using System;
using System.Collections.Generic;
using Misaki.GraphView.Editor;
using UnityEngine;

namespace Misaki.GraphView.Sample.Editor
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
            return _colors.TryGetValue(valueType, out color);
        }
    }
}