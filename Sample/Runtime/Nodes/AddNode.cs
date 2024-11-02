#if UNITY_EDITOR
using Misaki.GraphView.Editor;
#endif

namespace Misaki.GraphView.Sample
{
    [NodeInfo("Add", "Math")]
    public class AddNode : BackTraceBaseNode
    {
        [NodeInput]
#if UNITY_EDITOR
        [InspectorInput]
#endif
        public float a;

        [NodeInput]
#if UNITY_EDITOR
        [InspectorInput]
#endif
        public float b;

        [NodeOutput] 
        private float _result;

        protected override void OnExecute()
        {
            _result = a + b;
        }
    }
}