using System;

namespace Misaki.GraphView.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CustomInspectorAttribute : Attribute
    {
        public Type InspectorType { get; }
        
        public CustomInspectorAttribute(Type inspectorType)
        {
            InspectorType = inspectorType;
        }
    }
}