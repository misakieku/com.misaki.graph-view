using System.Collections.Generic;

namespace Misaki.GraphView
{
    [System.Serializable]
    public class SerializationData
    {
        public List<string> serializedFields = new (); 
        public List<string> serializedValues = new ();
    }
}