using System.Collections.Generic;

namespace Misaki.GraphView
{
    public static class BaseNodeExtension
    {
        public static void ClearAllExecuteFlag(this IList<BaseNode> nodes)
        {
            foreach (var node in nodes)
            {
                node.ClearExecuteFlag();
            }
        }
    }
}