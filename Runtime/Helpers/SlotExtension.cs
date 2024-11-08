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
        /// Pull data from the slot to property and execute the provided action.
        /// </summary>
        /// <param name="slot">The slot to pull data from.</param>
        /// <param name="OnPullData">The action to execute when pulling data.</param>
        public static void PullData(this ISlot slot, Action<ISlot> OnPullData)
        {
            if (slot.SlotData.direction == SlotDirection.Output)
            {
                return;
            }

            foreach (var slotData in slot.LinkedSlotDatas)
            {
                var node = slot.Owner.GraphObject.GetNode(slotData.nodeID);
                if (node is not IExecutable executable)
                {
                    continue;
                }

                if (!executable.IsExecuted)
                {
                    executable.Execute();
                }
            }

            OnPullData?.Invoke(slot);

            var property = slot.Owner.GetType().GetField(slot.SlotData.slotName, ConstResource.NODE_FIELD_BINDING_FLAGS);
            if (slot.IsLinked && property != null)
            {
                property?.SetValue(slot.Owner, slot.Data);
            }
        }

        /// <summary>
        /// Push data to the slot and execute the provided action.
        /// </summary>
        /// <param name="slot">The slot to push data to.</param>
        /// <param name="OnPushData">The action to execute when pushing data.</param>
        public static void PushData(this ISlot slot, Action<ISlot> OnPushData)
        {
            if (slot.SlotData.direction == SlotDirection.Input)
            {
                return;
            }

            var property = slot.Owner.GetType().GetField(slot.SlotData.slotName, ConstResource.NODE_FIELD_BINDING_FLAGS);
            if (property != null)
            {
                slot.ReceiveData(property.GetValue(slot.Owner));
            }

            OnPushData?.Invoke(slot);

            foreach (var connectedSlotData in slot.LinkedSlotDatas)
            {
                var node = slot.Owner.GraphObject.GetNode(connectedSlotData.nodeID);
                if (node is not ISlotContainer slotContainer)
                {
                    continue;
                }

                var connectedSlot = slotContainer.GetSlot(connectedSlotData.slotIndex, connectedSlotData.direction);

                if (connectedSlotData.GetValueType() == slot.SlotData.GetValueType() || slot.SlotData.GetValueType() == typeof(object) || connectedSlotData.GetValueType() == typeof(object))
                {
                    connectedSlot.ReceiveData(slot.Data);
                }
                else if (slot.Owner.GraphObject.ValueConverterManager != null && slot.Owner.GraphObject.ValueConverterManager.TryConvert(slot.SlotData.GetValueType(),
                             connectedSlotData.GetValueType(), slot.Data, out var data))
                {
                    connectedSlot.ReceiveData(data);
                }
                else
                {
                    slot.Owner.GraphObject.Logger?.LogError(slot.Owner, $"Failed to convert value from {slot.SlotData.valueType} to {connectedSlotData.valueType}");
                }
            }
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