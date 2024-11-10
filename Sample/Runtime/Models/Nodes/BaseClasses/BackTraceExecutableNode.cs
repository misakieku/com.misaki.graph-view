namespace Misaki.GraphView.Sample
{
    public abstract class BackTraceExecutableNode : ExecutableNode
    {
        protected override void OnPullData(ISlot input)
        {
            foreach (var linkedSlotData in input.LinkedSlotData)
            {
                var outputNode = GraphObject.GetNode(linkedSlotData.nodeID);
                if (outputNode is IExecutable executable)
                {
                    executable.Execute();
                }
            }
        }
    }
}