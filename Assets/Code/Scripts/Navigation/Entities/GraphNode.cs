using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public class GraphNode<T>
{

    public T Value { get; set; }

    public Position Position { get; set; }

    public GraphNode(T value, Position position)
    {
        Value = value;
        Position = position;
    }
    
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

