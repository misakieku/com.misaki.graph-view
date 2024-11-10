using System;
using System.Collections.Generic;

namespace Misaki.GraphView
{
    public interface ISlot
    {
        /// <summary>
        /// The slot data.
        /// </summary>
        public SlotData SlotData
        {
            get;
        }

        /// <summary>
        /// The linked slot datas.
        /// </summary>
        public List<SlotData> LinkedSlotDatas
        {
            get;
        }

        /// <summary>
        /// Is the slot linked with another slot.
        /// </summary>
        public bool IsLinked
        {
            get;
        }

        /// <summary>
        /// The owner of the slot.
        /// </summary>
        public DataNode Owner
        {
            get;
        }

        /// <summary>
        /// The data buffer of the slot.
        /// </summary>
        public object Data
        {
            get;
        }

        /// <summary>
        /// Link the current slot with another slot.
        /// </summary>
        /// <param name="other"> <see cref="ISlot"/> The slot need to be linked </param>
        /// <param name="connection"> <see cref="SlotConnection"/> The connection that created when link </param>
        /// <returns> True if the link action is succeed, otherwise false </returns>
        public bool Link(ISlot other, out SlotConnection connection);

        /// <summary>
        /// Unlink the current slot with another slot.W
        /// </summary>
        /// <param name="other"> <see cref="ISlot"/> The slot need to be unlinked </param>
        public void Unlink(ISlot other);

        /// <summary>
        /// Pull data from the linked slot to current slot.
        /// </summary>
        /// <param name="OnPullData">The action to execute when pulling data.</param>
        public void PullData(Action<ISlot> OnPullData);

        /// <summary>
        /// Push data from the current slot to linked slot.
        /// </summary>
        /// <param name="OnPushData">The action to execute when pushing data.</param>
        public void PushData(Action<ISlot> OnPushData);

        /// <summary>
        /// Send data to the slot.
        /// </summary>
        /// <param name="data"> The data want to send </param>
        public void ReceiveData(object data);
    }
}
