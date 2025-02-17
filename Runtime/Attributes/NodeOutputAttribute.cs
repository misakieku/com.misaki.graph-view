using System;

namespace Misaki.GraphView
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeOutputAttribute : Attribute
    {
        public string Name
        {
            get;
        }

        public object DefaultValue
        {
            get;
        }

        public NodeOutputAttribute(string name = null, object defaultValue = null)
        {
            Name = name;
            DefaultValue = defaultValue;
        }
    }
}