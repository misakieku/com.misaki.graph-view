using System;
using System.Collections.Generic;

namespace Misaki.GraphView
{
    public class Logger : ILogger
    {
        private readonly List<string> _logs = new();

        public Action<DataNode, string, LogType> OnLog
        {
            get; set;
        }

        public void LogInfo(DataNode node, string message)
        {
#if UNITY_EDITOR || ENABLE_GRAPH_LOGGING
            _logs.Add($"Log Info from node {node.GetType().Name}: {message}");
            OnLog?.Invoke(node, message, LogType.Info);
#endif
        }

        public void LogWarning(DataNode node, string message)
        {
#if UNITY_EDITOR || ENABLE_GRAPH_LOGGING
            _logs.Add($"Log Warning from node {node.GetType().Name}: {message}");
            OnLog?.Invoke(node, message, LogType.Warning);
#endif
        }

        public void LogError(DataNode node, string message)
        {
#if UNITY_EDITOR || ENABLE_GRAPH_LOGGING
            _logs.Add($"Log Error from node {node.GetType().Name}: {message}");
            OnLog?.Invoke(node, message, LogType.Error);
#endif
        }

        public void ClearLogs()
        {
            _logs.Clear();
        }
    }
}