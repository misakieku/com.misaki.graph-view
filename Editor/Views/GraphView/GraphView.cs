using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public partial class GraphView : UnityEditor.Experimental.GraphView.GraphView
    {
        private readonly Dictionary<string, Node> _nodeViewsMap = new();
        private readonly Dictionary<Edge, SlotConnection> _slotConnections = new();

        private readonly GraphBlackboardView _blackboardView;
        private readonly GraphInspectorView _graphInspectorView;

        private readonly GraphViewConfig _graphViewConfig;
        private GraphObject _graphObject => _graphViewConfig.graphObject;

        public EditorWindow EditorWindow
        {
            get;
        }
        public GraphViewConfig GraphViewConfig => _graphViewConfig;

        public GraphView(EditorWindow editorWindow, GraphViewConfig graphViewConfig)
        {
            EditorWindow = editorWindow;
            _graphViewConfig = graphViewConfig;

            var gridBackground = new GridBackground { name = "gridBackground" };
            Add(gridBackground);
            gridBackground.SendToBack();

            var miniMapConfig = _graphViewConfig.miniMapConfig;
            if (miniMapConfig is { enable: true })
            {
                var miniMap = new MiniMap()
                {
                    anchored = true
                };
                miniMap.SetPosition(miniMapConfig.position);
                Add(miniMap);
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
            if (zoomConfig != null)
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
            if (data is List<ISelectable> selectable)
            {
                var propertyViews = selectable.OfType<BlackboardPropertyView>().ToArray();
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