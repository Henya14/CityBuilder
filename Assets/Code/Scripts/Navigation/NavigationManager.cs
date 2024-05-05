using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public struct NeighbourWeights { public int WeightFromNeighbour; public int WeightToNeighbour; };

public class BuildingAdjacencyMatrix
{
    List<List<int>> weights = new List<List<int>>();
    List<SelectableObject> buildings = new List<SelectableObject>();

    public BuildingAdjacencyMatrix() { }

    public BuildingAdjacencyMatrix(List<List<int>> weights, List<SelectableObject> buildings)
    {
        this.weights = weights;
        this.buildings = buildings;
    }
    public int GetWeightOfPath(SelectableObject from, SelectableObject to)
    {
        int indexOfFromBuilding = buildings.IndexOf(from);
        int indexOfToBuilding = buildings.IndexOf(to);
        return weights[indexOfFromBuilding][indexOfToBuilding];
    }

    public void AddBuilding(SelectableObject building, Dictionary<SelectableObject, NeighbourWeights> neighbours)
    {
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i].Add(0);
        }

        int widthOfMatrix = weights[0].Count;
        if (widthOfMatrix == 0)
        {
            widthOfMatrix = 1;
        }

        weights.Add(Enumerable.Repeat(0, widthOfMatrix).ToList());
        buildings.Add(building);

        foreach (var neighbour in neighbours)
        {
            int indexOfNeighbour = buildings.IndexOf(neighbour.Key);
            int indexOfBuilding = buildings.IndexOf(building);
            if (indexOfNeighbour == -1)
            {
                throw new Exception($"Neighbour not in the list of buildings for {building.GetGameObject().name} {building.Description()}");
            }
            weights[indexOfNeighbour][indexOfBuilding] = neighbour.Value.WeightFromNeighbour;
            weights[indexOfBuilding][indexOfNeighbour] = neighbour.Value.WeightToNeighbour;
        }
    }

    public void RemoveBuilding(SelectableObject building)
    {
        int indexOfBuilding = buildings.IndexOf(building);
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i].RemoveAt(indexOfBuilding);
        }

        weights.RemoveAt(indexOfBuilding);
        buildings.Remove(building);
    }

    public int GetIndexOfBuildingAtGridPosition(Vector3Int gridPosition)
    {
        int indexOfBuilding = buildings.FindIndex(b => b.GetGridPosition() == gridPosition);
        return indexOfBuilding;
    }

    public SelectableObject GetBuildingAtGridPosition(Vector3Int gridPosition)
    {
        SelectableObject building = buildings.Find(b => b.GetGridPosition() == gridPosition);
        return building;
    }

    public BuildingAdjacencyMatrix WhereBuildings(Func<SelectableObject, bool> predicate)
    {
        var sublist = new List<List<int>>();
        var filteredBuildings =buildings.Where(predicate).ToList();
        var indexes = filteredBuildings.Select(b => buildings.IndexOf(b)).ToList();

        var weightsWithFilteredRows = weights.Where((row, rowIdx) => indexes.Contains(rowIdx)).ToList();
        weightsWithFilteredRows.ForEach(row => sublist.Add(row.Where((col, colIdx) => indexes.Contains(colIdx)).ToList()));

        return new BuildingAdjacencyMatrix(sublist, filteredBuildings);
    }
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
