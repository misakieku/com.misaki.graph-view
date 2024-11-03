using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class ExposedPropertyEditor
    {
        private readonly ExposedProperty _property;
        private readonly SerializedObject _serializedObject;
        
        public ExposedPropertyEditor(ExposedProperty property, SerializedObject serializedObject)
        {
            _property = property;
            _serializedObject = serializedObject;
        }
        
        public virtual VisualElement CreateInspector()
        {
            var root = new VisualElement();
            
            if (_serializedObject.targetObject is not GraphObject graphObject)
            {
                return root;
            }

            // Use reflection to get the inspector input fields
            var fields = _property.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

            if (fields.Length == 0)
            {
                var label = new Label("No properties to display.");
                root.Add(label);

                return root;
            }
            
            var i = graphObject.ExposedProperties.IndexOf(_property);
            foreach (var field in fields)
            {
                var serializedProperty = _serializedObject.FindProperty("_exposedProperties")?.GetArrayElementAtIndex(i)?.FindPropertyRelative(field.Name);
                
                if (serializedProperty == null)
                {
                    continue;
                }
                
                var propertyName = ObjectNames.NicifyVariableName(field.Name);
                var inputField = new PropertyField(serializedProperty, propertyName);
                inputField.Bind(_serializedObject);
                
                root.Add(inputField);
            }
            
            var showInInspectorProperty = _serializedObject.FindProperty("_exposedProperties")?.GetArrayElementAtIndex(i)?.FindPropertyRelative("showInInspector");
            if (showInInspectorProperty != null)
            {
                var showInInspectorField = new PropertyField(showInInspectorProperty, "Show In Inspector");
                showInInspectorField.Bind(_serializedObject);
                
                root.Add(showInInspectorField);
            }
            
            return root;
        }
    }
}