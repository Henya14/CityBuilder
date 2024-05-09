using System.Collections.Generic;
using System.Linq;

public class DijkstraAlgorithm : ShortestPathStrategy
{
    public List<GraphSearchNode<SelectableObject>> FindShortestPathToDestination(List<GraphSearchNode<SelectableObject>> adjacencyGraph, GraphSearchNode<SelectableObject> source, GraphSearchNode<SelectableObject> destination)
    {
        
        source.CostToStart = 0;        
        var nodeList = new List<GraphSearchNode<SelectableObject>> {
            source
        };
        while (nodeList.Any())  
        {   
            nodeList = nodeList.OrderBy(n => n.CostToStart).ToList();
            var node = nodeList.First();
            nodeList.Remove(node);
            foreach (var connection in node.GraphNode.Connections) 
            {
                var neighbourSearchNode = adjacencyGraph.Find(n => connection.Destination == n.GraphNode);

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

            if (node == destination) {
                break;
            }
        }
       

        if (destination.ShortestNodeToStart == null) 
        {
            return null;
        }
        else 
        {
            var shortestPath = new List<GraphSearchNode<SelectableObject>>{destination};
            var node = destination;
            while (node.ShortestNodeToStart != null) 
            {
                shortestPath.Add(node);
                node = node.ShortestNodeToStart;
            }
            shortestPath.Reverse();
            return shortestPath;
        }
    }
}