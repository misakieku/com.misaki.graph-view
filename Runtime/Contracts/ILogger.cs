using System;
using UnityEngine;

namespace Misaki.GraphView
{
    public interface ILogger
    {
        public Action<BaseNode, string, LogType> OnLog { get; set; }
        
        public void LogInfo(BaseNode node, string message);
        public void LogWarning(BaseNode node, string message);
        public void LogError(BaseNode node, string message);
    }
}