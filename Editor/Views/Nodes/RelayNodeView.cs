using System;
using UnityEditor.Experimental.GraphView;

namespace Misaki.GraphView.Editor
{
    public class RelayNodeView : Node, IPortContainer
    {
        private RelayNode _dataNode;
        private IPortColorManager _portColorManager;

        private readonly Port _inputPort;
        private readonly Port _outputPort;

        public RelayNodeView(RelayNode dataNode, IPortColorManager portColorManager)
        {
            _dataNode = dataNode;
            _portColorManager = portColorManager;

            _inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(object));
            _outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));

            _inputPort.portName = string.Empty;
            _outputPort.portName = string.Empty;

            SetPortsTypeAndColor(typeof(object));

            inputContainer.Add(_inputPort);
            outputContainer.Add(_outputPort);
        }

        private void SetPortsTypeAndColor(Type portType)
        {
            _inputPort.portType = portType;
            _outputPort.portType = portType;

            if (_portColorManager != null)
            {
                if (_portColorManager.TryGetColor(portType, out var portColor))
                {
                    _inputPort.portColor = portColor;
                    _outputPort.portColor = portColor;
                }
            }
        }

        public Port GetPort(int index, Direction direction)
        {
            return direction == Direction.Input ? _inputPort : _outputPort;
        }
    }
}
