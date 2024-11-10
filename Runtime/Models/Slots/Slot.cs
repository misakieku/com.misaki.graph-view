using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Misaki.GraphView
{
    [Serializable]
    public class Slot : ISlot
    {
        [SerializeField]
        private SlotData _slotData;
        [FormerlySerializedAs("_linkedSlotDatas")] [SerializeField]
        private List<SlotData> _linkedSlotData = new();
        [SerializeReference]
        private DataNode _owner;

        private object _data;

        public SlotData SlotData => _slotData;
        public List<SlotData> LinkedSlotData => _linkedSlotData;
        public bool IsLinked => _linkedSlotData.Count > 0;
        public DataNode Owner => _owner;
        public object Data => _data;

        public Slot(DataNode owner, SlotData slotData)
        {
            _owner = owner;
            _slotData = slotData;
        }

        /// <inheritdoc/>
        public bool Link(ISlot other, out SlotConnection connection)
        {
            connection = new(_slotData, other.SlotData);

            if (other.SlotData.direction == _slotData.direction ||
                _linkedSlotData.Contains(other.SlotData))
            {
                return false;
            }

            _linkedSlotData.Add(other.SlotData);
            other.LinkedSlotData.Add(_slotData);

            return true;
        }

        /// <inheritdoc/>
        public void Unlink(ISlot other)
        {
            _linkedSlotData.Remove(other.SlotData);
            other.LinkedSlotData.Remove(_slotData);
        }

        /// <inheritdoc/>
        public void PullData(Action<ISlot> OnPullData)
        {
            if (_slotData.direction == SlotDirection.Output)
            {
                return;
            }

            OnPullData?.Invoke(this);

            var property = _owner.GetType().GetField(_slotData.slotName, ConstResource.NODE_FIELD_BINDING_FLAGS);
            if (IsLinked && property != null)
            {
                property?.SetValue(_owner, _data);
            }
        }

        public void PushData(Action<ISlot> OnPushData)
        {
            if (_slotData.direction == SlotDirection.Input)
            {
                return;
            }

            var property = _owner.GetType().GetField(_slotData.slotName, ConstResource.NODE_FIELD_BINDING_FLAGS);
            if (property != null)
            {
                ReceiveData(property.GetValue(_owner));
            }

            OnPushData?.Invoke(this);

            foreach (var connectedSlotData in _linkedSlotData)
            {
                var node = _owner.GraphObject.GetNode(connectedSlotData.nodeID);
                if (node is not ISlotContainer slotContainer)
                {
                    continue;
                }

                var connectedSlot = slotContainer.GetSlot(connectedSlotData.slotIndex, connectedSlotData.direction);

                if (connectedSlotData.GetValueType() == _slotData.GetValueType() || _slotData.GetValueType() == typeof(object) || connectedSlotData.GetValueType() == typeof(object))
                {
                    connectedSlot.ReceiveData(_data);
                }
                else if (_owner.GraphObject.ValueConverterManager != null && _owner.GraphObject.ValueConverterManager.TryConvert(_slotData.GetValueType(),
                             connectedSlotData.GetValueType(), _data, out var data))
                {
                    connectedSlot.ReceiveData(data);
                }
                else
                {
                    _owner.GraphObject.Logger?.LogError(_owner, $"Failed to convert value from {_slotData.valueType} to {connectedSlotData.valueType}");
                    _owner.GraphObject.GraphProcessor.Break();
                }
            }
        }

        /// <inheritdoc/>
        public void ReceiveData(object data)
        {
            _data = data;
        }
    }
}