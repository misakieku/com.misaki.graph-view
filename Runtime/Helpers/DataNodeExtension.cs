using System.Collections.Generic;

namespace Misaki.GraphView
{
    public static class DataNodeExtension
    {
        public static void ClearAllExecuteFlag(this IList<DataNode> nodes)
        {
            foreach (var node in nodes)
            {
                if (node is IExecutable executable)
                {
                    executable.ClearExecutionFlag();
                }
            }
        }
    }
}