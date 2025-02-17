using UnityEditor;
using UnityEngine;

namespace Misaki.GraphView.Editor
{
    public struct BlackboardConfig
    {
        public bool enable;

        public static BlackboardConfig Default = new()
        {
            enable = true,
        };
    }

    public struct MiniMapConfig
    {
        public bool enable;
        public Rect position;

        public static MiniMapConfig Default = new()
        {
            enable = true,
            position = new(10, 20, 200, 200)
        };
    }

    public struct ZoomConfig
    {
        public float minScale;
        public float maxScale;
        public float scaleStep;

        public static ZoomConfig Default = new()
        {
            minScale = 0.1f,
            maxScale = 2.0f,
            scaleStep = 0.1f
        };
    }

    public struct InspectorConfig
    {
        public bool enable;

        public static InspectorConfig Default = new()
        {
            enable = true,
        };
    }

    public class GraphViewConfig
    {
        public GraphDirection direction;
        public string searchNamespace;

        public MiniMapConfig miniMapConfig = MiniMapConfig.Default;
        public ZoomConfig zoomConfig = ZoomConfig.Default;

        public GraphObject graphObject;
        public SerializedObject serializedObject;
        public IPortColorManager portColorManager;
        public IExposedPropertyTypeManager exposedPropertyTypeManager;
    }
}