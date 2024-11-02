
using System.Collections.ObjectModel;

namespace Misaki.GraphView
{
    public interface IGraphProcessor
    {
        public bool IsRunning { get; }
        
        public void UpdateComputeOrder();
        
        public void Execute(ReadOnlyCollection<BaseNode> nodes);
        
        public void Break();
    }
}