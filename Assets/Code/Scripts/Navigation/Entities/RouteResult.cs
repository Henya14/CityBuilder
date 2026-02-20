using System.Collections.Generic;

public class RouteResult<T>
{
    public List<GraphSearchNode<T>> ShortestPath { get; set; }
    public int TotalCost { get; set; }
    public bool IsSuccess { get; set; }

    public static RouteResult<T> Empty()
    {
        return new RouteResult<T>
        {
            ShortestPath = new List<GraphSearchNode<T>>(),
            TotalCost = 0,
            IsSuccess = false
        };
    }

}