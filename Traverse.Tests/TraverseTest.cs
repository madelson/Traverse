using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Medallion.Collections.Tests
{
    public class TraverseTest
    {
        [Test]
        public void TestAlong()
        {
            Assert.Throws<ArgumentNullException>(() => Traverse.Along("a", null));

            var ex = new Exception("a", new Exception("b", new Exception("c")));

            CollectionAssert.AreEqual(
                new[] { ex, ex.InnerException, ex.InnerException.InnerException },
                Traverse.Along(ex, e => e.InnerException)
            );

            CollectionAssert.AreEqual(
                Enumerable.Empty<Exception>(),
                Traverse.Along(default(Exception), e => e.InnerException)
            );
        }

        [Test]
        public void TestDepthFirst()
        {
            Assert.Throws<ArgumentNullException>(() => Traverse.DepthFirst("a", null));

            CollectionAssert.AreEqual(
                actual: Traverse.DepthFirst("abcd", s => s.Length < 2 ? Enumerable.Empty<string>() : new[] { s.Substring(0, s.Length - 1), s.Substring(1) }),
                expected: new[]
                {
                    "abcd",
                    "abc",
                    "ab",
                    "a",
                    "b",
                    "bc",
                    "b",
                    "c",
                    "bcd",
                    "bc",
                    "b",
                    "c",
                    "cd",
                    "c",
                    "d"
                }
            );
        }

        [Test]
        public void TestDepthFirstPostOrder()
        {
            CollectionAssert.AreEqual(
                actual: Traverse.DepthFirst("abcd", s => s.Length < 2 ? Enumerable.Empty<string>() : new[] { s.Substring(0, s.Length - 1), s.Substring(1) }, postOrder: true),
                expected: new[] { "a", "b", "ab", "b", "c", "bc", "abc", "b", "c", "bc", "c", "d", "cd", "bcd", "abcd" }
            );
        }

        /// <summary>
        /// Notes: we don't have an API for depth-first multi-root traversal since that's the same as concatenation. This test demonstrates
        /// that we get that same result
        /// </summary>
        [Test]
        public void TestDepthFirstMultipleRoots()
        {
            CollectionAssert.AreEqual(
                actual: new[] { 3, 5, 4 }.SelectMany(x => Traverse.DepthFirst(x, i => i <= 1 ? Enumerable.Empty<int>() : new[] { (i / 2) + (i % 2), i / 2 })),
                expected: new[] { 3, 2, 1, 1, 1, 5, 3, 2, 1, 1, 1, 2, 1, 1, 4, 2, 1, 1, 2, 1, 1 }
            );
        }

        /// <summary>
        /// Notes: we don't have an API for depth-first multi-root traversal since that's the same as concatenation. This test demonstrates
        /// that we get that same result
        /// </summary>
        [Test]
        public void TestDepthFirstMultipleRootsPostOrder()
        {
            CollectionAssert.AreEqual(
                actual: new[] { 3, 5, 4 }.SelectMany(x => Traverse.DepthFirst(x, i => i <= 1 ? Enumerable.Empty<int>() : new[] { (i / 2) + (i % 2), i / 2 }, postOrder: true)),
                expected: new[] { 1, 1, 2, 1, 3, 1, 1, 2, 1, 3, 1, 1, 2, 5, 1, 1, 2, 1, 1, 2, 4 }
            );
        }

        [Test]
        public void TestBreadthFirst()
        {
            Assert.Throws<ArgumentNullException>(() => Traverse.BreadthFirst("a", null));

            CollectionAssert.AreEqual(
                actual: Traverse.BreadthFirst("abcd", s => s.Length < 2 ? Enumerable.Empty<string>() : new[] { s.Substring(0, s.Length - 1), s.Substring(1) }),
                expected: new[]
                {
                    "abcd",
                    "abc",
                    "bcd",
                    "ab",
                    "bc",
                    "bc",
                    "cd",
                    "a",
                    "b",
                    "b",
                    "c",
                    "b",
                    "c",
                    "c",
                    "d",
                }
            );
        }

        [Test]
        public void TestBreadthFirstMultipleRoots()
        {
            Assert.Throws<ArgumentNullException>(() => Traverse.BreadthFirst(default(IEnumerable<string>), _ => Enumerable.Empty<string>()));
            Assert.Throws<ArgumentNullException>(() => Traverse.BreadthFirst(Enumerable.Empty<string>(), default(Func<string, IEnumerable<string>>)));

            CollectionAssert.AreEqual(
                actual: Traverse.BreadthFirst(new[] { 3, 5, 4 }, i => i <= 1 ? Enumerable.Empty<int>() : new[] { (i / 2) + (i % 2), i / 2 }),
                expected: new[] 
                { 
                    3, 5, 4, 
                    2, 1, 3, 2, 2, 2,  
                    1, 1, 2, 1, 1, 1, 1, 1, 1, 1,
                    1, 1
                }
            );
        }

        [Test]
        public void DepthFirstEnumeratorsAreLazyAndDisposeProperly()
        {
            var helper = new EnumeratorHelper();

            var sequence = Traverse.DepthFirst(10, i => helper.MakeEnumerator(i - 1));

            Assert.AreEqual(0, helper.IterateCount);
            Assert.AreEqual(0, helper.StartCount);
            Assert.AreEqual(0, helper.EndCount);

            using (var enumerator = sequence.GetEnumerator())
            {
                for (var i = 0; i < 10; ++i)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                }
                Assert.AreEqual(9, helper.IterateCount); // -1 for root
            }

            Assert.AreEqual(helper.EndCount, helper.StartCount);
        }

        [Test]
        public void BreadthFirstEnumeratorsAreLazyAndDisposeProperly()
        {
            var helper = new EnumeratorHelper();

            var sequence = Traverse.BreadthFirst(10, i => helper.MakeEnumerator(i - 1));

            Assert.AreEqual(0, helper.IterateCount);
            Assert.AreEqual(0, helper.StartCount);
            Assert.AreEqual(0, helper.EndCount);

            using (var enumerator = sequence.GetEnumerator())
            {
                for (var i = 0; i < 10; ++i)
                {
                    Assert.IsTrue(enumerator.MoveNext());
                }
                Assert.AreEqual(9, helper.IterateCount); // -1 for root
            }

            Assert.AreEqual(helper.EndCount, helper.StartCount);
        }

        [Test]
        public void TestEmpty()
        {
            Assert.IsEmpty(Traverse.Along(default(object), _ => throw new InvalidOperationException("should never get here")));
            Assert.IsEmpty(Traverse.DepthFirst(1, _ => Enumerable.Empty<int>()).Skip(1));
            Assert.IsEmpty(Traverse.BreadthFirst(1, _ => Enumerable.Empty<int>()).Skip(1));
            Assert.IsEmpty(Traverse.BreadthFirst<char>(Enumerable.Empty<char>(), _ => throw new InvalidOperationException("should never get here")));
        }

        [Test]
        public void TestHandlingOfEnumeratorDisposalErrors()
        {
            var enumerators = new List<ThrowingEnumeratorEnumerable>();
            var disposalException = Assert.Throws<InvalidOperationException>(
                () => Traverse.DepthFirst(
                        5,
                        i =>
                        {
                            var enumerator = i > 0 ? new ThrowingEnumeratorEnumerable(i - 1) : new ThrowingEnumeratorEnumerable();
                            enumerators.Add(enumerator);
                            return enumerator;
                        }
                    )
                    .ToArray()
            );
            Assert.AreEqual("Throw from 4 dispose", disposalException.Message);

            Assert.AreEqual(6, enumerators.Count);
            Assert.IsTrue(enumerators.All(e => e.Disposed));
        }

        private class EnumeratorHelper
        {
            public int StartCount { get; private set; }
            public int EndCount { get; private set; }
            public int IterateCount { get; private set; }

            public IEnumerable<int> MakeEnumerator(int i)
            {
                ++this.StartCount;

                try
                {
                    for (var j = 0; j < i; ++j)
                    {
                        ++this.IterateCount;
                        yield return i;
                    }
                }
                finally
                {
                    ++this.EndCount;
                }
            }
        }

        private class ThrowingEnumeratorEnumerable : IEnumerable<int>, IEnumerator<int>
        {
            private readonly int? _value;
            private bool _started;

            public ThrowingEnumeratorEnumerable(int? value = null)
            {
                this._value = value;
            }

            public bool Disposed { get; private set; }

            int IEnumerator<int>.Current => this._value ?? throw new InvalidOperationException("Throw from current");
            object IEnumerator.Current => this._value;

            void IDisposable.Dispose() {
                this.Disposed = true;
                throw new InvalidOperationException($"Throw from {this._value} dispose");
            }

            IEnumerator<int> IEnumerable<int>.GetEnumerator() => this;
            IEnumerator IEnumerable.GetEnumerator() => this;

            bool IEnumerator.MoveNext() => this._started ? false : (this._started = true);

            void IEnumerator.Reset() => throw new NotSupportedException();
        }
    }
}
