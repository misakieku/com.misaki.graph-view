﻿using System;
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
    public class ExecutableNodeView : Node, IInspectable, IPortContainer, IDataNodeView<ExecutableNode>
    {
        private readonly ExecutableNode _dataNode;
        private readonly Type _nodeType;
        private readonly NodeInfoAttribute _nodeInfo;

        private readonly List<Port> _inputPorts = new();
        private readonly List<Port> _outputPorts = new();

        private readonly IPortColorManager _portColorManager;
        private readonly SerializedObject _serializedObject;

        private readonly VisualElement _logContainer = new();

        public ExecutableNode DataNode => _dataNode;
        public DataNode GetDataNode() => _dataNode;

        public Action<IInspectable> OnItemSelected
        {
            get; set;
        }

        public string InspectorName => _nodeInfo.Name ?? _nodeType.Name;

        public ExecutableNodeView(ExecutableNode dataNode, SerializedObject serializedObject, IPortColorManager portColorManager, ILogger logger)
        {
            if (dataNode == null)
            {
                return;
            }

            _dataNode = dataNode;
            _portColorManager = portColorManager;
            _serializedObject = serializedObject;

            userData = dataNode;

            _nodeType = dataNode.GetType();
            _nodeInfo = _nodeType.GetCustomAttribute<NodeInfoAttribute>();

            name = _nodeInfo.Name ?? _nodeType.Name;
            title = _nodeInfo.Name ?? _nodeType.Name;
            style.minWidth = 104.0f;

            // Add the category as a class to the node so that we can style the node based on the category
            var depths = _nodeInfo.Category.Split('/').ToList();
            depths.Add(_nodeInfo.Name);
            foreach (var depth in depths)
            {
                AddToClassList(depth.ToLower().Replace(" ", "-"));
            }

            var inputs = _nodeType.GetProperty(nameof(ExecutableNode.Inputs));

            if (inputs != null)
            {
                var inputSlots = (IList<ISlot>)inputs.GetValue(_dataNode);

                if (inputSlots == null || inputSlots.Count == 0)
                {
                    inputContainer.style.display = DisplayStyle.None;
                    this.Query<VisualElement>("divider", "vertical").AtIndex(0).style.display = DisplayStyle.None;
                }
                else
                {
                    foreach (var slot in inputSlots)
                    {
                        CreateInputPort(slot);
                    }
                }
            }

            var outputs = _nodeType.GetProperty(nameof(ExecutableNode.Outputs));

            if (outputs != null)
            {
                var outputSlots = (IList<ISlot>)outputs.GetValue(_dataNode);
                if (outputSlots == null || outputSlots.Count == 0)
                {
                    outputContainer.style.display = DisplayStyle.None;
                    this.Query<VisualElement>("divider", "vertical").AtIndex(0).style.display = DisplayStyle.None;
                }
                else
                {
                    foreach (var slot in outputSlots)
                    {
                        CreateOutputPort(slot);
                    }
                }
            }

            _logContainer.style.position = Position.Absolute;
            _logContainer.style.top = 8;
            Add(_logContainer);

            _dataNode.OnExecuteFlagCleared += OnExecuteFlagCleared;
            _dataNode.OnExecutionFailed += () => AddToClassList("node-execution-failed");
            if (logger != null)
            {
                logger.OnLog += CreateLogElement;
            }
        }

        private void CreateLogElement(DataNode node, string message, LogType type)
        {
            if (node.Id != _dataNode.Id)
            {
                return;
            }

            _logContainer.style.left = layout.width;

            var logIcon = new Image()
            {
                image = type switch
                {
                    LogType.Error => EditorGUIUtility.IconContent("console.erroricon.sml").image,
                    LogType.Warning => EditorGUIUtility.IconContent("console.warnicon.sml").image,
                    _ => EditorGUIUtility.IconContent("console.infoicon.sml").image
                },
                tooltip = message,
                style =
                {
                    width = 24,
                    height = 24,

                    borderBottomLeftRadius = 4,
                    borderTopRightRadius = 4,
                    borderTopLeftRadius = 4,
                    borderBottomRightRadius = 4,

                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    paddingBottom = 1,
                    paddingTop = 1,
                    paddingLeft = 3,
                    paddingRight = 3,
                    position = Position.Absolute,
                    backgroundColor = type switch
                    {
                        LogType.Error => new Color(0.4f, 0.2f, 0.2f),
                        LogType.Warning => new Color(0.4f, 0.35f, 0.2f),
                        _ => new Color(0.4f, 0.4f, 0.4f)
                    }
                }
            };

            _logContainer.Add(logIcon);
        }

        private void OnExecuteFlagCleared()
        {
            _logContainer.Clear();
            RemoveFromClassList("node-execution-failed");
        }

        private void CreateInputPort(ISlot slot)
        {
            var valueType = Type.GetType(slot.SlotData.valueType);
            var inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, valueType);

            inputPort.portName = ObjectNames.NicifyVariableName(slot.SlotData.slotName);
            inputPort.portType = valueType;
            inputPort.tooltip = valueType.FullName;
            inputPort.userData = slot;
            if (_portColorManager != null && _portColorManager.TryGetColor(valueType, out var portColor))
            {
                inputPort.portColor = portColor;
            }

            inputContainer.Add(inputPort);
            _inputPorts.Add(inputPort);
        }

        private void CreateOutputPort(ISlot slot)
        {
            var valueType = Type.GetType(slot.SlotData.valueType);
            var outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, valueType);

            outputPort.portName = ObjectNames.NicifyVariableName(slot.SlotData.slotName);
            outputPort.tooltip = valueType.FullName;
            outputPort.portType = valueType;
            outputPort.userData = slot;
            if (_portColorManager != null && _portColorManager.TryGetColor(valueType, out var portColor))
            {
                outputPort.portColor = portColor;
            }

            outputContainer.Add(outputPort);
            _outputPorts.Add(outputPort);
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

        public Port GetPort(int index, Direction direction)
        {
            return direction switch
            {
                Direction.Input => _inputPorts[index],
                Direction.Output => _outputPorts[index],
                _ => null
            };
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
            var fields = _nodeType.GetFields(ConstResource.NODE_FIELD_BINDING_FLAGS).Where(f => Attribute.IsDefined(f, typeof(InspectorInputAttribute))).ToArray();

            if (fields.Length == 0)
            {
                var label = new Label("No properties to display.");
                root.Add(label);

                return root;
            }

            var i = graphObject.Nodes.IndexOf(_dataNode);
            var nodeProperty = _serializedObject.FindProperty("_nodes")?.GetArrayElementAtIndex(i);
            foreach (var field in fields)
            {
                var serializedProperty = nodeProperty?.FindPropertyRelative(field.Name);

                if (serializedProperty == null)
                {
                    continue;
                }

                var inspectorInputAttribute = field.GetCustomAttribute<InspectorInputAttribute>();
                var propertyName = inspectorInputAttribute.Name ?? ObjectNames.NicifyVariableName(field.Name);
                var connectionBinding = inspectorInputAttribute.ConnectionBinding;

                var connectionField = string.IsNullOrEmpty(connectionBinding) ?
                    field
                    : _nodeType.GetFields(ConstResource.NODE_FIELD_BINDING_FLAGS).FirstOrDefault(f => f.Name == connectionBinding);

                if (connectionField != null)
                {
                    if (_dataNode.Inputs.FirstOrDefault(x => x.SlotData.slotName == connectionField.Name) is { } inputSlot)
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

        protected VisualElement CreateFieldForConnectedSlot(ISlot slot, string propertyName)
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