using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphNode<T>
{

    public T Value { get; set; }
    
    public List<GraphConnection<GraphNode<T>>> Connections { get; set; } = new List<GraphConnection<GraphNode<T>>>();

    public void AddConnection(GraphConnection<GraphNode<T>> connection)
    {
        Connections.Add(connection);
    }

    public void AddConnection(GraphNode<T> destination, int weight)
    {
        var connectionFromNode = new GraphConnection<GraphNode<T>>
        {
            Source = this,
            Destination = destination,
            Weight = weight
        };
        destination.AddConnection(connectionFromNode);
        Connections.Add(connectionFromNode);
    }

    public void RemoveConnection(GraphConnection<GraphNode<T>> connection)
    {
        Connections.Remove(connection);
    }

    public GraphConnection<GraphNode<T>> GetConnection(GraphNode<T> from, GraphNode<T> to)
    {
        return Connections.Find(x => x.Source == from && x.Destination == to) ?? null;
    }

    public List<GraphConnection<GraphNode<T>>> WhereConnections(Func<GraphConnection<GraphNode<T>>, bool> predicate)
    {
        return Connections.Where(predicate).ToList();
    }
    public List<GraphConnection<GraphNode<T>>> GetOutGoingConnections() {
        return WhereConnections(connection => connection.Source == this);
    }
}

public class GraphSearchNode<T>
{
    public GraphNode<T> GraphNode {get; set;}
    public GraphConnection<GraphNode<T>> ShortestConnectionToStart { get; set; } = null;
    public GraphSearchNode<T> ShortestNodeToStart { get; set; } = null;
    public int? CostToStart { get; set; } = null;
    public float? StraightLineDistanceToDestination = null;
    public bool Visited { get; set; } = false;
}