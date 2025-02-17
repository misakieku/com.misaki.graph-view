using System;

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

        public SlotData(string name, string id, int index, SlotDirection slotDirection, Type type)
        {
            slotName = name;
            nodeID = id;
            slotIndex = index;
            direction = slotDirection;
            valueType = type.AssemblyQualifiedName;
        }

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
}
