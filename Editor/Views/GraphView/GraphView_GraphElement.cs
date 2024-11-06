using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Misaki.GraphView.Editor
{
    public partial class GraphView
    {
        public void AddNode(ExecutableNode executableNode)
        {
            Undo.RecordObject(_graphObject, $"Add {executableNode.GetType().Name}");

            _graphObject.AddNode(executableNode);
            AddNodeView(executableNode);

            EditorUtility.SetDirty(_graphObject);
        }

        private void AddNodeView(DataNode node)
        {
            var nodeView = CreateNodeView(node);
            if (nodeView == null)
            {
                return;
            }

            nodeView.SetPosition(node.position);

            if (nodeView is IInspectable inspectable)
            {
                inspectable.OnItemSelected += ChangeInspectorView;
            }

            AddElement(nodeView);
            _nodeViewsMap.Add(node.Id, nodeView);
        }

        protected virtual Node CreateNodeView(DataNode node)
        {
            var types = TypeCache.GetTypesWithAttribute<CustomInspectorAttribute>();
            var type = types.FirstOrDefault(t =>
                t.GetCustomAttribute<CustomInspectorAttribute>().InspectorType == node.GetType());

            if (node is PropertyInput propertyInputNode)
            {
                return PropertyInputNodeView.Create(propertyInputNode, _graphViewConfig.portColorManager);
            }
            else if (node is ExecutableNode executableNode)
            {
                type ??= typeof(ExecutableNodeView);
                return Activator.CreateInstance(type, executableNode, _graphViewConfig.serializedObject, _graphViewConfig.portColorManager, _graphObject.Logger) as ExecutableNodeView;
            }
            else if (node is RelayNode relayNode)
            {
                return new RelayNodeView(relayNode, _graphViewConfig.portColorManager);
            }

            return null;
        }

        private void RemoveNode(DataNode dataNode)
        {
            Undo.RecordObject(_graphObject, $"Remove {dataNode.GetType().Name}");

            _graphObject.RemoveNode(dataNode);
            RemoveNodeView(dataNode);

            EditorUtility.SetDirty(_graphObject);
        }

        private void RemoveNodeView(DataNode dataNode)
        {
            if (_nodeViewsMap.Remove(dataNode.Id, out var nodeView))
            {
                RemoveElement(nodeView);

                if (nodeView is IInspectable inspectable)
                {
                    inspectable.OnItemSelected -= ChangeInspectorView;
                }
            }
        }

        public void AddStickyNote(StickyNoteData stickyNote)
        {
            Undo.RecordObject(_graphObject, $"Add {stickyNote.title}");

            _graphObject.AddStickyNote(stickyNote);
            AddStickyNoteView(stickyNote);

            EditorUtility.SetDirty(_graphObject);
        }

        private void AddStickyNoteView(StickyNoteData stickyNote)
        {
            var stickyNoteView = new StickyNoteView(stickyNote);
            stickyNoteView.SetPosition(stickyNote.position);
            AddElement(stickyNoteView);
        }

        public void RemoveStickyNote(StickyNoteData stickyNote)
        {
            Undo.RecordObject(_graphObject, $"Remove {stickyNote.title}");

            _graphObject.RemoveStickyNote(stickyNote);

            RemoveStickyNoteView(stickyNote);
        }

        private void RemoveStickyNoteView(StickyNoteData stickyNote)
        {
            // var stickyNoteView = GetStickyNoteView(stickyNote);
            // if (stickyNoteView != null)
            // {
            //     RemoveElement(stickyNoteView);
            // }
        }

        public void AddConnection(SlotConnection connection)
        {
            Undo.RecordObject(_graphObject, $"Add {connection.GetType().Name}");

            _graphObject.AddConnection(connection);

            EditorUtility.SetDirty(_graphObject);
        }

        private void RemoveConnection(SlotConnection connection, bool notify = true)
        {
            if (notify)
            {
                Undo.RecordObject(_graphObject, $"Remove {connection.GetType().Name}");
            }

            RemoveConnectionView(connection);
            _graphObject.RemoveConnection(connection);

            EditorUtility.SetDirty(_graphObject);
        }

        private void AddConnectionView(SlotConnection connection)
        {
            var inputSlotData = connection.InputSlotData;
            var outputSlotData = connection.OutputSlotData;

            if (!_nodeViewsMap.TryGetValue(inputSlotData.nodeID, out var inputNodeView) ||
                !_nodeViewsMap.TryGetValue(outputSlotData.nodeID, out var outputNodeView))
            {
                return;
            }

            if (inputNodeView is not IPortContainer inputPortContainer || outputNodeView is not IPortContainer outputPortContainer)
            {
                return;
            }

            var portA = inputPortContainer.GetPort(inputSlotData.slotIndex, (Direction)inputSlotData.direction);
            var portB = outputPortContainer.GetPort(outputSlotData.slotIndex, (Direction)outputSlotData.direction);

            var edge = portA.ConnectTo(portB);
            AddElement(edge);
            _slotConnections.Add(edge, connection);
        }

        private void RemoveConnectionView(SlotConnection connection)
        {
            var edge = _slotConnections.FirstOrDefault(x => x.Value == connection).Key;
            if (edge != null)
            {
                RemoveElement(edge);
                _slotConnections.Remove(edge);
            }
        }

        public void AddRelayNode(RelayNode relayNode, Edge edge)
        {
            Undo.RecordObject(_graphObject, $"Add {relayNode.GetType().Name}");

            _graphObject.AddNode(relayNode);
            AddRelayNodeView(relayNode, edge);

            var connection = _slotConnections[edge];
            RemoveConnection(connection, false);

            EditorUtility.SetDirty(_graphObject);
        }

        private void AddRelayNodeView(RelayNode relayNode, Edge edge)
        {
            var relayNodeView = new RelayNodeView(relayNode, _graphViewConfig.portColorManager);
            relayNodeView.SetPosition(relayNode.position);

            relayNodeView.Connect(edge,
                out var inputConnection, out var outputConnection,
                out var inputEdge, out var outputEdge);

            _graphObject.AddConnection(inputConnection);
            _graphObject.AddConnection(outputConnection);

            _slotConnections.Add(inputEdge, inputConnection);
            _slotConnections.Add(outputEdge, outputConnection);

            AddElement(relayNodeView);
            AddElement(inputEdge);
            AddElement(outputEdge);
        }
    }
}