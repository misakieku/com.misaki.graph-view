using System;
using System.Reflection;
using UnityEngine;

namespace Misaki.GraphView
{
    [Serializable]
    public abstract class BaseNode : SlotContainer
    {
        [SerializeField] 
        private GraphObject _graphObject;
        [SerializeField] 
        private string _id = Guid.NewGuid().ToString();
        
        private bool _isExecuted;

        public Rect position;

        public GraphObject GraphObject => _graphObject;
        public string Id => _id;

        /// <summary>
        /// Initialize the node with the graph object, this method is called when the node is added to the graph.
        /// </summary>
        public virtual void Initialize(GraphObject graph)
        {
            _graphObject = graph;

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
                    AddInput(inputSlot);

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
                    AddOutput(outputSlot);
                }
            }
        }

        /// <summary>
        /// Unload the node from the graph, this method is called when the node is removed from the graph.
        /// </summary>
        public virtual void UnLoad()
        {
        }

        /// <summary>
        /// Get the slot by the index and direction.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Slot GetSlot(int index, SlotDirection direction)
        {
            return direction switch
            {
                SlotDirection.Input => Inputs[index],
                SlotDirection.Output => Outputs[index],
                _ => null
            };
        }

        /// <summary>
        /// Unlink all the slots of the node.
        /// </summary>
        public void UnlinkAllSlots()
        {
            foreach (var input in Inputs)
            {
                input.UnlinkAll();
                _graphObject.RemoveAllConnectionsForSlot(input);
            }

            foreach (var output in Outputs)
            {
                output.UnlinkAll();
                _graphObject.RemoveAllConnectionsForSlot(output);
            }
        }

        /// <summary>
        /// Execute the node.
        /// </summary>
        public void Execute()
        {
            if (_isExecuted)
            {
                return;
            }
            
            PullData();
            OnExecute();
            PushData();
            
            _isExecuted = true;
        }

        private void PullData()
        {
            foreach (var input in Inputs)
            {
                var property = GetType().GetField(input.slotData.slotName, ConstResource.NODE_FIELD_BINDING_FLAGS);
                if (property == null) continue;

                OnPullData(input);

                if (input.LinkedSlotData.Count == 0)
                {
                    continue;
                }
                
                property.SetValue(this, input.value);
            }
        }

        protected virtual void OnPullData(Slot input)
        {
        }

        private void PushData()
        {
            foreach (var output in Outputs)
            {
                var property = GetType().GetField(output.slotData.slotName, ConstResource.NODE_FIELD_BINDING_FLAGS);
                if (property == null) continue;

                OnPushData(output);

                output.value = property.GetValue(this);
                foreach (var slotData in output.LinkedSlotData)
                {
                    var slot = _graphObject.GetNode(slotData.nodeID).GetSlot(slotData.slotIndex, slotData.direction);

                    if (slotData.valueType == output.slotData.valueType || output.slotData.valueType == typeof(object).FullName)
                    {
                        slot.ReceiveData(output.value);
                    }
                    else if (_graphObject.ValueConverterManager != null && _graphObject.ValueConverterManager.TryConvert(output.slotData.GetValueType(),
                                 slotData.GetValueType(), output.value, out var data))
                    {
                        slot.ReceiveData(data);
                    }
                }
            }
        }

        protected virtual void OnPushData(Slot output)
        {
        }
        
        public bool IsExecuted()
        {
            return _isExecuted;
        }
        
        public void ClearExecuteFlag()
        {
            _isExecuted = false;
        }

        /// <summary>
        ///     The execution logic of the node.
        /// </summary>
        protected abstract void OnExecute();
    }
}