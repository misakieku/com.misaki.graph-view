namespace Misaki.GraphView
{
    public interface ISlotContainer
    {
        /// <summary>
        /// Add the slot to the container.
        /// </summary>
        /// <param name="slot"> <see cref="Slot"/> The slot want to add </param>
        public void AddSlot(Slot slot);
        
        /// <summary>
        /// Remove the slot from the container.
        /// </summary>
        /// <param name="slot"> <see cref="Slot"/> The slot want to remove </param>
        public void RemoveSlot(Slot slot);

        /// <summary>
        /// Get the slot by the index and direction.
        /// </summary>
        /// <param name="index"> <see cref="int"/> Index of the slot</param>
        /// <param name="direction"> <see cref="SlotDirection"/> Direction of the slot </param>
        /// <returns> <see cref="Slot"/> The slot that matches the index and direction </returns>
        public Slot GetSlot(int index, SlotDirection direction);

        /// <summary>
        /// Unlink all the slots of the node.
        /// </summary>
        public void UnlinkAllSlots();
    }
}