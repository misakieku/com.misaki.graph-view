using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class PropertyInputNodeView : TokenNode, IPortContainer
    {
        private readonly Port _outputPort;

        private readonly PropertyInput _data;
        //private readonly ExposedPropertyEditor _editor;

        public PropertyInput Data => _data;

        public PropertyInputNodeView(PropertyInput data, Port output) : base(null, output)
        {
            _data = data;
            _outputPort = output;

            name = data.Property.propertyName;
            title = data.Property.propertyName;
            userData = data;

            this.Q<VisualElement>("top").style.minHeight = 24;
        }

        public static PropertyInputNodeView Create(PropertyInput data, IPortColorManager portColorManager)
        {
            if (data == null)
            {
                return null;
            }

            var outputSlot = data.GetSlot(0, SlotDirection.Output);
            var outputPort = CreateOutputPort(data.Property, outputSlot, portColorManager);
            var nodeView = new PropertyInputNodeView(data, outputPort);

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

        public string InspectorName => _data.Property.propertyName;

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _data.position = newPos;
        }

        public Port GetPort(int index, Direction direction)
        {
            return _outputPort;
        }
    }
}