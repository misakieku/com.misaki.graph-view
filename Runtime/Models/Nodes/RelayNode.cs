using System;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public class RelayNode : DataNode, ISlotContainer
    {
        [SerializeField]
        private ProxySlot _inputSlot;
        [SerializeField]
        private ProxySlot _outputSlot;

        public string portValueType;

        public override void Initialize(GraphObject graph)
        {
            base.Initialize(graph);

            _inputSlot = new(this, new SlotData
            {
                slotName = "Input",
                nodeID = Id,
                slotIndex = 0,
                direction = SlotDirection.Input,
                valueType = typeof(object).FullName
            });
            _outputSlot = new(this, new SlotData
            {
                slotName = "Output",
                nodeID = Id,
                slotIndex = 0,
                direction = SlotDirection.Output,
                valueType = typeof(object).FullName
            });

            portValueType = typeof(object).FullName;
        }

        /// <summary>
        /// Bind the slot to the relay node.
        /// </summary>
        /// <param name="slot"> <see cref="ISlot"/> The slot want to bind to current relay node </param>
        public void BindSlot(ISlot slot)
        {
            switch (slot.SlotData.direction)
            {
                case SlotDirection.Input:
                    _inputSlot.Bind(slot);
                    break;
                case SlotDirection.Output:
                    _outputSlot.Bind(slot);
                    break;
            }
        }

        /// <summary>
        /// Unbind the slot from the relay node.
        /// </summary>
        /// <param name="direction"> The direction of the slot </param>
        public void UnbindSlot(SlotDirection direction)
        {
            switch (direction)
            {
                case SlotDirection.Input:
                    _inputSlot.Unbind();
                    break;
                case SlotDirection.Output:
                    _outputSlot.Unbind();
                    break;
            }
        }

        public ISlot GetSlot(int index, SlotDirection direction)
        {
            return direction switch
            {
                SlotDirection.Input => _inputSlot,
                SlotDirection.Output => _outputSlot,
                _ => null
            };
        }

        public void UnlinkAllSlots()
        {
            _inputSlot.UnlinkAll();
            _outputSlot.UnlinkAll();
        }

        public override void Dispose()
        {
            base.Dispose();

            _inputSlot.Unbind();
            _outputSlot.Unbind();
        }
    }
}