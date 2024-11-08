using System.Collections.Generic;

namespace Misaki.GraphView
{
    public interface ISlot
    {
        public SlotData SlotData
        {
            get;
        }

        public List<SlotData> LinkedSlotDatas
        {
            get;
        }

        public bool IsLinked
        {
            get;
        }

        public DataNode Owner
        {
            get;
        }

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
        /// Send data to the slot.
        /// </summary>
        /// <param name="data"> The data want to send </param>
        public void ReceiveData(object data);
    }
}
