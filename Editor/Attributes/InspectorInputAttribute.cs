using System;

namespace Misaki.GraphView.Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorInputAttribute : Attribute
    {
        public string Name { get; }

        public InspectorInputAttribute(string name = null)
        {
            Name = name;
        }
    }
}