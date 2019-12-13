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
List<OperationCanceledException> FindOperationCanceledExceptions(Exception ex)
{
	var result = new List<OperationCanceledException>();
	
	void Search(Exception e)
	{
		if (e is OperationCanceledException oce) { result.Add(e); }
		else if (e is AggregateException agg)
		{
			foreach (var ie in agg.InnerExceptions) { Search(ie); }
		}
		else if (e.InnerException != null) { Search(e.InnerException); }
	}
}
```

The following traversal methods are supported:
	* [DepthFirst](https://en.wikipedia.org/wiki/Depth-first_search): pre-order by default, but can be flipped to post-order by passing `postOrder: true`
	* [BreadthFirst] (https://en.wikipedia.org/wiki/Breadth-first_search): can traverse from one root or from multiple roots
	* Along: traverses along a singly-linked list, e. g. `Traverse.Along(new DirectoryInfo(path), d => d.Parent)`


## Release notes
- 1.0.0 Initial release
