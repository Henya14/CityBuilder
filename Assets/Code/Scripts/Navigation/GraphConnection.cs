public class GraphConnection<T> {
    public const int NO_CONNECTION_WEIGHT = 0;
    public int Weight {get; set;} = NO_CONNECTION_WEIGHT;
    public GraphNode<T> Source  {get; set;}
    public GraphNode<T> Destination  {get; set;}

}