using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class GraphBlackboardView : Blackboard
    {
        private readonly GraphObject _graphObject;
        private readonly GraphView _owner;
        private readonly SerializedObject _serializedObject;
        private readonly IExposedPropertyTypeManager _exposedPropertyTypeManager;
        
        public Action<IInspectable> OnPropertySelected;
        
        public GraphBlackboardView(GraphObject graphObject, GraphView owner, SerializedObject serializedObject, IExposedPropertyTypeManager exposedPropertyTypeManager)
        {
            style.marginBottom = 8;
            style.marginTop = 8;
            style.marginLeft = 8;
            style.marginRight = 8;
            
            _graphObject = graphObject;
            _owner = owner;
            _serializedObject = serializedObject;
            _exposedPropertyTypeManager = exposedPropertyTypeManager;
            
            title = "Exposed Properties";
            subTitle = graphObject.name;
            
            addItemRequested = OnAddItemRequested;
            editTextRequested += OnEditTextRequested;
        }

        private void OnAddItemRequested(Blackboard blackboard)
        {
            if (_exposedPropertyTypeManager == null)
            {
                return;
            }
            
            var menu = new GenericMenu();
            
            foreach (var type in _exposedPropertyTypeManager.GetPropertyTypes())
            {
                menu.AddItem(new GUIContent(type.Value.Name), false, () =>
                {
                    AddProperty(type.Key);
                });
            }
            
            menu.ShowAsContext();
        }
        
        private void OnEditTextRequested(Blackboard blackboard, VisualElement element, string newValue)
        {
            if (element is BlackboardPropertyView propertyView)
            {
                propertyView.text = newValue;

                if (propertyView.userData is not ExposedProperty exposedProperty)
                {
                    return;
                }
                
                exposedProperty.propertyName = newValue;
                _owner.Query<PropertyInputNodeView>().ForEach(n =>
                {
                    if (n.Data.Property.Equals(exposedProperty))
                    {
                        n.title = newValue;
                    }
                });
                
                _serializedObject.Update();
            }
        }
        
        private void AddProperty(Type type)
        {
            var property = Activator.CreateInstance(type) as ExposedProperty;

            if (property == null)
            {
                return;
            }
            
            property.propertyName = $"New {property.GetValueType().Name} Property";
            property.propertyType = type.FullName;
            
            AddProperty(property);
            
            _graphObject.AddExposedProperty(property);
            _serializedObject.Update();
        }
        
        public void AddProperty(ExposedProperty property)
        {
            var shortTypeName = property.GetValueType().Name;
            
            var editorTypes = TypeCache.GetTypesWithAttribute<CustomInspectorAttribute>();
            var type = editorTypes.FirstOrDefault(t => t.GetCustomAttribute<CustomInspectorAttribute>().InspectorType == property.GetType()) ?? typeof(ExposedPropertyEditor);
            var editor = Activator.CreateInstance(type, property, _serializedObject) as ExposedPropertyEditor;
            
            var blackboardField = new BlackboardPropertyView (editor)
            {
                text = property.propertyName, 
                typeText = shortTypeName,
                userData = property,
                OnItemSelected = OnPropertySelected
            };

            Add(blackboardField);
        }
    }
}