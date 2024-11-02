using System;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public interface IInspectable
    {
        public Action<IInspectable> OnItemSelected { get; set; }
        
        public string InspectorName { get; }
        public VisualElement CreateInspector();
    }
}