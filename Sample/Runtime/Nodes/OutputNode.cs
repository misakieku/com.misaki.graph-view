using UnityEngine;

namespace Misaki.GraphView.Sample
{
    [NodeInfo("Output Node", "Output")]
    public class OutputNode : BackTraceBaseNode
    {
        [NodeInput]
        private float _input;
        
        protected override void OnExecute()
        {
            Debug.Log(_input);
        }
    }
}