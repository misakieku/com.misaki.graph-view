using UnityEditor;
using UnityEngine;

namespace Misaki.GraphView.Editor
{
    public class BlackboardConfig
    {
        public bool enable;
    }
    
    public class MiniMapConfig
    {
        public bool enable = true;
        public Rect position = new(10, 20, 200, 200);
    }
    
    public class ZoomConfig
    {
        public float minScale = 0.1f;
        public float maxScale = 2f;
        public float scaleStep = 0.1f;
    }
    
    public class InspectorConfig
    {
        public bool enable;
    }
    
    public class GraphViewConfig
    {
        public GraphDirection direction;
        public string searchNamespace;
        
        public MiniMapConfig miniMapConfig;
        public ZoomConfig zoomConfig;

        public GraphObject graphObject;
        public SerializedObject serializedObject;
        public IPortColorManager portColorManager;
        public IExposedPropertyTypeManager exposedPropertyTypeManager;
    }
}