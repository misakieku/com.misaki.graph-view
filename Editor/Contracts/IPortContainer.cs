using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

namespace Misaki.GraphView.Editor
{
    public interface IPortContainer
    {
        public List<Port>  InputPorts { get; }
        public List<Port>  OutputPorts { get; }
    }
}