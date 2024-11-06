using System;

namespace Misaki.GraphView
{
    [Serializable]
    public class RelayNode : DataNode, ISlotContainer, IExecutable
    {
        private Slot _inputSlot;
        private Slot _outputSlot;

        private bool _isExecuted;

        public string portValueType;

        public override void Initialize(GraphObject graph)
        {
            graphObject = graph;

            _inputSlot = new(this, new()
            {
                slotName = "Input",
                nodeID = Id,
                slotIndex = 0,
                direction = SlotDirection.Input,
                valueType = typeof(object).FullName
            });

            _outputSlot = new(this, new()
            {
                slotName = "Output",
                nodeID = Id,
                slotIndex = 0,
                direction = SlotDirection.Output,
                valueType = typeof(object).FullName
            });

            portValueType = typeof(object).FullName;
        }

        public void AddSlot(Slot slot)
        {
        }

        public void RemoveSlot(Slot slot)
        {
        }

        public Slot GetSlot(int index, SlotDirection direction)
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

            _outputSlot.ReceiveData(_inputSlot.value);
            _outputSlot.PushData(null);

            _isExecuted = true;
        }

        public void ClearExecutionFlag()
        {
            _isExecuted = false;
        }
    }
}