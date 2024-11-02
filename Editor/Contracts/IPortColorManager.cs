using System;
using UnityEngine;

namespace Misaki.GraphView.Editor
{
    public interface IPortColorManager
    {
        public void SetColor<T>(Color color);
        public bool TryGetColor(Type valueType, out Color color);
    }
}