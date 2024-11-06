namespace Misaki.GraphView.Editor
{
    public interface IDataNodeView<T> : IDataNodeView
    {
        /// <summary>
        /// The data node that the view represents.
        /// </summary>
        public T DataNode
        {
            get;
        }
    }

    public interface IDataNodeView
    {
        /// <summary>
        /// Get the data node that the view represents.
        /// </summary>
        /// <returns></returns>
        public DataNode GetDataNode();
    }
}