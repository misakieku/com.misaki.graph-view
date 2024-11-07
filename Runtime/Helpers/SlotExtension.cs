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
        /// <param name="slotData">The slot data to get the value type from.</param>
        /// <returns><see cref="Type"/> The type of the slot value.</returns>
        public static Type GetValueType(this SlotData slotData)
        {
            return Type.GetType(slotData.valueType);
        }

        public static void ReceiveData(this Slot slot, object data)
        {
            slot.value = data;
        }

        /// <summary>
        /// Pull data from the slot and execute the provided action.
        /// </summary>
        /// <param name="slot">The slot to pull data from.</param>
        /// <param name="OnPullData">The action to execute when pulling data.</param>
        public static void PullData(this Slot slot, Action<Slot> OnPullData)
        {
            OnPullData?.Invoke(slot);

            var property = slot.owner.GetType().GetField(slot.slotData.slotName, ConstResource.NODE_FIELD_BINDING_FLAGS);
            property?.SetValue(slot.owner, slot.value);
        }

        /// <summary>
        /// Push data to the slot and execute the provided action.
        /// </summary>
        /// <param name="slot">The slot to push data to.</param>
        /// <param name="OnPushData">The action to execute when pushing data.</param>
        public static void PushData(this Slot slot, Action<Slot> OnPushData)
        {
            var property = slot.owner.GetType().GetField(slot.slotData.slotName, ConstResource.NODE_FIELD_BINDING_FLAGS);
            if (property != null)
            {
                slot.value = property.GetValue(slot.owner);
            }

            OnPushData?.Invoke(slot);

            foreach (var slotData in slot.LinkedSlotData)
            {
                var node = slot.owner.GraphObject.GetNode(slotData.nodeID);
                if (node is not ISlotContainer slotContainer)
                {
                    continue;
                }

                var otherSlot = slotContainer.GetSlot(slotData.slotIndex, slotData.direction);

                if (slotData.GetValueType() == slot.slotData.GetValueType() || slot.slotData.GetValueType() == typeof(object))
                {
                    otherSlot.ReceiveData(slot.value);
                }
                else if (slot.owner.GraphObject.ValueConverterManager != null && slot.owner.GraphObject.ValueConverterManager.TryConvert(slot.slotData.GetValueType(),
                             slotData.GetValueType(), slot.value, out var data))
                {
                    otherSlot.ReceiveData(data);
                }
                else
                {
                    slot.owner.GraphObject.Logger?.LogError(slot.owner, $"Failed to convert value from {otherSlot.slotData.valueType} to {slotData.valueType}");
                }
            }
        }

        /// <summary>
        /// Pull data from a collection of slots and execute the provided action.
        /// </summary>
        /// <param name="slots">The collection of slots to pull data from.</param>
        /// <param name="OnPullData">The action to execute when pulling data.</param>
        public static void PullData(this IEnumerable<Slot> slots, Action<Slot> OnPullData)
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
        public static void PushData(this IEnumerable<Slot> slots, Action<Slot> OnPushData)
        {
            foreach (var slot in slots)
            {
                slot.PushData(OnPushData);
            }
        }
    }
}