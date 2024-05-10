using System.Collections.Generic;
using System.Linq;

public class DijkstraAlgorithm : ShortestPathStrategy
{
    public List<GraphSearchNode<SelectableObject>> FindShortestPathToDestination(List<GraphSearchNode<SelectableObject>> graphSearchNodes, GraphNode<SelectableObject> source, GraphNode<SelectableObject> destination)
    {
        var sourceSearchNode = graphSearchNodes.Find(sn => sn.GraphNode == source);
        sourceSearchNode.CostToStart = 0;
        var destinationSearchNode =  graphSearchNodes.Find(sn => sn.GraphNode == destination);
        var nodeList = new List<GraphSearchNode<SelectableObject>> {
            sourceSearchNode
        };
        while (nodeList.Any())  
        {   
            nodeList = nodeList.OrderBy(n => n.CostToStart).ToList();
            var node = nodeList.First();
            nodeList.Remove(node);
            foreach (var connection in node.GraphNode.Connections) 
            {
                var neighbourSearchNode = graphSearchNodes.Find(n => connection.Destination == n.GraphNode);
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
            return null;
        }
        else 
        {
            var shortestPath = new List<GraphSearchNode<SelectableObject>>{destinationSearchNode};
            var node = destinationSearchNode;
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