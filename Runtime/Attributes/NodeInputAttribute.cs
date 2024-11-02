using System;

namespace Misaki.GraphView
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeInputAttribute : System.Attribute
    {
        public string Name { get; }

        public NodeInputAttribute(string name = null)
        {
            Name = name;
        }
    }
}