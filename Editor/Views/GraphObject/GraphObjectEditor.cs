using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    [CustomEditor(typeof(GraphObject))]
    public class GraphObjectEditor : UnityEditor.Editor
    {
        private readonly Dictionary<string, List<SerializedProperty>> _inspectorPropertyMap = new();

        private GraphObject _graphObject;

        private void OnEnable()
        {
            _graphObject = target as GraphObject;

            if (_graphObject == null)
            {
                return;
            }

            GetSerializedProperty();
        }

        protected virtual void GetSerializedProperty()
        {
            foreach (var property in _graphObject.ExposedProperties)
            {
                var showInInspectorField = property.GetType().GetField(nameof(ExposedProperty.showInInspector));
                if (showInInspectorField == null)
                {
                    continue;
                }

                var showInInspectorValue = showInInspectorField.GetValue(property);
                if (showInInspectorValue is not bool or false)
                {
                    continue;
                }

                var fields = property.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                var i = _graphObject.ExposedProperties.IndexOf(property);
                foreach (var field in fields)
                {
                    var serializedProperty = serializedObject.FindProperty("_exposedProperties")?.GetArrayElementAtIndex(i)?.FindPropertyRelative(field.Name);
                    if (serializedProperty == null)
                    {
                        continue;
                    }

                    if (!_inspectorPropertyMap.ContainsKey(property.propertyName))
                    {
                        _inspectorPropertyMap[property.propertyName] = new();
                    }

                    _inspectorPropertyMap[property.propertyName].Add(serializedProperty);
                }
            }
        }

        protected virtual VisualElement CreateInspectorProperty()
        {
            if (_inspectorPropertyMap.Count <= 0)
            {
                return null;
            }

            var graphPropertyFoldout = new Foldout()
            {
                text = "Graph Properties",
                value = true
            };

            foreach (var property in _inspectorPropertyMap)
            {
                var label = new Label(property.Key)
                {
                    style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        marginTop = 4,
                        marginBottom = 2,
                    }
                };

                var propertyContainer = new VisualElement()
                {
                    style =
                    {
                        marginLeft = 15
                    }
                };

                foreach (var serializedProperty in property.Value)
                {
                    var inputField = new PropertyField(serializedProperty);
                    inputField.Bind(serializedObject);
                    propertyContainer.Add(inputField);
                }

                graphPropertyFoldout.Add(label);
                graphPropertyFoldout.Add(propertyContainer);
            }

            return graphPropertyFoldout;
        }
    }
}