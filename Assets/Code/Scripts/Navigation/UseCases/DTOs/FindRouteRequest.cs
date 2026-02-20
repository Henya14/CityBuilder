
public class FindRouteRequest<T>
{
    public GraphNode<T> Start { get; }
    public GraphNode<T> End { get; }

    public FindRouteRequest(
        GraphNode<T> start,
        GraphNode<T> end)
    {
        Start = start;
        End = end;
    }
}