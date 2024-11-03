using Misaki.GraphView;
using Misaki.GraphView.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Sample.Editor
{
    public class SampleGraphEditor : EditorWindow
    {
        private const string Icon_Path = "Packages/com.misaki.graph-view/Sample/Icon/icons8-workflow-96.png";
        
        [SerializeField]
        private StyleSheet _styleSheet;
        
        private readonly PortColorManager _portColorManager = new ();
        private readonly ExposedPropertyTypeManager _exposedPropertyTypeManager = new ();
        private GraphObject _currentAsset;
        
        private GraphViewConfig _config;
        
        [MenuItem("Tools/SampleGraphEditor")]
        private static void Open()
        {
            var window = CreateWindow<SampleGraphEditor>(typeof(SceneView));
            window.titleContent = new GUIContent("Sample Graph Editor", EditorGUIUtility.IconContent(Icon_Path).image);
        }
        
        public static void Open(GraphObject asset)
        {
            var window = GetWindow<SampleGraphEditor>(typeof(SceneView));
            window.titleContent = new GUIContent(asset.name, EditorGUIUtility.IconContent(Icon_Path).image);
            window.Clear();
            window.LoadAsset(asset);
            window.DrawGraph();
            window.Focus();
        }

        public SampleGraphEditor()
        {
            _portColorManager.SetColor<uint>(Color.cyan);
            
            _exposedPropertyTypeManager.AddPropertyType<FloatProperty, float>();
        }
        
        private void Clear()
        {
            rootVisualElement.Clear();
        }
        
        private void LoadAsset(GraphObject asset)
        {
            Clear();

            _currentAsset = asset;
            _config = new()
            {
                direction = GraphDirection.Horizontal,
                miniMapConfig = new ()
                {
                    enable = false,
                },
                zoomConfig = new ()
                {
                    minScale = 0.25f,
                    maxScale = 2f,
                    scaleStep = 0.1f
                },
                graphObject = _currentAsset,
                serializedObject = new SerializedObject(_currentAsset),
                portColorManager = _portColorManager,
                exposedPropertyTypeManager = _exposedPropertyTypeManager
            };
        }
        
        private void OnEnable()
        {
            if (_currentAsset == null)
            {
                var label = new Label("No asset loaded")
                {
                    style =
                    {
                        flexGrow = 1,
                        unityTextAlign = TextAnchor.MiddleCenter,
                        fontSize = 20
                    }
                };
                rootVisualElement.Add(label);
            }
            else
            {
                LoadAsset(_currentAsset);
                DrawGraph();
            }
        }
        
        private void DrawGraph()
        {
            var graphContainer = new VisualElement
            {
                name = "GraphContainer",
                style =
                {
                    flexDirection = FlexDirection.Column
                }
            };
            graphContainer.StretchToParentSize();

            var graphView = new GraphView.Editor.GraphView(this, _config);
            graphView.styleSheets.Add(_styleSheet);
            graphView.UpdateViewTransform(_currentAsset.graphPosition, _currentAsset.graphScale);
            
            var toolbar = new GraphToolbarView(_currentAsset);
            toolbar.BlackboardButtonClicked += graphView.ToggleBlackboardViewVisibility;
            toolbar.InspectButtonClicked += graphView.ToggleInspectorViewVisibility;
            
            // We can not directly add the graph view to the graphContainer since the RectangleSelector is calculated base on the parent position, so we need to add it to a container first
            var graphViewContainer = new VisualElement
            {
                name = "GraphViewContainer",
                style =
                {
                    flexGrow = 1
                }
            };
            
            graphContainer.Add(toolbar);
            
            graphViewContainer.Add(graphView);
            graphContainer.Add(graphViewContainer);
            
            rootVisualElement.Add(graphContainer);

            // // If no asset is loaded, show a warning label
            // if (currentAsset == null)
            // {
            //     RenderNoAssetAlert();
            // }
        }
    }
}
