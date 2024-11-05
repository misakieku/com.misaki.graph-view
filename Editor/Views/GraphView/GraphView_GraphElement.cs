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

        private void RemoveConnection(SlotConnection connection)
        {
            Undo.RecordObject(_graphObject, $"Remove {connection.GetType().Name}");

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

            var inputPort = inputPortContainer.GetPort(inputSlotData.slotIndex, Direction.Input);
            var outputPort = outputPortContainer.GetPort(outputSlotData.slotIndex, Direction.Output);

            var edge = inputPort.ConnectTo(outputPort);
            AddElement(edge);
            _slotConnections.Add(edge, connection);
        }

        public void AddRelayNode(RelayNode relayNode)
        {
            Undo.RecordObject(_graphObject, $"Add {relayNode.GetType().Name}");

            _graphObject.AddNode(relayNode);
            AddRelayNodeView(relayNode);

            EditorUtility.SetDirty(_graphObject);
        }

        private void AddRelayNodeView(RelayNode relayNode)
        {
            var relayNodeView = new RelayNodeView(relayNode, _graphViewConfig.portColorManager);
            relayNodeView.SetPosition(relayNode.position);
            AddElement(relayNodeView);
        }
    }
}