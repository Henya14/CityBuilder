public class GraphConnection<T> {
    public int Weight {get; set;} = 0;
    public GraphNode<T> Source  {get; set;}
    public GraphNode<T> Target  {get; set;}

}