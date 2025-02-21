﻿using System.Collections.ObjectModel;

namespace Misaki.GraphView.Sample
{
    public class BackTraceGraphProcessor : IGraphProcessor
    {
        private bool _isRunning;

        public bool IsRunning => _isRunning;

        public void UpdateComputeOrder()
        {
        }

        public void Execute(ReadOnlyCollection<DataNode> nodes)
        {
            _isRunning = true;
            nodes.ClearAllExecuteFlag();

            foreach (var node in nodes)
            {
                if (!_isRunning)
                {
                    break;
                }

                if (node is Output outputNode)
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