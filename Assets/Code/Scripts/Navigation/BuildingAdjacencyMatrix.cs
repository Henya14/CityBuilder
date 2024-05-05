using System;
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
            weights[i].Add(int.MinValue);
        }

        int widthOfMatrix = weights[0].Count;
        if (widthOfMatrix == 0)
        {
            widthOfMatrix = 1;
        }

        weights.Add(Enumerable.Repeat(int.MinValue, widthOfMatrix).ToList());
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
        var filteredBuildings = buildings.Where(predicate).ToList();
        var indexes = filteredBuildings.Select(b => buildings.IndexOf(b)).ToList();
        var sublist = this[indexes, indexes];

        return new BuildingAdjacencyMatrix(sublist, filteredBuildings);
    }

    public int this[int row, int column] {
        get  {
            return weights[row][column];
        }
        set  {
            weights[row][column] = value;
        }
    }

    public List<List<int>> this[List<int> rowIndexes, List<int> columnIndexes] {
        get  {
            var sublist = new List<List<int>>();
            var weightsWithFilteredRows = weights.Where((row, rowIdx) => rowIndexes.Contains(rowIdx)).ToList();
            weightsWithFilteredRows.ForEach(row => sublist.Add(row.Where((col, colIdx) => columnIndexes.Contains(colIdx)).ToList()));
            return sublist;
        }
    }
}