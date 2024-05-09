using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public class BuildingAdjacencyGraph
{
    public List<GraphNode<SelectableObject>> GraphNodes {get; private set;} = new List<GraphNode<SelectableObject>>();

    public BuildingAdjacencyGraph() { }

    public int GetWeightOfPath(GraphNode<SelectableObject> from, GraphNode<SelectableObject> to)
    {
        return from.GetConnection(from, to)?.Weight ?? GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT;
    }

    public void AddBuilding(SelectableObject building, Dictionary<SelectableObject, NeighbourWeights> neighbours) 
    {
        var node = new GraphNode<SelectableObject> {
            Value = building
        };
        AddGraphNode(node, neighbours);
    }

    public void AddGraphNode(GraphNode<SelectableObject> node, Dictionary<SelectableObject, NeighbourWeights> neighbours) 
    {
        
        GraphNodes.Add(node);
        foreach (var neighbour in neighbours)
        {
            var neighbourNode = GraphNodes.Find(x => x.Value.Equals(neighbour.Key));
            if (neighbourNode == null) {
                continue;
            }
            if (neighbour.Value.WeightFromNeighbour != GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT) {
                neighbourNode.AddConnection(node, neighbour.Value.WeightFromNeighbour);
            }
            
            if (neighbour.Value.WeightToNeighbour != GraphConnection<SelectableObject>.NO_CONNECTION_WEIGHT) {
                node.AddConnection(neighbourNode, neighbour.Value.WeightToNeighbour);
            }
        }
    }

    public void RemoveGraphNode(GraphNode<SelectableObject> nodeToDelete) 
    {
        GraphNodes.Remove(nodeToDelete);

        foreach (var graphNode in GraphNodes) 
        {
            var connection = graphNode.GetConnection(graphNode, nodeToDelete);
            graphNode.RemoveConnection(connection);
        }
    }

    public BuildingAdjacencyGraph WhereGraphNodes(Func<GraphNode<SelectableObject>, bool> predicate) 
    {
        

        var newNodes = GraphNodes.Where(predicate).Select(n => {
            var newNode = new GraphNode<SelectableObject> 
            {
                Value = n.Value
            };
            
            var filteredConnections = n.WhereConnections(c => predicate(c.Source) && predicate(c.Destination));

            filteredConnections.ForEach(c => newNode.AddConnection(c));
            return newNode;
        }).ToList();

        var newGraph = new BuildingAdjacencyGraph()
        {
            GraphNodes = newNodes
        };
        newGraph.GraphNodes = newNodes;

        return newGraph;
    }

    public GraphNode<SelectableObject> GetGraphNodeAtGridPosition(Vector3Int gridPosition)
    {
        return GraphNodes.Find(o => o.Value.GetGridPosition() == gridPosition);
    }

    public List<GraphSearchNode<SelectableObject>> GetGraphSearchNodes() 
    {
        var searchNodes = new List<GraphSearchNode<SelectableObject>>();
        foreach (var node in GraphNodes) 
        {
            var searchNode = new GraphSearchNode<SelectableObject> {
                CostToStart = null,
                ShortestConnectionToStart = null,
                Visited = false,
                GraphNode = node
            };
            searchNodes.Add(searchNode);
        } 

        return searchNodes;
    }

}