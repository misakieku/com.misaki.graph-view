namespace Misaki.GraphView
{
    public interface IGraphProcessor
    {
        public void UpdateComputeOrder();
        
        public void Execute();
    }
}