using System;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public class RelayNode : DataNode, ISlotContainer, IExecutable
    {
        [SerializeField]
        private Slot _inputSlot;
        [SerializeField]
        private Slot _outputSlot;

        private bool _isExecuted;

        public bool IsExecuted => _isExecuted;

        public string portValueType;

        public override void Initialize(GraphObject graph)
        {
            base.Initialize(graph);

            _inputSlot = new Slot(this, new SlotData
            {
                slotName = "Input",
                nodeID = Id,
                slotIndex = 0,
                direction = SlotDirection.Input,
                valueType = typeof(object).FullName
            });

            _outputSlot = new Slot(this, new SlotData
            {
                slotName = "Output",
                nodeID = Id,
                slotIndex = 0,
                direction = SlotDirection.Output,
                valueType = typeof(object).FullName
            });

            portValueType = typeof(object).FullName;
        }

        ///// <summary>
        ///// Bind the slot to the relay node.
        ///// </summary>
        ///// <param name="slot"> <see cref="ISlot"/> The slot want to bind to current relay node </param>
        //public void BindSlot(ISlot slot)
        //{
        //    switch (slot.SlotData.direction)
        //    {
        //        case SlotDirection.Input:
        //            _inputSlot.Bind(slot);
        //            break;
        //        case SlotDirection.Output:
        //            _outputSlot.Bind(slot);
        //            break;
        //    }
        //}

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

        public void Execute()
        {
            if (_isExecuted)
            {
                return;
            }

            _inputSlot.PullData(null);
            _outputSlot.ReceiveData(_inputSlot.Data);
            _outputSlot.PushData(null);

            _isExecuted = true;
        }

        public void ClearExecutionFlag()
        {
            _isExecuted = false;
        }
    }
}