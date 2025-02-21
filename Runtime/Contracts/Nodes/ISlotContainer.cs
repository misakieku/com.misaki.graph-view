﻿namespace Misaki.GraphView
{
    public interface ISlotContainer
    {
        /// <summary>
        /// Get the slot by the index and direction.
        /// </summary>
        /// <param name="index"> <see cref="int"/> Index of the slot</param>
        /// <param name="direction"> <see cref="SlotDirection"/> Direction of the slot </param>
        /// <returns> <see cref="ISlot"/> The slot that matches the index and direction </returns>
        public ISlot GetSlot(int index, SlotDirection direction);

        /// <summary>
        /// Unlink all the slots of the node.
        /// </summary>
        public void UnlinkAllSlots();
    }
}