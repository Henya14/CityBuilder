public class GraphConnection<G> {
    public const int NO_CONNECTION_WEIGHT = 0;
    public int Weight {get; set;} = NO_CONNECTION_WEIGHT;
    public G Source  {get; set;}
    public G Destination  {get; set;}

}