using System;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public abstract class DataNode
    {
        [SerializeField]
        private GraphObject _graphObject;
        [SerializeField]
        private string _id = Guid.NewGuid().ToString();

        public Rect nodePosition;

        public GraphObject GraphObject => _graphObject;
        public string Id => _id;

        /// <summary>
        /// Initialize the node with the graph object, this method is called when the node is added to the graph.
        /// </summary>
        public virtual void Initialize(GraphObject graph)
        {
            _graphObject = graph;
        }

        /// <summary>
        /// Dispose the node, this method is called when editor window is closed or the node is removed from the graph.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Destroy the node, this method is called when the node is removed from the graph.
        /// </summary>
        public virtual void Destroy()
        {
            Dispose();
            _graphObject = null;
        }
    }
}