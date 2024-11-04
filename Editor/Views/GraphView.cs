using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class GraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        private readonly Dictionary<string, Node> _nodeViewsMap = new();
        private readonly Dictionary<Edge, SlotConnection> _slotConnections = new();
        
        private readonly GraphBlackboardView _blackboardView;
        private readonly GraphInspectorView _graphInspectorView;
        
        private readonly GraphViewConfig _graphViewConfig;
        private GraphObject _graphObject => _graphViewConfig.graphObject;
        
        public EditorWindow EditorWindow { get; }
        public GraphViewConfig GraphViewConfig => _graphViewConfig;
        
        public GraphView(EditorWindow editorWindow, GraphViewConfig graphViewConfig)
        {
            EditorWindow = editorWindow;
            _graphViewConfig = graphViewConfig;

            var gridBackground = new GridBackground { name = "gridBackground" };
            Add(gridBackground);
            gridBackground.SendToBack();
            
            var minimapConfig = _graphViewConfig.miniMapConfig;
            if (minimapConfig is { enable: true })
            {
                var minimap = new MiniMap()
                {
                    anchored = true
                };
                minimap.SetPosition(minimapConfig.position);
                Add(minimap);
            }
            
            _blackboardView = new GraphBlackboardView(_graphObject, this, _graphViewConfig.serializedObject, _graphViewConfig.exposedPropertyTypeManager);
            _blackboardView.OnPropertySelected += ChangeInspectorView;

            foreach (var property in _graphObject.ExposedProperties)
            {
                _blackboardView.AddProperty(property);
            }
            
            _graphInspectorView = new GraphInspectorView();
            
            Add(_blackboardView);
            Add(_graphInspectorView);

            this.StretchToParentSize();

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            var zoomConfig = _graphViewConfig.zoomConfig;
            var zoomer = new ContentZoomer();
            if (zoomConfig !=null)
            {
                zoomer.minScale = zoomConfig.minScale;
                zoomer.maxScale = zoomConfig.maxScale;
                zoomer.scaleStep = zoomConfig.scaleStep;
            }
            this.AddManipulator(zoomer);

            InitializeAssetElements();

            graphViewChanged += OnGraphViewChanged;
            
            RegisterCallback<DragPerformEvent>(OnDragPerform);
            RegisterCallbackOnce<GeometryChangedEvent>(_ => _graphInspectorView?.DockToParent(layout, DockingPosition.Right, false));
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var mousePosition = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            
            if (selection.Count == 0)
            {
                evt.menu.InsertAction(1, "Create Sticky Note", e =>
                {
                    var stickyNote = new StickyNoteData
                    {
                        title = "Sticky Note",
                        contents = "Contents",
                        theme = StickyNoteTheme.Classic,
                        fontSize = StickyNoteFontSize.Medium,
                        position = new Rect(mousePosition, new Vector2(200, 200))
                    };
                    
                    AddStickyNote(stickyNote);
                }, DropdownMenuAction.AlwaysEnabled);
            }
        }

        private void InitializeAssetElements()
        {
            var searchProvider = ScriptableObject.CreateInstance<NodeSearchProvider>();
            searchProvider.SetOwner(this);

            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchProvider);
            };

            foreach (var node in _graphObject.Nodes)
            {
                AddNodeView(node);
            }

            foreach (var noteData in _graphObject.StickyNotes)
            {
                AddStickyNoteView(noteData);
            }
            
            foreach (var connection in _graphObject.Connections)
            {
                AddConnectionView(connection);
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                var removedElements = graphViewChange.elementsToRemove;
                Undo.RecordObject(_graphObject, "Remove elements");

                for (var i = removedElements.Count - 1; i >= 0; i--)
                {
                    if (removedElements[i] is Node { userData: ExecutableNode node })
                    {
                        RemoveNode(node);
                    }

                    if (removedElements[i] is StickyNote {userData : StickyNoteData stickyNote})
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

            // if (graphViewChange.movedElements != null)
            // {
            //     var movedElements = graphViewChange.movedElements;
            //     Undo.RecordObject(_graphObject, $"Move {movedElements.FirstOrDefault()?.GetType().Name}");
            //
            //     foreach (var element in graphViewChange.movedElements)
            //     {
            //         element.SetPosition(element.GetPosition());
            //     }
            // }

            if (graphViewChange.edgesToCreate != null)
            {
                var createdEdges = graphViewChange.edgesToCreate;
                Undo.RecordObject(_graphObject, $"Connect {createdEdges.FirstOrDefault()?.GetType().Name}");

                foreach (var edge in createdEdges)
                    if (edge.input.userData is Slot inputSlot && edge.output.userData is Slot outputSlot)
                    {
                        var connection = new SlotConnection(inputSlot.slotData, outputSlot.slotData);
                        _slotConnections.Add(edge, connection);

                        outputSlot.Link(inputSlot);
                        AddConnection(connection);
                    }
            }

            _graphObject.SetGraphTransform(viewTransform);
            
            _graphViewConfig.serializedObject.Update();
            EditorUtility.SetDirty(_graphObject);
            
            return graphViewChange;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            foreach (var port in ports)
            {
                if (startPort == port || startPort.node == port.node || startPort.direction == port.direction)
                {
                    continue;
                }
                
                if (startPort.portType != port.portType && 
                    _graphObject.ValueConverterManager != null &&
                    !_graphObject.ValueConverterManager.CanConvert(startPort.portType, port.portType))
                {
                    continue;
                }

                compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }

        private void OnDragPerform(DragPerformEvent evt)
        {
            var data = DragAndDrop.GetGenericData("DragSelection");
            if (data is List<ISelectable> selectables)
            {
                var propertyViews = selectables.OfType<BlackboardPropertyView>().ToArray();
                if (propertyViews.Length <= 0)
                {
                    return;
                }

                var position = (evt.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                foreach (var view in propertyViews)
                {
                    if (view.userData is not ExposedProperty property)
                    {
                        continue;
                    }
                    
                    var baseNode = new PropertyInput(property)
                    {
                        position = new Rect(position, Vector2.zero)
                    };
                    AddNode(baseNode);
                }
            }
        }

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

        private void RemoveNode(ExecutableNode executableNode)
        {
            Undo.RecordObject(_graphObject, $"Remove {executableNode.GetType().Name}");

            _graphObject.RemoveNode(executableNode);
            RemoveNodeView(executableNode);

            EditorUtility.SetDirty(_graphObject);
        }

        private void RemoveNodeView(ExecutableNode executableNode)
        {
            if (_nodeViewsMap.Remove(executableNode.Id, out var nodeView))
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
                
            var inputPort = inputPortContainer.InputPorts[inputSlotData.slotIndex];
            var outputPort = outputPortContainer.OutputPorts[outputSlotData.slotIndex];

            var edge = inputPort.ConnectTo(outputPort);
            AddElement(edge);
            _slotConnections.Add(edge, connection);
        }
        
        public void ToggleBlackboardViewVisibility()
        {
            if (_blackboardView == null)
            {
                return;
            }
            
            _blackboardView.style.visibility = _blackboardView.style.visibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }
        
        private void ChangeInspectorView(IInspectable inspectable)
        {
            if (_graphInspectorView == null)
            {
                return;
            }
            
            _graphInspectorView.OnNodeSelectionChanged(inspectable);
        }
        
        public void ToggleInspectorViewVisibility()
        {
            if (_graphInspectorView == null)
            {
                return;
            }
            
            _graphInspectorView.style.visibility = _graphInspectorView.style.visibility == Visibility.Hidden
                ? Visibility.Visible
                : Visibility.Hidden;
        }
    }
}