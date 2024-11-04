using System;
using UnityEngine;

namespace Misaki.GraphView
{
    public interface ILogger
    {
        public Action<ExecutableNode, string, LogType> OnLog { get; set; }
        
        public void LogInfo(ExecutableNode node, string message);
        public void LogWarning(ExecutableNode node, string message);
        public void LogError(ExecutableNode node, string message);
        
        public void ClearLogs();
    }
}