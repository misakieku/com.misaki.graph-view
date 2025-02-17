using System;

namespace Misaki.GraphView.Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public class InspectorInputAttribute : Attribute
    {
        public string Name
        {
            get;
        }
        public string ConnectionBinding
        {
            get;
        }

        public InspectorInputAttribute(string name = null, string connectionBinding = null)
        {
            Name = name;
            ConnectionBinding = connectionBinding;
        }
    }
}