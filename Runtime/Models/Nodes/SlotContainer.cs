using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Misaki.GraphView
{
    [Serializable]
    public abstract class SlotContainer
    {
        [SerializeField]
        private List<Slot> _inputs = new ();
        [SerializeField]
        private List<Slot> _outputs = new ();
        
        public ReadOnlyCollection<Slot> Inputs => _inputs.AsReadOnly();
        public ReadOnlyCollection<Slot> Outputs => _outputs.AsReadOnly();
        
        public void AddInput(Slot input)
        {
            _inputs.Add(input);
        }
        
        public void AddOutput(Slot output)
        {
            _outputs.Add(output);
        }
        
        public void ClearInputs()
        {
            _inputs.Clear();
        }
        
        public void ClearOutputs()
        {
            _outputs.Clear();
        }
    }
}