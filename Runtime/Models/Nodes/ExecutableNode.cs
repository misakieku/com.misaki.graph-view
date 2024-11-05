using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public abstract class ExecutableNode : DataNode, ISlotContainer, IExecutable
    {
        [SerializeField]
        private List<Slot> _inputs = new();
        [SerializeField]
        private List<Slot> _outputs = new();

        public ReadOnlyCollection<Slot> Inputs => _inputs.AsReadOnly();
        public ReadOnlyCollection<Slot> Outputs => _outputs.AsReadOnly();

        private bool _isExecuted;

        public Action OnExecutionStarted;
        public Action OnExecutionCompleted;
        public Action OnExecutionFailed;
        public Action OnExecuteFlagCleared;

        public override void Initialize(GraphObject graph)
        {
            graphObject = graph;

            InitializeSlot();
        }

        public virtual void InitializeSlot()
        {
            var type = GetType();
            var fields = type.GetFields(ConstResource.NODE_FIELD_BINDING_FLAGS);

            var inputSlotIndex = 0;
            var outputSlotIndex = 0;
            foreach (var field in fields)
            {
                var inputAttribute = field.GetCustomAttribute<NodeInputAttribute>();
                if (inputAttribute != null)
                {
                    var inputSlot = new Slot(this, new SlotData
                    {
                        slotName = field.Name,
                        nodeID = Id,
                        slotIndex = inputSlotIndex++,
                        direction = SlotDirection.Input,
                        valueType = field.FieldType.FullName
                    });
                    AddSlot(inputSlot);

                    continue;
                }

                var outputAttribute = field.GetCustomAttribute<NodeOutputAttribute>();
                if (outputAttribute != null)
                {
                    var outputSlot = new Slot(this, new SlotData
                    {
                        slotName = field.Name,
                        nodeID = Id,
                        slotIndex = outputSlotIndex++,
                        direction = SlotDirection.Output,
                        valueType = field.FieldType.FullName
                    });
                    AddSlot(outputSlot);
                }
            }
        }

        /// <inheritdoc />
        public void AddSlot(Slot slot)
        {
            switch (slot.slotData.direction)
            {
                case SlotDirection.Input:
                    _inputs.Add(slot);
                    break;
                case SlotDirection.Output:
                    _outputs.Add(slot);
                    break;
            }
        }

        /// <inheritdoc />
        public void RemoveSlot(Slot slot)
        {
            switch (slot.slotData.direction)
            {
                case SlotDirection.Input:
                    _inputs.Remove(slot);
                    break;
                case SlotDirection.Output:
                    _outputs.Remove(slot);
                    break;
            }
        }

        /// <inheritdoc />
        public Slot GetSlot(int index, SlotDirection direction)
        {
            return direction switch
            {
                SlotDirection.Input => Inputs[index],
                SlotDirection.Output => Outputs[index],
                _ => null
            };
        }

        /// <inheritdoc />
        public void UnlinkAllSlots()
        {
            foreach (var input in Inputs)
            {
                input.UnlinkAll();
                graphObject.RemoveAllConnectionsForSlot(input);
            }

            foreach (var output in Outputs)
            {
                output.UnlinkAll();
                graphObject.RemoveAllConnectionsForSlot(output);
            }
        }

        /// <inheritdoc />
        public void Execute()
        {
            if (_isExecuted)
            {
                return;
            }

            OnExecutionStarted?.Invoke();
            Inputs.PullData(OnPullData);

            if (!graphObject.GraphProcessor.IsRunning)
            {
                return;
            }

            if (!OnExecute())
            {
                graphObject.GraphProcessor.Break();
                OnExecutionFailed?.Invoke();
                return;
            }

            Outputs.PushData(OnPushData);

            _isExecuted = true;
            OnExecutionCompleted?.Invoke();
        }

        /// <inheritdoc />
        public void ClearExecutionFlag()
        {
            _isExecuted = false;
            OnExecuteFlagCleared?.Invoke();
        }

        protected virtual void OnPullData(Slot input)
        {
        }

        protected virtual void OnPushData(Slot output)
        {
        }

        /// <summary>
        /// The execution logic of the node.
        /// </summary>
        /// <returns> <see cref="bool"/> Return true if the execution is success, otherwise false </returns>
        protected abstract bool OnExecute();
    }
}