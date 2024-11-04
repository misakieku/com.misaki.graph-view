namespace Misaki.GraphView
{
    public interface IExecutable
    {
        /// <summary>
        /// Execute the node.
        /// </summary>
        public void Execute();
        
        /// <summary>
        /// Clear the execution flag.
        /// </summary>
        public void ClearExecutionFlag();
    }
}