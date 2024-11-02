using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Codice.CM.Common;
using Misaki.GraphView.Editor;

namespace Misaki.GraphView.Sample.Editor
{
    public class ExposedPropertyTypeManager : IExposedPropertyTypeManager
    {
        private readonly Dictionary<Type, Type> _propertyTypes = new();
        
        public void AddPropertyType<T, TV>() where T : ExposedProperty
        {
            _propertyTypes.Add(typeof(T), typeof(TV));
        }

        public void AddPropertyType(Type type, Type valueType)
        {
            if (type.IsSubclassOf(typeof(ExposedProperty)))
            {
                _propertyTypes.Add(type, valueType);
            }
        }

        public void RemovePropertyType<T>()
        {
            _propertyTypes.Remove(typeof(T));
        }

        public void RemovePropertyType(Type type)
        {
            _propertyTypes.Remove(type);
        }

        public ReadOnlyDictionary<Type, Type> GetPropertyTypes()
        {
            //_propertyTypes.AsReadOnly(); // Ancient .NET version doesn't have this method :(
            return new ReadOnlyDictionary<Type, Type>(_propertyTypes);
        }
    }
}