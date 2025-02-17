using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Misaki.GraphView.Editor
{
    public partial class GraphView
    {
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                var removedElements = graphViewChange.elementsToRemove;
                Undo.RecordObject(_graphObject, "Remove elements");

                for (var i = removedElements.Count - 1; i >= 0; i--)
                {
                    if (removedElements[i] is Node { userData: DataNode node })
                    {
                        RemoveNode(node);
                    }

                    if (removedElements[i] is StickyNote { userData: StickyNoteData stickyNote })
                    {
                        RemoveStickyNote(stickyNote);
                    }

                    if (removedElements[i] is Edge edge)
                    {
                        if (_slotConnections.Remove(edge, out var connection))
                        {
                            var inputNode = _graphObject.GetNode(connection.InputSlotData.nodeID);
                            var outputNode = _graphObject.GetNode(connection.OutputSlotData.nodeID);

                            if (inputNode is ISlotContainer inputSlotContainer && outputNode is ISlotContainer outputSlotContainer)
                            {
                                var inputSlot = inputSlotContainer.GetSlot(connection.InputSlotData.slotIndex, connection.InputSlotData.direction);
                                var outputSlot = outputSlotContainer.GetSlot(connection.OutputSlotData.slotIndex, connection.OutputSlotData.direction);

                                inputSlot.Unlink(outputSlot);
                            }

                            RemoveConnection(connection);
                        }
                    }
                }
            }

            if (graphViewChange.movedElements != null)
            {
                var movedElements = graphViewChange.movedElements;
                Undo.RecordObject(_graphObject, $"Move {movedElements.FirstOrDefault()?.GetType().Name}");

                foreach (var element in movedElements)
                {
                    if (element is IDataNodeView dataNodeView)
                    {
                        dataNodeView.GetDataNode().nodePosition = element.GetPosition();
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                var createdEdges = graphViewChange.edgesToCreate;
                Undo.RecordObject(_graphObject, $"Connect {createdEdges.FirstOrDefault()?.GetType().Name}");

                foreach (var edge in createdEdges)
                {
                    if (edge.input.userData is Slot inputSlot && edge.output.userData is Slot outputSlot)
                    {
                        outputSlot.Link(inputSlot, out var connection);
                        _slotConnections.Add(edge, connection);

                        AddConnection(connection);
                    }
                }
            }

            _graphObject.SetGraphTransform(viewTransform);

            _graphViewConfig.serializedObject.Update();
            EditorUtility.SetDirty(_graphObject);

            return graphViewChange;
        }
    }
}
