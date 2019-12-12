using System;
using System.Collections;
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
        public static IEnumerable<T> BreadthFirst<T>(T root, Func<T, IEnumerable<T>> children) =>
            BreadthFirstIterator(new SingleRootEnumerable<T>(root), children ?? throw new ArgumentNullException(nameof(children)));

        /// <summary>
        /// Similar to <see cref="BreadthFirst{T}(T, Func{T, IEnumerable{T}})"/>, but traverses from multiple roots
        /// simultaneously.
        /// </summary>
        public static IEnumerable<T> BreadthFirst<T>(IEnumerable<T> roots, Func<T, IEnumerable<T>> children) =>
            BreadthFirstIterator(roots ?? throw new ArgumentNullException(nameof(roots)), children ?? throw new ArgumentNullException(nameof(children)));

        private static IEnumerable<T> BreadthFirstIterator<T>(IEnumerable<T> roots, Func<T, IEnumerable<T>> children)
        {
            // note that this implementation has two nice properties which require a bit more complexity
            // in the code: (1) children are yielded in order and (2) child enumerators are fully lazy

            var queue = new Queue<IEnumerable<T>>();
            queue.Enqueue(roots);

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

        /// <summary>
        /// A minimal single-element <see cref="IEnumerable{T}"/> implementation. Can only be
        /// enumerated once, so this is only used internally. All unnecessary methods throw
        /// <see cref="NotSupportedException"/>
        /// </summary>
        private sealed class SingleRootEnumerable<T> : IEnumerable<T>, IEnumerator<T>
        {
            private readonly T _root;
            private bool _started;

            public SingleRootEnumerable(T root)
            {
                this._root = root;
            }

            T IEnumerator<T>.Current => this._root;

            object IEnumerator.Current => throw NotSupported();

            void IDisposable.Dispose() { }

            IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;

            IEnumerator IEnumerable.GetEnumerator() => throw NotSupported();

            bool IEnumerator.MoveNext() => this._started ? false : (this._started = true);

            void IEnumerator.Reset() => throw NotSupported();

            NotSupportedException NotSupported() => new NotSupportedException();
        }
    }
}
