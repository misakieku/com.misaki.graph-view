using System.Collections.ObjectModel;
using UnityEngine;

namespace Misaki.GraphView.Sample
{
    public class BackTraceGraphProcessor : IGraphProcessor
    {
        private bool _isRunning;
        
        public bool IsRunning => _isRunning;
        
        public void UpdateComputeOrder()
        {
        }

        public void Execute(ReadOnlyCollection<BaseNode> nodes)
        {
            _isRunning = true;
            nodes.ClearAllExecuteFlag();
            
            foreach (var node in nodes)
            {
                if (node is OutputNode outputNode)
                {
                    outputNode.Execute();
                }
            }
        }

        public void Break()
        {
            _isRunning = false;
        }
    }
}