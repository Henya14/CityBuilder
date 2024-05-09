using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public interface ShortestPathStrategy {
    List<GraphSearchNode<SelectableObject>> FindShortestPathToDestination(List<GraphSearchNode<SelectableObject>> adjacencyGraph, GraphSearchNode<SelectableObject> source, GraphSearchNode<SelectableObject> destination);
}

public class NavigationManager : MonoBehaviour
{
    BuildingAdjacencyMatrix adjacencyMatrix = new BuildingAdjacencyMatrix();
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
        if (selectedObjects.Count == 2)
        {
            DeselectObjects();
        }
        if (adjacencyGraph.GetGraphNodeAtGridPosition(selectedObject.GetGridPosition()) != null)
        {
            selectedObjects.Add(selectedObject);
            selectedObject.ToggleHighlight(true);
            selectedObject.FreezeHighlight(true);

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
