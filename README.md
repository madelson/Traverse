# Traverse

Traverse is a .NET library that makes it easy to explore trees and tree-like structures using LINQ.

Traverse is available for download as a [NuGet package](https://www.nuget.org/packages/Traverse). [![NuGet Status](http://img.shields.io/nuget/v/Traverse.svg?style=flat)](https://www.nuget.org/packages/Traverse/)

[Release notes](#release-notes)

## Documentation

### Overview

The `Traverse` class contains utility methods for lazily flattening trees and implicit trees into `IEnumerable`s. 

For example, let's say we wanted to explore the implicit "tree" of an `Exception` in search of `OperationCanceledException`s. We could use `Traverse` to express the algorithm like this:
```C#
Exception ex = ...;
var operationCanceledExceptions = Traverse.DepthFirst(
		ex,
		e => e is AggregateException agg ? agg.InnerExceptions : new[] { e.InnerException }.Where(ie => ie != null)
	)
	.OfType<OperationCanceledException>();
```

This is roughly equivalent to the following recursive method, except that **the `Traverse` approach is lazy, can be defined inline and, since it does not use recursion, is not vulnerable to stack overflow**:
```C#
// without using Traverse
List<OperationCanceledException> FindOperationCanceledExceptions(Exception ex)
{
	var result = new List<OperationCanceledException>();
	
	void Search(Exception e)
	{
		if (e is OperationCanceledException oce) { result.Add(e); }
		
		if (e is AggregateException agg)
		{
			foreach (var ie in agg.InnerExceptions) { Search(ie); }
		}
		else if (e.InnerException != null) { Search(e.InnerException); }
	}
}
```

The following traversal methods are supported:
- [DepthFirst](https://en.wikipedia.org/wiki/Depth-first_search): pre-order by default, but can be flipped to post-order by passing `postorder: true`
- [BreadthFirst](https://en.wikipedia.org/wiki/Breadth-first_search): can traverse from one root or from multiple roots
- Along: traverses along a singly-linked list

### Basic Examples

For these examples, assume the following directory structure:
```
C:\
C:\a
C:\a\b
C:\a\c
C:\d
C:\d\e
```

```C#
// yields C:\, C:\a, C:\a\b, C:\a\c, C:\d, C:\d\e
Traverse.DepthFirst(new DirectoryInfo(@"C:\"), d => d.GetDirectories());

// yields C:\a\b, C:\a\c, C:\a, C:\d\e, C:\d, C:\
Traverse.DepthFirst(new DirectoryInfo(@"C:\"), d => d.GetDirectories(), postorder: true);

// yields C:\, C:\a, C:\d, C:\a\b, C:\a\c, C:\d\e
Traverse.BreadthFirst(new DirectoryInfo(@"C:\"), d => d.GetDirectories());

// yields C:\a, C:\d, C:\a\b, C:\a\c, C:\d\e
Traverse.BreadthFirst(
	new[] { new DirectoryInfo(@"C:\a"), new DirectoryInfo(@"C:\d") }, 
	d => d.GetDirectories()
);

// yields C:\a\c, C:\a, C:\
Traverse.Along(new DirectoryInfo(@"C:\a\c"), d => d.Parent);
```

### Tracking traversal depth

Sometimes, it can be useful to know your depth within the tree as you traverse. While this isn't built into the `Traverse` library, it is easy enough to track:
```C#
Traverse.DepthFirst(
	(dir: new DirectoryInfo(@"C:\"), depth: 0), 
	t => t.dir.GetDirectories().Select(d => (dir: d, depth: t.depth + 1))
);
```

### Dealing with cycles and repeats

The library assumes a tree structure, and makes no attempt to detect repeat visits to the same node (which you might see in a non-tree DAG) or cycles. However, this is also easily handled:
```C#
var visited = new HashSet<Node> { root };
// note, do not enumerate this sequence multiple times as the visited set will not reset!
var distinctTraversal = Traverse.BreadthFirst(root, n => n.Children.Where(visited.Add));
```

## Release notes
- 1.0.0 Initial release
