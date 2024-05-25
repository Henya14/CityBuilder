using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public interface ShortestPathStrategy {
    List<GraphSearchNode<SelectableObject>> FindShortestPathToDestination(List<GraphSearchNode<SelectableObject>> adjacencyGraph, GraphNode<SelectableObject> source, GraphNode<SelectableObject> destination);
}

public class NavigationManager : MonoBehaviour
{
    BuildingAdjacencyGraph adjacencyGraph = new BuildingAdjacencyGraph();
    List<SelectableObject> selectedObjects = new List<SelectableObject>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ObjectSelected(SelectableObject selectedObject)
    {
        if (selectedObject.GetSelectableObjectType() == SelectableObjectType.Tile) {
            return;
        }
        if (selectedObjects.Count >= 2)
        {
            DeselectObjects();
        }
        if (adjacencyGraph.GetGraphNodeAtGridPosition(selectedObject.GetGridPosition()) != null)
        {
            selectedObjects.Add(selectedObject);
            selectedObject.ToggleHighlight(true);
            selectedObject.FreezeHighlight(true);
            if (selectedObjects.Count == 2) {
                
                var algo = new AStarAlgorithm();
                var start = adjacencyGraph.GetGraphNodeForSelectableObject(selectedObjects[0]);
                var destination = adjacencyGraph.GetGraphNodeForSelectableObject(selectedObjects[1]);
                var searchNodes = adjacencyGraph.GetGraphSearchNodes().Select(sn => {
                    sn.StraightLineDistanceToDestination = (start.Value.GetGridPosition() - sn.GraphNode.Value.GetGridPosition()).magnitude;
                    return sn;
                }).ToList();
                var ruta = algo.FindShortestPathToDestination(searchNodes, 
                start, 
                destination);
                ruta?.ForEach(r => {
                    r.GraphNode.Value.FreezeHighlight(false);
                    r.GraphNode.Value.ToggleHighlight(true);
                    r.GraphNode.Value.FreezeHighlight(true);
                    selectedObjects.Add(r.GraphNode.Value);
                });
            }

        }
    }

    public void DeselectObjects()
    {
        selectedObjects.ForEach(so =>
        {
            so.FreezeHighlight(false);
            so.ToggleHighlight(false);
        });

        selectedObjects.Clear();
    }

    public void AddBuilding(SelectableObject building, Dictionary<SelectableObject, NeighbourWeights> weights) 
    {
        adjacencyGraph.AddBuilding(building, weights);
    }

}
