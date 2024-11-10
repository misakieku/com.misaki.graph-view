using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class RelayNodeView : Node, IPortContainer, IDataNodeView<RelayNode>
    {
        private RelayNode _dataNode;
        private IPortColorManager _portColorManager;

        private readonly Port _inputPort;
        private readonly Port _outputPort;

        public RelayNode DataNode => _dataNode;
        public DataNode GetDataNode() => _dataNode;

        public RelayNodeView(RelayNode dataNode, IPortColorManager portColorManager)
        {
            _dataNode = dataNode;
            _portColorManager = portColorManager;
            userData = dataNode;

            _inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(object));
            _outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(object));

            _inputPort.userData = _dataNode.GetSlot(0, SlotDirection.Input);
            _outputPort.userData = _dataNode.GetSlot(0, SlotDirection.Output);

            this.Q<VisualElement>("title").style.height = 0;
            var divider = this.Q<VisualElement>("divider");
            divider.style.height = 0;
            divider.style.borderBottomWidth = 0;

            _inputPort.style.height = 16;
            _outputPort.style.height = 16;

            inputContainer.Add(_inputPort);
            outputContainer.Add(_outputPort);

            var portType = Type.GetType(_dataNode.portValueType) ?? typeof(object);
            SetPortsTypeAndColor(portType);
        }

        public void Connect(Edge edge, out SlotConnection inputConnection, out SlotConnection outputConnection, out Edge inputEdge, out Edge outputEdge)
        {
            inputConnection = default;
            outputConnection = default;
            inputEdge = null;
            outputEdge = null;

            var input = edge.input;
            var output = edge.output;
            if (input.userData is ISlot inputSlot && output.userData is ISlot outputSlot)
            {
                inputSlot.Unlink(outputSlot);

                input.Disconnect(edge);
                output.Disconnect(edge);

                _dataNode.BindSlot(inputSlot);
                _dataNode.BindSlot(outputSlot);

                var outputProxySlot = (ProxySlot)_dataNode.GetSlot(0, SlotDirection.Output);
                inputSlot.Link(outputProxySlot.MasterSlot, out _);
                inputConnection = new(inputSlot.SlotData, outputProxySlot.SlotData);

                _dataNode.GetSlot(0, SlotDirection.Input).Link(outputSlot, out outputConnection);

                inputEdge = output.ConnectTo(_inputPort);
                outputEdge = _outputPort.ConnectTo(input);

                SetPortsTypeAndColor(input.portType);
            }
        }

        public void Disconnect(out List<SlotConnection> newConnections, out List<Edge> newEdges)
        {
            newConnections = new List<SlotConnection>();
            newEdges = new List<Edge>();

            if (_inputPort.userData is not ProxySlot inputSlot || _outputPort.userData is not ProxySlot outputSlot)
            {
                return;
            }

            var linkedOutputPort = _inputPort.connections.FirstOrDefault()?.output;

            if (linkedOutputPort == null)
            {
                return;
            }

            inputSlot.MasterSlot.Unlink(outputSlot.MasterSlot);

            foreach (var edge in _outputPort.connections.ToList())
            {
                var linkedInputPort = edge.input;
                if (linkedOutputPort.userData is ISlot linkedOutputSlot && linkedInputPort.userData is ISlot linkedInputSlot)
                {
                    linkedOutputSlot.Link(linkedInputSlot, out var inputConnection);
                    newConnections.Add(inputConnection);
                    newEdges.Add(linkedOutputPort.ConnectTo(linkedInputPort));
                }
            }

            inputSlot.UnlinkAll();
            outputSlot.UnlinkAll();

            _inputPort.DisconnectAll();
            _outputPort.DisconnectAll();

            return;
        }

        private void SetPortsTypeAndColor(Type portType)
        {
            _inputPort.portType = portType;
            _outputPort.portType = portType;

            _inputPort.portName = string.Empty;
            _outputPort.portName = string.Empty;

            if (_portColorManager != null)
            {
                if (_portColorManager.TryGetColor(portType, out var portColor))
                {
                    _inputPort.portColor = portColor;
                    _outputPort.portColor = portColor;
                }
            }

            _dataNode.portValueType = portType.FullName;
        }

        public Port GetPort(int index, Direction direction)
        {
            return direction == Direction.Input ? _inputPort : _outputPort;
        }
    }
}
