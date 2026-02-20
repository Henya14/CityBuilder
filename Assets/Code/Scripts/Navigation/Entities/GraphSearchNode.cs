public class GraphSearchNode<T>
{
    public GraphNode<T> GraphNode {get; set;}
    public GraphConnection<GraphNode<T>> ShortestConnectionToStart { get; set; } = null;
    public GraphSearchNode<T> ShortestNodeToStart { get; set; } = null;
    public int? CostToStart { get; set; } = null;
    public float? StraightLineDistanceToDestination = null;
    public bool Visited { get; set; } = false;
}