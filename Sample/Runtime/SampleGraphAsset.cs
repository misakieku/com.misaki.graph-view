using UnityEngine;

namespace Misaki.GraphView.Sample
{
    [CreateAssetMenu(fileName = "GraphAsset", menuName = "Scriptable Objects/GraphAsset")]
    public class SampleGraphAsset : GraphObject
    {
        public override void Execute()
        {
            Nodes.ClearAllExecuteFlag();
            
            foreach (var node in Nodes)
            {
                if (node is OutputNode outputNode)
                {
                    outputNode.Execute();
                }
            }
        }
    }
}