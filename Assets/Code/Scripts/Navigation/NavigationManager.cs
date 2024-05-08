using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;


public interface ShortestPathStrategy {
    void FindShortestPathToDestination(BuildingAdjacencyMatrix adjacencyMatrix, SelectableObject source, SelectableObject destination);
}

public class NavigationManager : MonoBehaviour
{
    BuildingAdjacencyMatrix adjacencyMatrix = new BuildingAdjacencyMatrix();
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
        if (adjacencyMatrix.GetBuildingAtGridPosition(selectedObject.GetGridPosition()) != null)
        {
            selectedObjects.Add(selectedObject);
            selectedObject.FreezeHighlight(true);
            selectedObject.ToggleHighlight(true);

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

}
