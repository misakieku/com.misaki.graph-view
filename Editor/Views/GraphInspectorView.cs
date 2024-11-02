using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public sealed class GraphInspectorView : GraphSubWindow
    {
        private const string UIDocumentPath = "Packages/com.misaki.graph-view/Editor/Views/GraphInspectorView.uxml";
        
        private readonly Label _header;
        private readonly VisualElement _inspectorPropertiesContainer; 
        
        private Vector2 _startMousePosition; 
        private Vector2 _startElementPosition;
        
        public GraphInspectorView()
        {
            style.minWidth = 300;
            style.minHeight = 500;
            
            var uiDocument = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIDocumentPath);
            var inspectorView = uiDocument.Instantiate();
            inspectorView.style.flexGrow = 1;
            
            _header = inspectorView.Q<Label>("node-name-label");
            _header.text = string.Empty;
            _inspectorPropertiesContainer = inspectorView.Q<VisualElement>("inspector-properties-container");
            
            Add(inspectorView);
        }

        private void ClearInspector()
        {
            _header.text = string.Empty;
            _inspectorPropertiesContainer.Clear();
        }
        
        public void OnNodeSelectionChanged(IInspectable selection)
        {
            ClearInspector();

            if (selection == null)
            {
                return;
            }

            _header.text = selection.InspectorName ?? "Inspector";
            _inspectorPropertiesContainer.Add(selection.CreateInspector());
        }
    }
}