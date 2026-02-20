
using System;
using System.Collections.Generic;

public interface IGraphRepository<T>
{
    GraphNode<T> GetNode(T value);
    IReadOnlyList<GraphNode<T>> GetAllNodes();
    void RemoveNode(T node);
    
    GraphNode<T> AddNode(T value, Position position);
    public void AddConnection(GraphNode<T> from, GraphNode<T> to, float weight);
    public GraphConnection<GraphNode<T>> GetConnection(GraphNode<T> from, GraphNode<T> to);

    public IReadOnlyList<GraphNode<T>> Query(Func<GraphNode<T>, bool> predicate);

    public IReadOnlyList<GraphSearchNode<T>> GetGraphSearchNodes(GraphNode<T> startNode);
}