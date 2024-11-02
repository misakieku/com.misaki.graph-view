using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Misaki.GraphView.Editor
{
    public class EditorNodeView : Node, IInspectable, IPortContainer
    {
        private readonly BaseNode _dataNode;
        private readonly Type _nodeType;
        private readonly NodeInfoAttribute _nodeInfo;
        private readonly List<Port> _inputPorts = new ();
        private readonly List<Port> _outputPorts = new ();
        
        private readonly IPortColorManager _portColorManager;
        private readonly SerializedObject _serializedObject;
        
        public BaseNode DataNode => _dataNode;
        public List<Port>  InputPorts => _inputPorts;
        public List<Port>  OutputPorts => _outputPorts;
        
        public Action<IInspectable> OnItemSelected { get; set; }

        public string InspectorName => _nodeInfo.Name ?? _nodeType.Name;

        public EditorNodeView(BaseNode dataNode, SerializedObject serializedObject, IPortColorManager portColorManager)
        {
            _dataNode = dataNode;
            _portColorManager = portColorManager;
            _serializedObject = serializedObject;
            
            userData = dataNode;
            
            _nodeType = dataNode.GetType();
            _nodeInfo = _nodeType.GetCustomAttribute<NodeInfoAttribute>();
            
            name = _nodeInfo.Name ?? _nodeType.Name;
            title = _nodeInfo.Name ?? _nodeType.Name;
            
            // Add the category as a class to the node so that we can style the node based on the category
            var depths = _nodeInfo.Category.Split('/').ToList();
            depths.Add(_nodeInfo.Name);
            foreach (var depth in depths)
            {
                AddToClassList(depth.ToLower().Replace(" ", "-"));
            }
            
            var inputs = _nodeType.GetProperty(nameof(BaseNode.Inputs));

            if (inputs != null)
            {
                var inputSlots = (IList<Slot>)inputs.GetValue(_dataNode);
                
                if (inputSlots == null || inputSlots.Count == 0)
                {
                    inputContainer.style.display = DisplayStyle.None;
                }
                else
                {
                    foreach (var slot in inputSlots)
                    {
                        CreateInputPort(slot);
                    }
                }
            }

            var outputs = _nodeType.GetProperty(nameof(BaseNode.Outputs));

            if (outputs != null)
            {
                var outputSlots = (IList<Slot>)outputs.GetValue(_dataNode);
                if (outputSlots == null || outputSlots.Count == 0)
                {
                    outputContainer.style.display = DisplayStyle.None;
                }
                else
                {
                    foreach (var slot in outputSlots)
                    {
                        CreateOutputPort(slot);
                    }
                }
            }
        }

        private void CreateInputPort(Slot slot)
        {
            var valueType = Type.GetType(slot.slotData.valueType);
            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, valueType);

            inputPort.portName = ObjectNames.NicifyVariableName(slot.slotData.slotName);
            inputPort.portType = valueType;
            inputPort.userData = slot;
            if (_portColorManager != null && _portColorManager.TryGetColor(valueType, out var portColor))
            {
                inputPort.portColor = portColor;
            }

            inputContainer.Add(inputPort);
            _inputPorts.Add(inputPort);
        }

        private void CreateOutputPort(Slot slot)
        {
            var valueType = Type.GetType(slot.slotData.valueType);
            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, valueType);

            outputPort.portName = ObjectNames.NicifyVariableName(slot.slotData.slotName);
            outputPort.portType = valueType;
            outputPort.userData = slot;
            if (_portColorManager != null && _portColorManager.TryGetColor(valueType, out var portColor))
            {
                outputPort.portColor = portColor;
            }

            outputContainer.Add(outputPort);
            _outputPorts.Add(outputPort);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            _dataNode.position = newPos;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnItemSelected?.Invoke(this);
        }
        
        public override void OnUnselected()
        {
            base.OnUnselected();
            OnItemSelected?.Invoke(null);
        }

        /// <summary>
        /// Displays the inspector for the node.
        /// </summary>
        public virtual VisualElement CreateInspector()
        {
            var root = new VisualElement();
            
            if (_serializedObject.targetObject is not GraphObject graphObject)
            {
                return root;
            }

            // Use reflection to get the inspector input fields
            var fields = _nodeType.GetFields().Where(f => f.GetCustomAttribute<InspectorInputAttribute>() != null).ToArray();

            if (fields.Length == 0)
            {
                var label = new Label("No properties to display.");
                root.Add(label);

                return root;
            }

            foreach (var field in fields)
            {
                var i = graphObject.Nodes.IndexOf(_dataNode);
                var serializedProperty = _serializedObject.FindProperty("_nodes")?.GetArrayElementAtIndex(i)?.FindPropertyRelative(field.Name);
                
                if (serializedProperty == null)
                {
                    continue;
                }

                if (field.GetCustomAttribute<NodeOutputAttribute>() is not null)
                {
                    continue;
                }
                
                var propertyName = field.GetCustomAttribute<InspectorInputAttribute>().Name ?? ObjectNames.NicifyVariableName(field.Name);

                if (field.GetCustomAttribute<NodeInputAttribute>() is not null)
                {
                    if (_dataNode.Inputs.FirstOrDefault(x => x.slotData.slotName == field.Name) is { } inputSlot)
                    {
                        if (inputSlot.LinkedSlotData.Count > 0)
                        {
                            root.Add(CreateFieldForConnectedSlot(inputSlot, propertyName));
                            continue;
                        }
                    }
                }
                
                var inputField = new PropertyField(serializedProperty, propertyName);
                inputField.Bind(_serializedObject);
                
                root.Add(inputField);
            }
            
            return root;
        }
        
        protected VisualElement CreateFieldForConnectedSlot(Slot slot, string propertyName)
        {
            var root = new VisualElement()
            {
                style =
                {
                    height = 21,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingBottom = 1,
                    paddingTop = 1,
                    marginLeft = 3,
                    marginRight = 3
                }
            };
            
            var label = new Label(propertyName)
            {
                style =
                {
                    width = 120
                }
            };
            label.AddToClassList("unity-base-field__label");
            
            var value = new Label($"Connected to {ObjectNames.NicifyVariableName(slot.LinkedSlotData[0].slotName)}");
            value.AddToClassList("unity-base-field__input");

            root.Add(label);
            root.Add(value);
            return root;
        }
    }
}