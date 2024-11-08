using System;
using System.Collections.Generic;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public class ProxySlot : ISlot
    {
        [SerializeReference]
        private ISlot _slot;

        public SlotData SlotData => _slot == null ? default : _slot.SlotData;
        public List<SlotData> LinkedSlotDatas => _slot?.LinkedSlotDatas;
        public bool IsLinked => LinkedSlotDatas?.Count > 0;
        public DataNode Owner => _slot?.Owner;
        public object Data => _slot?.Data;

        public void Bind(ISlot slot)
        {
            _slot = slot;
        }

        public bool Link(ISlot other, out SlotConnection connection)
        {
            if (_slot == null)
            {
                connection = default;
                return false;
            }

            return _slot.Link(other, out connection);
        }
        public void Unlink(ISlot other)
        {
            _slot?.Unlink(other);
        }

        public void ReceiveData(object data)
        {
            _slot?.ReceiveData(data);
        }
    }
}