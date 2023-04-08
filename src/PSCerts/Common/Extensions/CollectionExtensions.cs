using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PSCerts
{
    public static class CollectionExtensions
    {
        public static List<T> AsList<T>(this ICollection collection)
        {
            return collection.Cast<T>().ToList();
        }
    }
}
