using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class GraphRepositoryAdapter<T> : IGraphRepository<T>
{
    private readonly NavigationGraph<T> _navigationGraph;
    
    public GraphRepositoryAdapter(NavigationGraph<T> navigationGraph)
    {
        _navigationGraph = navigationGraph;
    }

    public void AddConnection(GraphNode<T> from, GraphNode<T> to, float weight)
    {
        _navigationGraph.AddConnection(from, to, weight);
    }

    public GraphNode<T> AddNode(T value, Position position)
    {
        return _navigationGraph.AddNode(value, position);
    }

    public IReadOnlyList<GraphNode<T>> GetAllNodes()
    {
        return _navigationGraph.GetAllNodes();
    }

    public GraphConnection<GraphNode<T>> GetConnection(GraphNode<T> from, GraphNode<T> to)
    {
        return _navigationGraph.GetConnection(from, to);
    }

    public IReadOnlyList<GraphSearchNode<T>> GetGraphSearchNodes(GraphNode<T> startNode)
    {
        return _navigationGraph.GetGraphSearchNodes(startNode);
    }

    public GraphNode<T> GetNode(T value)
    {
        return _navigationGraph.GetNode(value);
    }

    public IReadOnlyList<GraphNode<T>> Query(Func<GraphNode<T>, bool> predicate)
    {
        return _navigationGraph.Where(predicate);
    }
    

    public void RemoveNode(T node)
    {
        _navigationGraph.RemoveNode(node);
    }
}