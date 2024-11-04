using System;

namespace Misaki.GraphView
{
    public static class SlotExtension
    {
        /// <summary>
        /// Unlink all slots from this slot.
        /// </summary>
        public static void UnlinkAll(this Slot slot)
        {
            var slotCount = slot.LinkedSlotData.Count;
            for (var i = 0; i < slotCount; i++)
            {
                var other = slot.LinkedSlotData[i];
                var otherNode = slot.owner.GraphObject.GetNode(other.nodeID);

                if (otherNode is ISlotContainer slotContainer)
                {
                    slotContainer.GetSlot(other.slotIndex, other.direction)?.Unlink(slot);
                }
            }
        }
        
        /// <summary>
        /// Get the value type of the slot data.
        /// </summary>
        /// <returns><see cref="Type"/> The type of the slot value </returns>
        public static Type GetValueType(this SlotData slotData)
        {
            return Type.GetType(slotData.valueType);
        }
    }
}