
using System;
using System.Collections.Generic;
using System.Linq;

public class NavigationGraph<T>
{
    private readonly List<GraphNode<T>> _nodes = new List<GraphNode<T>>();

    public IReadOnlyList<GraphNode<T>> Nodes => _nodes;

    public void AddNode(GraphNode<T> node)
    {
        _nodes.Add(node);
    }

    public GraphNode<T> AddNode(T value, Position position)
    {
        var node = new GraphNode<T>(value, position);
         _nodes.Add(node);
         return node;
    }

    

    public GraphNode<T> GetNode(T value)
    {
        return _nodes.Find(node => EqualityComparer<T>.Default.Equals(node.Value, value));
    }

    public void RemoveNode(GraphNode<T> node)
    {
        _nodes.Remove(node);
        foreach (var graphNode in Nodes)
        {
            var connection = graphNode.GetConnection(graphNode, node);
            graphNode.RemoveConnection(connection);
        }
    }

    public void RemoveNode(T value)
    {
        var node = GetNode(value);
        if (node != null)
        {
            RemoveNode(node);
        }
    }

    public GraphConnection<GraphNode<T>> GetConnection(GraphNode<T> from, GraphNode<T> to)
    {
        return _nodes.SelectMany(node => node.Connections).FirstOrDefault(x => x.Source == from && x.Destination == to);
    }

    public List<GraphNode<T>> Where(Func<GraphNode<T>, bool> predicate)
    {
        return _nodes.Where(predicate).Select(n =>
        {
            var newNode = new GraphNode<T>(n.Value, n.Position);

            var filteredConnections = n.WhereConnections(c => predicate(c.Source) && predicate(c.Destination));

            filteredConnections.ForEach(c => newNode.AddConnection(c));
            return newNode;
        }).ToList();
    }
    
    public IReadOnlyList<GraphSearchNode<T>> GetGraphSearchNodes(GraphNode<T> startNode)
    {
        var searchNodes = new List<GraphSearchNode<T>>();
        foreach (var node in Nodes)
        {
            var straightLineDistanceToDestination = Position.Distance(node.Position, startNode.Position);
            var searchNode = new GraphSearchNode<T>
            {
                CostToStart = null,
                ShortestConnectionToStart = null,
                Visited = false,
                GraphNode = node,
                StraightLineDistanceToDestination = straightLineDistanceToDestination
            };
            searchNodes.Add(searchNode);
        }

        return searchNodes;
    }

    public IReadOnlyList<GraphNode<T>> GetAllNodes()
    {
        return _nodes;
    }

    public void AddConnection(GraphNode<T> from, GraphNode<T> to, float weight)
    {
        from.AddConnection(to, (int)weight);
    }
}