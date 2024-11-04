namespace Misaki.GraphView.Sample
{
    public abstract class BackTraceExecutableNode : ExecutableNode
    {
        protected override void OnPullData(Slot input)
        {
            if (input.LinkedSlotData.Count == 0)
            {
                return;
            }
            
            var outputNode = GraphObject.GetNode(input.LinkedSlotData[0].nodeID);
            if (outputNode is IExecutable executable)
            {
                executable.Execute();
            }
        }
    }
}