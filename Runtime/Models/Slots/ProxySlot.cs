using System;
using System.Collections.Generic;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public class ProxySlot : ISlot
    {
        [SerializeReference]
        private ISlot _masterSlot;

        [SerializeField]
        private SlotData _slotData;
        [SerializeField]
        private List<SlotData> _linkedSlotDatas = new();
        [SerializeReference]
        private DataNode _owner;

        public SlotData SlotData => _slotData;
        public List<SlotData> LinkedSlotDatas => _linkedSlotDatas;
        public bool IsLinked => _linkedSlotDatas.Count > 0;
        public DataNode Owner => _owner;

        public ISlot MasterSlot => _masterSlot;
        public object Data => _masterSlot?.Data;

        public ProxySlot(DataNode owner, SlotData slotData)
        {
            _owner = owner;
            _slotData = slotData;
        }

        public void Bind(ISlot slot)
        {
            _masterSlot = slot;

            _slotData.direction = slot.SlotData.direction;
            _slotData.valueType = slot.SlotData.valueType;
        }

        public void Unbind()
        {
            _masterSlot = null;
        }

        public bool Link(ISlot other, out SlotConnection connection)
        {
            connection = new(_slotData, other.SlotData);

            if (other.SlotData.direction == _slotData.direction ||
                _linkedSlotDatas.Contains(other.SlotData) ||
                _masterSlot == null)
            {
                return false;
            }

            _linkedSlotDatas.Add(other.SlotData);
            return _masterSlot.Link(other, out _);
        }

        public void Unlink(ISlot other)
        {
            _linkedSlotDatas.Remove(other.SlotData);
            _masterSlot?.Unlink(other);
        }

        public void PullData(Action<ISlot> OnPullData)
        {
            _masterSlot.PullData(OnPullData);
        }

        public void PushData(Action<ISlot> OnPushData)
        {
            _masterSlot.PushData(OnPushData);
        }

        public void ReceiveData(object data)
        {
            _masterSlot?.ReceiveData(data);
        }
    }
}