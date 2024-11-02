using System;
using UnityEngine;

namespace Misaki.GraphView
{
    /// <summary>
    ///     Represents a connection between two connection ports.
    /// </summary>
    [Serializable]
    public struct SlotConnection : IEquatable<SlotConnection>
    {
        [SerializeField] 
        private SlotData _inputSlotData;

        [SerializeField] 
        private SlotData _outputSlotData;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SlotConnection" /> struct.
        /// </summary>
        /// <param name="inputSlotData">The input connection port.</param>
        /// <param name="outputSlotData">The output connection port.</param>
        public SlotConnection(SlotData inputSlotData, SlotData outputSlotData)
        {
            _inputSlotData = inputSlotData;
            _outputSlotData = outputSlotData;
        }

        public SlotData InputSlotData => _inputSlotData;
        public SlotData OutputSlotData => _outputSlotData;

        public bool Equals(SlotConnection other)
        {
            return _inputSlotData.Equals(other._inputSlotData) && _outputSlotData.Equals(other._outputSlotData);
        }

        public override bool Equals(object obj)
        {
            return obj is SlotConnection other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(InputSlotData, OutputSlotData);
        }
    }
}