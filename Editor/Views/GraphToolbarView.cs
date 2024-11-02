using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class GraphToolbarView : Toolbar
    {
        private readonly GraphObject _graphObject;
        
        private readonly ToolbarMenu _assetActionMenu = new();
        private readonly ToolbarButton _saveButton = new();
        
        private readonly ToolbarButton _executeButton = new();
        
        private readonly VisualElement _customElementContainer = new ();
        
        private readonly ToolbarButton _blackboardButton = new();
        private readonly ToolbarButton _inspectorButton = new();
        
        public Action BlackboardButtonClicked;
        public Action InspectButtonClicked;
        
        public GraphToolbarView(GraphObject graphObject)
        {
            _graphObject = graphObject;
            
            _saveButton.iconImage = new () {texture = (Texture2D)EditorGUIUtility.IconContent("SaveAs").image};
            _saveButton.tooltip = "Save";
            _saveButton.clicked += SaveAsset;
            _saveButton.style.borderRightWidth = 0;
            
            _assetActionMenu.tooltip = "Asset Actions";
            _assetActionMenu.menu.AppendAction("Save As", a => SaveAsAsset());
            _assetActionMenu.menu.AppendAction("Select Asset", a => Selection.activeObject = _graphObject);
            _assetActionMenu.style.borderLeftWidth = 0;
            
            _executeButton.iconImage = new () {texture = (Texture2D)EditorGUIUtility.IconContent("PlayButton").image};
            _executeButton.tooltip = "Execute";
            _executeButton.clicked += () => _graphObject.Execute();
            
            _customElementContainer.style.flexGrow = 1;
            
            _blackboardButton.iconImage = new () {texture = (Texture2D)EditorGUIUtility.IconContent("Audio Mixer").image};
            _blackboardButton.tooltip = "Blackboard";
            _blackboardButton.clicked += () => BlackboardButtonClicked?.Invoke();
            
            _inspectorButton.iconImage = new () {texture = (Texture2D)EditorGUIUtility.IconContent("UnityEditor.InspectorWindow").image};
            _inspectorButton.tooltip = "Inspector";
            _inspectorButton.clicked += () => InspectButtonClicked?.Invoke();
            
            Add(_saveButton);
            Add(CreateSmallSplitter());
            Add(_assetActionMenu);
            Add(new ToolbarSpacer());
            Add(_executeButton);
            
            Add(_customElementContainer);
            
            Add(_blackboardButton);
            Add(_inspectorButton);
        }

        private void SaveAsset()
        {
            AssetDatabase.SaveAssetIfDirty(_graphObject);
        }
        
        private void SaveAsAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Graph", "NewGraph", "asset", "Save Graph");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            
            AssetDatabase.CreateAsset(_graphObject, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public static VisualElement CreateSmallSplitter()
        {
            var splitter = new VisualElement()
            {
                style =
                {
                    marginTop = 4,
                    marginBottom = 4,
                    backgroundColor = new Color(0.15f, 0.15f, 0.15f),
                    width = 2
                }
            };
            
            return splitter;
        }

        public void SetCustomElement(VisualElement element)
        {
            _customElementContainer.Clear();
            _customElementContainer.Add(element);
        }
    }
}