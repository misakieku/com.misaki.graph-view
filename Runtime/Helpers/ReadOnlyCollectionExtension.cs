using System;
using System.Collections.ObjectModel;

namespace Misaki.GraphView
{
    public static class ReadOnlyCollectionExtension
    {
        public static int FindIndex<T>(this ReadOnlyCollection<T> collection, Predicate<T> match)
        {
            for (var i = 0; i < collection.Count; i++)
            {
                if (match(collection[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}