using System;
using UnityEngine;

namespace Misaki.GraphView
{
    public class PropertyInputNode : BaseNode
    {
        [SerializeReference]
        private ExposedProperty _property;
        
        public ExposedProperty Property => _property;
        
        [NodeOutput]
        public object value;

        public PropertyInputNode(ExposedProperty property)
        {
            _property = property;
        }
        
        protected override void OnExecute()
        {
            value = _property.Value;
        }
    }
}