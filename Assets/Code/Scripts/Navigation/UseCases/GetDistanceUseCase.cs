using System.Collections.Generic;

public class GetDistanceUseCase<T>
{
   private readonly IShortestPathStrategy _shortestPathStrategy;

   public GetDistanceUseCase(IShortestPathStrategy shortestPathStrategy)
   {
       _shortestPathStrategy = shortestPathStrategy;
   }

   public float Execute(GraphNode<T> start, GraphNode<T> end, IReadOnlyList<GraphSearchNode<T>> graphSearchNodes)
   {
       var routeResult = _shortestPathStrategy.FindShortestPathToDestination(graphSearchNodes, start, end);
       return routeResult.TotalCost;
   }
}