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
        [SerializeReference]
        private List<ISlot> _inputs = new();
        [SerializeReference]
        private List<ISlot> _outputs = new();

        private bool _isExecuted;

        public ReadOnlyCollection<ISlot> Inputs => _inputs.AsReadOnly();
        public ReadOnlyCollection<ISlot> Outputs => _outputs.AsReadOnly();

        public bool IsExecuted => _isExecuted;
        public ILogger Logger => GraphObject.Logger;

        public Action OnExecutionStarted;
        public Action OnExecutionCompleted;
        public Action OnExecutionFailed;
        public Action OnExecuteFlagCleared;

        public override void Initialize(GraphObject graph)
        {
            base.Initialize(graph);
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
                    var inputSlot = new Slot(this,
                        new SlotData(field.Name, Id, inputSlotIndex++, SlotDirection.Input, field.FieldType));
                    _inputs.Add(inputSlot);
                }

                var outputAttribute = field.GetCustomAttribute<NodeOutputAttribute>();
                if (outputAttribute != null)
                {
                    var outputSlot = new Slot(this,
                        new SlotData(field.Name, Id, outputSlotIndex++, SlotDirection.Output, field.FieldType));
                    _outputs.Add(outputSlot);
                }
            }
        }

        /// <inheritdoc />
        public ISlot GetSlot(int index, SlotDirection direction)
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
                GraphObject.RemoveAllConnectionsForSlot(input);
            }

            foreach (var output in Outputs)
            {
                output.UnlinkAll();
                GraphObject.RemoveAllConnectionsForSlot(output);
            }
        }

        /// <inheritdoc />
        public void Execute()
        {
            if (_isExecuted)
            {
                return;
            }

            OnPreExecute();

            OnExecutionStarted?.Invoke();
            Inputs.PullData(OnPullData);

            if (!GraphObject.GraphProcessor.IsRunning)
            {
                return;
            }

            if (!OnExecute())
            {
                GraphObject.GraphProcessor.Break();
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

        protected virtual void OnPullData(ISlot input)
        {
        }

        protected virtual void OnPushData(ISlot output)
        {
        }

        protected virtual void OnPreExecute()
        {
        }

        /// <summary>
        /// The execution logic of the node.
        /// </summary>
        /// <returns> <see cref="bool"/> Return true if the execution is success, otherwise false </returns>
        protected abstract bool OnExecute();
    }
}