using System;

namespace Misaki.GraphView
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class NodeInfoAttribute : System.Attribute
    {
        public string Name { get; }
        public string Category { get; }

        public NodeInfoAttribute(string name, string category)
        {
            Name = name;
            Category = category;
        }
    }
}