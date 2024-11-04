using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class Slot
    {
        [SerializeField] 
        private List<SlotData> _linkedSlotData = new();
        
        public ReadOnlyCollection<SlotData> LinkedSlotData => _linkedSlotData.AsReadOnly();

        [SerializeReference]
        public DataNode owner;
        public SlotData slotData;
        
        public object value;
        
        public Slot(DataNode owner, SlotData slotData)
        {
            this.owner = owner;
            this.slotData = slotData;
        }
        
        /// <summary>
        /// Link the current slot with another slot.
        /// </summary>
        /// <param name="other"> The slot need to be linked </param>
        public void Link(Slot other)
        {
            if (other.slotData.direction == slotData.direction)
            {
                return;
            }
            
            if (_linkedSlotData.Contains(other.slotData))
            {
                return;
            }
            
            _linkedSlotData.Add(other.slotData);
            other._linkedSlotData.Add(slotData);
        }
        
        /// <summary>
        /// Unlink the current slot with another slot.
        /// </summary>
        /// <param name="other"> The slot need to be unlinked </param>
        public void Unlink(Slot other)
        {
            _linkedSlotData.Remove(other.slotData);
            other._linkedSlotData.Remove(slotData);
        }

        public void ReceiveData(object data)
        {
            value = data;
            // We move this to PullData method in BaseNode
            //owner.GetType().GetField(slotData.slotName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(owner, data);
        }
    }
}