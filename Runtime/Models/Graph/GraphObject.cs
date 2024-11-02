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
        private List<BaseNode> _nodes = new();
        [SerializeField]
        private List<SlotConnection> _connections = new();
        [SerializeReference]
        private List<ExposedProperty> _exposedProperties = new();
        
        private readonly Dictionary<string, BaseNode> _nodeMap = new();

        public ReadOnlyCollection<BaseNode> Nodes => _nodes.AsReadOnly();
        public ReadOnlyCollection<SlotConnection> Connections => _connections.AsReadOnly();
        public ReadOnlyCollection<ExposedProperty> ExposedProperties => _exposedProperties.AsReadOnly();

        public Vector3 graphPosition;
        public Vector3 graphScale = Vector3.one;
        
        public virtual IValueConverterManager ValueConverterManager { get; } = null;

        private void OnEnable()
        {
            _nodeMap.Clear();
            foreach (var node in _nodes)
            {
                TryAddNodeToMap(node);
            }
        }

        public void AddNode(BaseNode baseNode)
        {
            _nodes.Add(baseNode);
            TryAddNodeToMap(baseNode);
            baseNode.Initialize(this);
        }

        public void RemoveNode(BaseNode baseNode)
        {
            _nodes.Remove(baseNode);
            RemoveNodeFromMap(baseNode);
            baseNode.UnLoad();
            baseNode.UnlinkAllSlots();
        }

        public bool TryAddNodeToMap(BaseNode baseNode)
        {
            return _nodeMap.TryAdd(baseNode.Id, baseNode);
        }

        public void RemoveNodeFromMap(BaseNode baseNode)
        {
            _nodeMap.Remove(baseNode.Id);
        }

        public BaseNode GetNode(string id)
        {
            return _nodeMap.GetValueOrDefault(id);
        }

        public bool TryGetNode(string id, out BaseNode baseNode)
        {
            return _nodeMap.TryGetValue(id, out baseNode);
        }

        public void AddConnection(SlotConnection connection)
        {
            _connections.Add(connection);
        }

        public void RemoveConnection(SlotConnection connection)
        {
            _connections.Remove(connection);
        }

        public void RemoveAllConnectionsForSlot(Slot slot)
        {
            _connections.RemoveAll(connection =>
                connection.InputSlotData == slot.slotData || connection.OutputSlotData == slot.slotData);
        }

        public SlotConnection TryGetConnection(Slot input, Slot output)
        {
            return _connections.FirstOrDefault(connection =>
                connection.InputSlotData == input.slotData && connection.OutputSlotData == output.slotData);
        }
        
        public void AddExposedProperty(ExposedProperty property)
        {
            _exposedProperties.Add(property);
        }
        
        public void RemoveExposedProperty(ExposedProperty property)
        {
            _exposedProperties.Remove(property);
        }
        
        public void SetTransform(ITransform transform)
        {
            graphPosition = transform.position;
            graphScale = transform.scale;
        }

        public abstract void Execute();
    }
}