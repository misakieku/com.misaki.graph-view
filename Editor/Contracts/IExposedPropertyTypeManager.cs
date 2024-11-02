using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Misaki.GraphView.Editor
{
    public interface IExposedPropertyTypeManager
    {
        public void AddPropertyType<T, TV>() where T : ExposedProperty;
        public void AddPropertyType(Type type, Type valueType);
        
        public void RemovePropertyType<T>();
        public void RemovePropertyType(Type type);
        
        public ReadOnlyDictionary<Type, Type> GetPropertyTypes();
    }
}