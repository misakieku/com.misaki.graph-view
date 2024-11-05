using UnityEditor.Experimental.GraphView;

namespace Misaki.GraphView.Editor
{
    public interface IPortContainer
    {
        public Port GetPort(int index, Direction direction);
    }
}