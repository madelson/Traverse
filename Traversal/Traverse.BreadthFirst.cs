using System;
using System.Collections.Generic;
using System.Text;

namespace Medallion.Collections
{
    public static partial class Traverse
    {
        /// <summary>
        /// Enumerates the implicit tree described by <paramref name="root"/> and <paramref name="children"/>
        /// in a breadth-first manner. For example, this could be used to enumerate the exceptions of an
        /// <see cref="AggregateException"/>:
        /// <code>
        ///     var allExceptions = Traverse.BreadthFirst((Exception)new AggregateException(), e => (e as AggregateException)?.InnerExceptions ?? Enumerable.Empty&lt;Exception&gt;());
        /// </code>
        /// </summary>
        public static IEnumerable<T> BreadthFirst<T>(T root, Func<T, IEnumerable<T>> children)
        {
            if (children == null) { throw new ArgumentNullException(nameof(children)); }

            return BreadthFirstIterator();

            IEnumerable<T> BreadthFirstIterator()
            {
                // note that this implementation has two nice properties which require a bit more complexity
                // in the code: (1) children are yielded in order and (2) child enumerators are fully lazy

                yield return root;
                var queue = new Queue<IEnumerable<T>>();
                queue.Enqueue(children(root));

                do
                {
                    foreach (var child in queue.Dequeue())
                    {
                        yield return child;
                        queue.Enqueue(children(child));
                    }
                }
                while (queue.Count > 0);
            }
        }
    }
}
