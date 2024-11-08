using System;
using System.Collections.Generic;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public struct SlotData : IEquatable<SlotData>
    {
        public string slotName;
        public string nodeID;
        public int slotIndex;
        public SlotDirection direction;
        public string valueType;

        public bool Equals(SlotData other)
        {
            return slotName == other.slotName && nodeID == other.nodeID && slotIndex == other.slotIndex && direction == other.direction && valueType == other.valueType;
        }

        public override bool Equals(object obj)
        {
            return obj is SlotData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(slotName, nodeID, slotIndex, direction, valueType);
        }

        public static bool operator ==(SlotData left, SlotData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SlotData left, SlotData right)
        {
            return !left.Equals(right);
        }
    }

    [Serializable]
    public class Slot : ISlot
    {
        [SerializeField]
        private SlotData _slotData;
        [SerializeField]
        private List<SlotData> _linkedSlotData = new();
        [SerializeReference]
        private DataNode _owner;

        private object _data;

        public SlotData SlotData => _slotData;
        public List<SlotData> LinkedSlotDatas => _linkedSlotData;
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
            connection = default;
            if (other.SlotData.direction == _slotData.direction)
            {
                return false;
            }

            if (_linkedSlotData.Contains(other.SlotData))
            {
                return false;
            }

            _linkedSlotData.Add(other.SlotData);
            other.LinkedSlotDatas.Add(_slotData);
            connection = new(_slotData, other.SlotData);

            return true;
        }

        /// <inheritdoc/>
        public void Unlink(ISlot other)
        {
            _linkedSlotData.Remove(other.SlotData);
            other.LinkedSlotDatas.Remove(_slotData);
        }

        /// <inheritdoc/>
        public void ReceiveData(object data)
        {
            _data = data;
        }
    }
}