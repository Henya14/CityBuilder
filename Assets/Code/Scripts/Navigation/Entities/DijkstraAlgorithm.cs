using System.Collections.Generic;
using System.Linq;

public class DijkstraAlgorithm : IShortestPathStrategy
{
    public RouteResult<T> FindShortestPathToDestination<T>(IReadOnlyList<GraphSearchNode<T>> graphSearchNodes, GraphNode<T> source, GraphNode<T> destination)
    {
        var sourceSearchNode = graphSearchNodes.FirstOrDefault(sn => sn.GraphNode == source);
        sourceSearchNode.CostToStart = 0;
        var destinationSearchNode =  graphSearchNodes.FirstOrDefault(sn => sn.GraphNode.Value.Equals(destination.Value));
        var nodeList = new List<GraphSearchNode<T>> {
            sourceSearchNode
        };
        while (nodeList.Any())  
        {   
            nodeList = nodeList.OrderBy(n => n.CostToStart).ToList();
            var node = nodeList.First();
            nodeList.Remove(node);
            var outgoingConnections = node.GraphNode.GetOutGoingConnections();
            foreach (var connection in outgoingConnections) 
            {
                if (connection.Weight == GraphConnection<T>.NO_CONNECTION_WEIGHT)
                {
                    continue;
                }
                var neighbourSearchNode = graphSearchNodes.FirstOrDefault(n => connection.Destination == n.GraphNode);
                if (neighbourSearchNode.Visited) {
                    continue;
                }

                if (neighbourSearchNode.CostToStart == null || node.CostToStart + connection.Weight < neighbourSearchNode.CostToStart ) {
                    neighbourSearchNode.CostToStart = node.CostToStart + connection.Weight;
                    neighbourSearchNode.ShortestNodeToStart = node;
                } 

                if (!nodeList.Contains(neighbourSearchNode)) 
                {
                    nodeList.Add(neighbourSearchNode);
                }
            }
            node.Visited = true;

            if (node == destinationSearchNode) {
                break;
            }
        }
       

        if (destinationSearchNode.ShortestNodeToStart == null) 
        {
            return RouteResult<T>.Empty();
        }
        else 
        {
            var shortestPath = new List<GraphSearchNode<T>>();
            var node = destinationSearchNode;
            while (node != null) 
            {
                shortestPath.Add(node);
                node = node.ShortestNodeToStart;
            }
            shortestPath.Reverse();
            return new RouteResult<T>
            {
                ShortestPath = shortestPath,
                TotalCost = destinationSearchNode.CostToStart ?? 0,
                IsSuccess = true
            };
        }
    }
}