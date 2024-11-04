#if UNITY_EDITOR
using Misaki.GraphView.Editor;
#endif

namespace Misaki.GraphView.Sample
{
    [NodeInfo("Add", "Math")]
    public class Add : BackTraceExecutableNode
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

        protected override bool OnExecute()
        {
            _result = a + b;

            return true;
        }
    }
}