using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Medallion.Collections
{
    public static partial class Traverse
    {
        /// <summary>
        /// Enumerates the implicit tree described by <paramref name="root"/> and <paramref name="children"/>
        /// in a depth-first manner. By default, a pre-order traversal is used (parent before its children), but 
        /// specifying <paramref name="postOrder"/> will switch to a post-order traversal (children before their parent).
        /// 
        /// For example, this could be used to enumerate the exceptions of an
        /// <see cref="AggregateException"/>:
        /// <code>
        ///     var allExceptions = Traverse.DepthFirst((Exception)new AggregateException(), e => (e as AggregateException)?.InnerExceptions ?? Enumerable.Empty&lt;Exception&gt;());
        /// </code>
        /// </summary>
        public static IEnumerable<T> DepthFirst<T>(T root, Func<T, IEnumerable<T>> children, bool postOrder = false)
        {
            if (children == null) { throw new ArgumentNullException(nameof(children)); }

            return DepthFirstIterator();

            IEnumerable<T> DepthFirstIterator()
            {
                // note that this implementation has two nice properties which require a bit more complexity
                // in the code: (1) children are yielded in order and (2) child enumerators are fully lazy

                // setup: yield the root for pre-order, then prime the stack with the children of root
                if (!postOrder)
                {
                    yield return root;
                }
                var stack = new Stack<IEnumerator<T>>();
                stack.Push(children(root).GetEnumerator());

                try
                {
                    while (true)
                    {
                        // if the to enumerator has a new element...
                        var childrenEnumerator = stack.Peek();
                        if (childrenEnumerator.MoveNext())
                        {
                            var current = childrenEnumerator.Current;
                            // yield it for pre-order
                            if (!postOrder)
                            {
                                yield return current;
                            }
                            // then push it's children
                            stack.Push(children(current).GetEnumerator());
                        }
                        else
                        {
                            // otherwise, clean up the exhausted enumerator and remove it from the stack
                            childrenEnumerator.Dispose();
                            stack.Pop();
                            if (stack.Count == 0) // done
                            {
                                if (postOrder) // for post-order, finish with root
                                {
                                    yield return root;
                                }
                                break;
                            }
                            else if (postOrder) // for post-order, yield the current parent on the way back
                            {
                                yield return stack.Peek().Current;
                            }
                        }
                    }
                }
                finally
                {
                    // guarantee that everything is cleaned up even
                    // if we don't enumerate all the way through
                    Exception? lastDisposalException = null;
                    while (stack.Count != 0)
                    {
                        try { stack.Pop()?.Dispose(); }
                        catch (Exception ex) { lastDisposalException = ex; }
                    }

                    // rethrow only the last caught (outermost) disposal exception, mimicking what would
                    // happen with a bunch of nested using blocks in the case of multiple disposal failures
                    if (lastDisposalException != null)
                    {
                        ExceptionDispatchInfo.Capture(lastDisposalException).Throw();
                    }
                }
            }
        }
    }
}
