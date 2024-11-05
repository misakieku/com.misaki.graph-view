using System;

namespace Misaki.GraphView
{
    public interface ILogger
    {
        public Action<DataNode, string, LogType> OnLog
        {
            get; set;
        }

        public void LogInfo(DataNode node, string message);
        public void LogWarning(DataNode node, string message);
        public void LogError(DataNode node, string message);

        public void ClearLogs();
    }
}