using System;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public abstract class DataNode
    {
        [SerializeField]
        protected GraphObject graphObject;
        [SerializeField]
        protected string id = Guid.NewGuid().ToString();

        public Rect position;

        public GraphObject GraphObject => graphObject;
        public string Id => id;

        /// <summary>
        /// Initialize the node with the graph object, this method is called when the node is added to the graph.
        /// </summary>
        public virtual void Initialize(GraphObject graph)
        {
            graphObject = graph;
        }

        /// <summary>
        /// Dispose the node, this method is called when the node is removed from the graph.
        /// </summary>
        public virtual void Dispose()
        {
            graphObject = null;
        }
    }
}