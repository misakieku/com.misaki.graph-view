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
            if (minimapConfig != null && minimapConfig.enable)
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
        }

        private void InitializeAssetElements()
        {
            var searchProvider = ScriptableObject.CreateInstance<NodeSearchProvider>();
            searchProvider.SetOwner(this);

            nodeCreationRequest = context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchProvider);
            };

            if (_graphObject.Nodes.Count > 0)
            {
                foreach (var node in _graphObject.Nodes) AddNodeView(node);

                foreach (var connection in _graphObject.Connections)
                    AddConnectionView(connection.InputSlotData, connection.OutputSlotData);
            }
            
            RegisterCallback<DragPerformEvent>(OnDragPerform);
            
            RegisterCallbackOnce<GeometryChangedEvent>(_ => _graphInspectorView?.DockToParent(layout, DockingPosition.Right, false));
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                var removedElements = graphViewChange.elementsToRemove;
                Undo.RecordObject(_graphObject, "Remove elements");

                for (var i = removedElements.Count - 1; i >= 0; i--)
                {
                    if (removedElements[i] is Node { userData: BaseNode node })
                    {
                        RemoveNode(node);
                    }

                    if (removedElements[i] is Edge edge)
                    {
                        if (_slotConnections.Remove(edge, out var connection))
                        {
                            var inputSlotData = connection.InputSlotData;
                            var outputSlotData = connection.OutputSlotData;
                            var inputSlot = _graphObject.GetNode(inputSlotData.nodeID)
                                .GetSlot(inputSlotData.slotIndex, inputSlotData.direction);
                            var outputSlot = _graphObject.GetNode(outputSlotData.nodeID)
                                .GetSlot(outputSlotData.slotIndex, outputSlotData.direction);

                            inputSlot.Unlink(outputSlot);
                            RemoveConnection(connection);
                        }
                    }
                }
            }

            if (graphViewChange.movedElements != null)
            {
                var movedElements = graphViewChange.movedElements;
                Undo.RecordObject(_graphObject, $"Move {movedElements.FirstOrDefault()?.GetType().Name}");

                foreach (var element in graphViewChange.movedElements)
                {
                    if (element is Node node)
                    {
                        node.SetPosition(node.GetPosition());
                    }
                }
            }

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

            _graphObject.SetTransform(viewTransform);
            
            EditorUtility.SetDirty(_graphObject);
            _graphViewConfig.serializedObject.ApplyModifiedProperties();
            _graphViewConfig.serializedObject.Update();
            
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
            if (data is List<ExposedProperty> properties)
            {
                var position = contentViewContainer.WorldToLocal(evt.mousePosition);
                foreach (var property in properties)
                {
                    var baseNode = new PropertyInputNode(property);
                    baseNode.position = new Rect(position, Vector2.zero);
                    AddNode(baseNode);
                }
            }
        }

        public void AddNode(BaseNode baseNode)
        {
            Undo.RecordObject(_graphObject, $"Add {baseNode.GetType().Name}");

            _graphObject.AddNode(baseNode);
            AddNodeView(baseNode);

            EditorUtility.SetDirty(_graphObject);
        }

        public virtual void AddNodeView(BaseNode baseNode)
        {
            Node nodeView;
            var types = TypeCache.GetTypesWithAttribute<CustomInspectorAttribute>();
            var type = types.FirstOrDefault(t =>
                t.GetCustomAttribute<CustomInspectorAttribute>().InspectorType == baseNode.GetType());

            if (baseNode is PropertyInputNode propertyInputNode)
            {
                // type ??= typeof(PropertyInputNodeView);
                // var slot = propertyInputNode.GetSlot(0, SlotDirection.Output);
                // nodeView = Activator.CreateInstance(type, propertyInputNode, _graphViewConfig.portColorManager) as PropertyInputNodeView;
                nodeView = PropertyInputNodeView.Create(propertyInputNode, _graphViewConfig.portColorManager);
            }
            else
            {
                type ??= typeof(EditorNodeView);
                nodeView = Activator.CreateInstance(type, baseNode, _graphViewConfig.serializedObject, _graphViewConfig.portColorManager) as EditorNodeView;
            }


            if (nodeView == null)
            {
                return;
            }

            nodeView.SetPosition(baseNode.position);

            if (nodeView is IInspectable inspectable)
            {
                inspectable.OnItemSelected += ChangeInspectorView;
            }

            AddElement(nodeView);
            _nodeViewsMap.Add(baseNode.Id, nodeView);
        }

        private void RemoveNode(BaseNode baseNode)
        {
            Undo.RecordObject(_graphObject, $"Remove {baseNode.GetType().Name}");

            _graphObject.RemoveNode(baseNode);
            RemoveNodeView(baseNode);

            EditorUtility.SetDirty(_graphObject);
        }

        private void RemoveNodeView(BaseNode baseNode)
        {
            if (_nodeViewsMap.Remove(baseNode.Id, out var nodeView))
            {
                RemoveElement(nodeView);

                if (nodeView is IInspectable inspectable)
                {
                    inspectable.OnItemSelected -= ChangeInspectorView;
                }
            }
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

        private void AddConnectionView(SlotData a, SlotData b)
        {
            if (_nodeViewsMap.TryGetValue(a.nodeID, out var inputNodeView) &&
                _nodeViewsMap.TryGetValue(b.nodeID, out var outputNodeView))
            {
                if (inputNodeView is not IPortContainer inputPortContainer || outputNodeView is not IPortContainer outputPortContainer)
                {
                    return;
                }
                
                var inputPort = inputPortContainer.InputPorts[a.slotIndex];
                var outputPort = outputPortContainer.OutputPorts[b.slotIndex];

                var edge = inputPort.ConnectTo(outputPort);
                AddElement(edge);
                _slotConnections.Add(edge, new SlotConnection(a, b));
            }
        }
    }
}