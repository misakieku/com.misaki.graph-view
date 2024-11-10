using System;
using System.Collections.Generic;

namespace Misaki.GraphView
{
    public static class SlotExtension
    {
        /// <summary>
        /// Unlink all slots from this slot.
        /// </summary>
        /// <param name="slot">The slot to unlink all connections from.</param>
        public static void UnlinkAll(this ISlot slot)
        {
            var slotCount = slot.LinkedSlotDatas.Count;
            for (var i = 0; i < slotCount; i++)
            {
                var other = slot.LinkedSlotDatas[i];
                var otherNode = slot.Owner.GraphObject.GetNode(other.nodeID);

                if (otherNode is ISlotContainer slotContainer)
                {
                    slotContainer.GetSlot(other.slotIndex, other.direction)?.Unlink(slot);
                }
            }
        }

        /// <summary>
        /// Get the value type of the slot data.
        /// </summary>
        /// <param name="slotData">The slot data to get the value type from.</param>
        /// <returns><see cref="Type"/> The type of the slot value.</returns>
        public static Type GetValueType(this SlotData slotData)
        {
            return Type.GetType(slotData.valueType);
        }

        /// <summary>
        /// Pull data from a collection of slots and execute the provided action.
        /// </summary>
        /// <param name="slots">The collection of slots to pull data from.</param>
        /// <param name="OnPullData">The action to execute when pulling data.</param>
        public static void PullData(this IEnumerable<ISlot> slots, Action<ISlot> OnPullData)
        {
            foreach (var slot in slots)
            {
                slot.PullData(OnPullData);
            }
        }

        /// <summary>
        /// Push data to a collection of slots and execute the provided action.
        /// </summary>
        /// <param name="slots">The collection of slots to push data to.</param>
        /// <param name="OnPushData">The action to execute when pushing data.</param>
        public static void PushData(this IEnumerable<ISlot> slots, Action<ISlot> OnPushData)
        {
            foreach (var slot in slots)
            {
                slot.PushData(OnPushData);
            }
        }
    }
}