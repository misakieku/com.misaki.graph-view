using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Misaki.GraphView.Editor.Editor.Helpers
{
    public class PropertyFieldHelper
    {
        private static readonly Dictionary<Type, Func<string, VisualElement>> _propertyFieldCreators = new ()
        {
            {typeof(float), (s) => new FloatField(s)},
            {typeof(int), (s) => new IntegerField(s)},
            {typeof(uint), (s) => new IntegerField(s)},
            {typeof(long), (s) => new LongField(s)},
            {typeof(bool), (s) => new Toggle(s)},
            {typeof(string), (s) => new TextField(s)}
        };
        
        public static VisualElement CreatePropertyField(Type propertyType, object dataSource, PropertyPath bindingPath, string label)
        {
            if (_propertyFieldCreators.TryGetValue(propertyType, out var creator))
            {
                var propertyField = creator.Invoke(label);
                propertyField.dataSource = dataSource;
                propertyField.dataSourcePath = bindingPath;
                return propertyField;
            }

            return null;
        }
    }
}