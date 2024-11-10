namespace Misaki.GraphView.Sample
{
    public abstract class BackTraceExecutableNode : ExecutableNode
    {
        protected override void OnPullData(ISlot input)
        {
            if (input.LinkedSlotDatas.Count == 0)
            {
                return;
            }

            var outputNode = GraphObject.GetNode(input.LinkedSlotDatas[0].nodeID);
            if (outputNode is IExecutable executable)
            {
                executable.Execute();
            }
        }
    }
}