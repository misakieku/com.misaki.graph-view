using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView
{
    public abstract class GraphObject : ScriptableObject
    {
        [SerializeReference]
        private List<DataNode> _nodes = new();
        [SerializeField]
        private List<StickyNoteData> _stickyNotes = new();
        [SerializeField]
        private List<SlotConnection> _connections = new();
        [SerializeReference]
        private List<ExposedProperty> _exposedProperties = new();
        [SerializeField]
        private List<NodeGroupData> nodeGroupDatas = new();

        private readonly Dictionary<string, DataNode> _nodeMap = new();

        public ReadOnlyCollection<DataNode> Nodes => _nodes.AsReadOnly();
        public ReadOnlyCollection<StickyNoteData> StickyNotes => _stickyNotes.AsReadOnly();
        public ReadOnlyCollection<SlotConnection> Connections => _connections.AsReadOnly();
        public ReadOnlyCollection<ExposedProperty> ExposedProperties => _exposedProperties.AsReadOnly();
        public ReadOnlyCollection<NodeGroupData> NodeGroupDatas => nodeGroupDatas.AsReadOnly();

        public Vector3 graphPosition;
        public Vector3 graphScale = Vector3.one;

        public virtual IGraphProcessor GraphProcessor { get; } = null;
        public virtual IValueConverterManager ValueConverterManager { get; } = null;
        public virtual ILogger Logger { get; } = null;

        private void OnEnable()
        {
            _nodeMap.Clear();
            foreach (var node in _nodes)
            {
                TryAddNodeToMap(node);
            }
        }

        public void AddNode(DataNode node)
        {
            _nodes.Add(node);
            TryAddNodeToMap(node);
            node.Initialize(this);
        }

        public void RemoveNode(DataNode node)
        {
            _nodes.Remove(node);
            RemoveNodeFromMap(node);

            if (node is ISlotContainer slotContainer)
            {
                slotContainer.UnlinkAllSlots();
            }

            node.Dispose();
        }

        public bool TryAddNodeToMap(DataNode node)
        {
            return _nodeMap.TryAdd(node.Id, node);
        }

        public void RemoveNodeFromMap(DataNode node)
        {
            _nodeMap.Remove(node.Id);
        }

        public DataNode GetNode(string id)
        {
            return _nodeMap.GetValueOrDefault(id);
        }

        public bool TryGetNode(string id, out DataNode node)
        {
            return _nodeMap.TryGetValue(id, out node);
        }

        public void AddStickyNote(StickyNoteData stickyNote)
        {
            _stickyNotes.Add(stickyNote);
        }

        public void RemoveStickyNote(StickyNoteData stickyNote)
        {
            _stickyNotes.Remove(stickyNote);
        }

        public void AddConnection(SlotConnection connection)
        {
            _connections.Add(connection);
        }

        public void AddConnections(IEnumerable<SlotConnection> connections)
        {
            foreach (var connection in connections)
            {
                AddConnection(connection);
            }
        }

        public void RemoveConnection(SlotConnection connection)
        {
            _connections.Remove(connection);
        }

        public void RemoveAllConnectionsForSlot(ISlot slot)
        {
            _connections.RemoveAll(connection =>
                connection.InputSlotData == slot.SlotData || connection.OutputSlotData == slot.SlotData);
        }

        public SlotConnection TryGetConnection(ISlot input, ISlot output)
        {
            return _connections.FirstOrDefault(connection =>
                connection.InputSlotData == input.SlotData && connection.OutputSlotData == output.SlotData);
        }

        public void AddExposedProperty(ExposedProperty property)
        {
            _exposedProperties.Add(property);
        }

        public void RemoveExposedProperty(ExposedProperty property)
        {
            _exposedProperties.Remove(property);
        }

        public void SetGraphTransform(ITransform transform)
        {
            graphPosition = transform.position;
            graphScale = transform.scale;
        }

        public virtual void Execute()
        {
            if (GraphProcessor == null)
            {
                throw new ArgumentNullException(nameof(GraphProcessor), "GraphProcessor is null.");
            }

            GraphProcessor.UpdateComputeOrder();
            GraphProcessor.Execute(Nodes);
        }
    }
}