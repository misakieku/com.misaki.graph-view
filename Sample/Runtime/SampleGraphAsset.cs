using UnityEngine;

namespace Misaki.GraphView.Sample
{
    [CreateAssetMenu(fileName = "GraphAsset", menuName = "Scriptable Objects/GraphAsset")]
    public class SampleGraphAsset : GraphObject
    {
        private readonly Logger _logger = new Logger();
        private readonly BackTraceGraphProcessor _processor = new BackTraceGraphProcessor();

        public override ILogger Logger => _logger;
        public override IGraphProcessor GraphProcessor => _processor;
    }
}