using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Misaki.GraphView.Sample.Editor
{
    [CustomEditor(typeof(SampleGraphAsset))]
    public class SampleGraphAssetEditor : UnityEditor.Editor
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

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Execute"))
            {
                var asset = target as SampleGraphAsset;
                asset?.Execute();
            }
        }
    }
}