using System.Collections.Generic;

public interface IShortestPathStrategy
{
    RouteResult<T> FindShortestPathToDestination<T>(IReadOnlyList<GraphSearchNode<T>> adjacencyGraph, GraphNode<T> source, GraphNode<T> destination);
}