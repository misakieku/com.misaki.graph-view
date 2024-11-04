using System;
using UnityEngine;

namespace Misaki.GraphView
{
    public class PropertyInput : ExecutableNode
    {
        [SerializeReference]
        private ExposedProperty _property;
        
        public ExposedProperty Property => _property;
        
        [NodeOutput]
        public object value;

        public PropertyInput(ExposedProperty property)
        {
            _property = property;
        }
        
        protected override bool OnExecute()
        {
            value = _property.Value;
            
            return true;
        }
    }
}