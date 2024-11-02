using System;
using System.Collections.Generic;

namespace Misaki.GraphView.Sample
{
    public class Logger : ILogger
    {
        private readonly List<string> _logs = new ();
        
        public Action<BaseNode, string, LogType> OnLog { get; set; }
        
        public void LogInfo(BaseNode node, string message)
        {
            _logs.Add($"Log Info from node {node.GetType().Name}: {message}");
            OnLog?.Invoke(node, message, LogType.Info);
        }

        public void LogWarning(BaseNode node, string message)
        {
            _logs.Add($"Log Warning from node {node.GetType().Name}: {message}");
            OnLog?.Invoke(node, message, LogType.Warning);
        }

        public void LogError(BaseNode node, string message)
        {
            _logs.Add($"Log Error from node {node.GetType().Name}: {message}");
            OnLog?.Invoke(node, message, LogType.Error);
        }
    }
}