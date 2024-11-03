using System;
using UnityEngine;

namespace Misaki.GraphView
{
    public interface ILogger
    {
        public Action<SlotContainerNode, string, LogType> OnLog { get; set; }
        
        public void LogInfo(SlotContainerNode node, string message);
        public void LogWarning(SlotContainerNode node, string message);
        public void LogError(SlotContainerNode node, string message);
        
        public void ClearLogs();
    }
}