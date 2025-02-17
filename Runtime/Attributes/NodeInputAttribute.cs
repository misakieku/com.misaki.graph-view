using System;

namespace Misaki.GraphView
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeInputAttribute : Attribute
    {
        public string Name
        {
            get;
        }

        public object DefaultValue
        {
            get;
        }

        public NodeInputAttribute(string name = null, object defaultValue = null)
        {
            Name = name;
            DefaultValue = defaultValue;
        }
    }
}