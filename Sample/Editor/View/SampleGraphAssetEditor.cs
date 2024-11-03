using Misaki.GraphView.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Sample.Editor
{
    [CustomEditor(typeof(SampleGraphAsset))]
    public class SampleGraphAssetEditor : GraphObjectEditor
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as SampleGraphAsset;

            if (asset != null)
            {
                SampleGraphEditor.Open(asset);
                return true;
            }

            return false;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            var graphProperties = CreateInspectorProperty();
            
            var executeButton = new Button(() =>
            {
                var graph = target as SampleGraphAsset;
                graph?.Execute();
            })
            {
                text = "Execute"
            };
            
            root.Add(graphProperties);
            root.Add(executeButton);
            return root;
        }
    }
}