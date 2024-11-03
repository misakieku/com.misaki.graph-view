namespace Misaki.GraphView.Sample
{
    public abstract class BackTraceNode : SlotContainerNode
    {
        protected override void OnPullData(Slot input)
        {
            if (input.LinkedSlotData.Count == 0)
            {
                return;
            }
            
            var outputNode = GraphObject.GetNode(input.LinkedSlotData[0].nodeID);
            outputNode.Execute();
        }
    }
}