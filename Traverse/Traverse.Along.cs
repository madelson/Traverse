using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    /// <summary>
    /// Contains utility methods for traversing graph-like datastructures.
    /// 
    /// See https://github.com/madelson/Traverse
    /// </summary>
    public static partial class Traverse
    {
        /// <summary>
        /// Enumerates the implicit sequence starting from <paramref name="root"/>
        /// and following the chain of <paramref name="next"/> calls until a null value
        /// is encountered. For example, this can be used to traverse a chain of exceptions:
        /// <code>
        ///     var exceptions = Traverse.Along(exception, e => e.InnerException);
        /// </code>
        /// </summary>
        public static IEnumerable<T> Along<T>(T? root, Func<T, T?> next)
            where T : class
        {
            if (next == null) { throw new ArgumentNullException(nameof(next)); }

            return AlongIterator();

            IEnumerable<T> AlongIterator()
            {
                for (T? node = root; node != null; node = next(node))
                {
                    yield return node;
                }
            }
        }
    }
}
