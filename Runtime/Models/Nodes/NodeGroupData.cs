using System;
using System.Collections.Generic;

namespace Misaki.GraphView
{
    [Serializable]
    public struct NodeGroupData
    {
        public string id;
        public string name;
        public List<string> nodeIds;

        public NodeGroupData(string groupName)
        {
            id = Guid.NewGuid().ToString();
            name = groupName;
            nodeIds = new List<string>();
        }
    }
}