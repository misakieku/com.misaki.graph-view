using System;

namespace Misaki.GraphView
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class NodeOutputAttribute : System.Attribute
    {
        public string Name { get; }

        public NodeOutputAttribute(string name = null)
        {
            Name = name;
        }
    }
}