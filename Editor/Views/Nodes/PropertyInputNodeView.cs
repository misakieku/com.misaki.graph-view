using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class PropertyInputNodeView : TokenNode, IPortContainer
    {
        private readonly List<Port> _outputPorts = new List<Port>();
        
        private readonly PropertyInputNode _dataNode;
        //private readonly ExposedPropertyEditor _editor;
        
        public PropertyInputNode DataNode => _dataNode;
        
        public List<Port> InputPorts => null;
        public List<Port> OutputPorts => _outputPorts;
        
        public PropertyInputNodeView(PropertyInputNode dataNode, Port output) : base(null, output)
        {
            _dataNode = dataNode;
            //_editor = editor;

            name = dataNode.Property.propertyName;
            title = dataNode.Property.propertyName;
            userData = dataNode;

            this.Q<VisualElement>("top").style.minHeight = 24;
            
            _outputPorts.Add(output);
        }
        
        public static PropertyInputNodeView Create(PropertyInputNode dataNode, IPortColorManager portColorManager)
        {
            if (dataNode == null)
            {
                return null;
            }
            
            var outputSlot = dataNode.GetSlot(0, SlotDirection.Output);
            var outputPort = CreateOutputPort(dataNode.Property, outputSlot, portColorManager);
            var nodeView = new PropertyInputNodeView(dataNode, outputPort);
            
            return nodeView;
        }
        
        private static Port CreateOutputPort(ExposedProperty property, Slot slot, IPortColorManager portColorManager)
        {
            if (property == null)
            {
                return null;
            }
            
            var portType = property.GetValueType();
            var port = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, portType);
            
            port.portName = string.Empty;
            if (portColorManager != null && portColorManager.TryGetColor(portType, out var portColor))
            {
                port.portColor = portColor;
            }
            port.portType = portType;
            port.userData = slot;
            
            return port;
        }

        public string InspectorName => _dataNode.Property.propertyName;

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _dataNode.position = newPos;
        }
    }
}