using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphNode<T>
{

    public T Value { get; set; }
    private List<GraphConnection<T>> connections { get; set; } = new List<GraphConnection<T>>();

    public void AddConnection(GraphConnection<T> connection)
    {
        connections.Add(connection);
    }

    public void AddConnection(GraphNode<T> destination, int weight)
    {
        var connection = new GraphConnection<T>
        {
            Source = this,
            Destination = destination,
            Weight = weight
        };
        connections.Add(connection);
    }

    public void RemoveConnection(GraphConnection<T> connection)
    {
        connections.Remove(connection);
    }

    public GraphConnection<T> GetConnection(GraphNode<T> from, GraphNode<T> to)
    {
        return connections.Find(x => x.Source == from && x.Destination == to) ?? null;
    }

    public List<GraphConnection<T>> WhereConnections(Func<GraphConnection<T>, bool> predicate)
    {
        return connections.Where(predicate).ToList();
    }
}

public class GraphSearchNode<T> : GraphNode<T>
{

    public GraphConnection<T> ShortestConnectionToStart { get; set; } = null;
    public int CostToStart { get; set; } = 0;
    public bool Visited { get; set; } = false;
}