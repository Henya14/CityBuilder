using System.Collections.Generic;

public class GraphNode<T> {

    public T Value  { get; set; }
    public List<GraphConnection<T>>  connections  { get; set; } = new List<GraphConnection<T>>();
    
}

public class GraphSearchNode<T>: GraphNode<T> {

    public GraphConnection<T> ShortestConnectionToStart  { get; set; } = null;
    public int CostToStart  { get; set; } = 0;
    public bool Visited  { get; set; } = false;
}