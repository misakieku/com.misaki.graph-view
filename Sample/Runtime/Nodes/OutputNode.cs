using UnityEngine;

namespace Misaki.GraphView.Sample
{
    [NodeInfo("Output Node", "Output")]
    public class OutputNode : BackTraceBaseNode
    {
        [NodeInput]
        private float _input;
        
        protected override bool OnExecute()
        {
            GraphObject.Logger.LogInfo(this, $"{_input}");

            return true;
        }
    }
}