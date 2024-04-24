using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

struct NeighbourWeights { public int WeightFromNeighbour; public int WeightToNeighbour; };

public class BuildingAdjacencyMatrix
{
    List<List<int>> weights = new List<List<int>>();
    List<AbstractBuildingType> buildigs = new List<AbstractBuildingType>();


    int GetWeightOfPath(AbstractBuildingType from, AbstractBuildingType to)
    {
        int indexOfFromBuilding = buildigs.IndexOf(from);
        int indexOfToBuilding = buildigs.IndexOf(to);
        return weights[indexOfFromBuilding][indexOfToBuilding];
    }

    void AddBuilding(AbstractBuildingType building, Dictionary<AbstractBuildingType, NeighbourWeights> neighbours)
    {
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i].Add(0);
        }

        int widthOfMatrix = weights[0].Count;
        if (widthOfMatrix == 0) {
            widthOfMatrix = 1;
        }

        weights.Add(Enumerable.Repeat(0, widthOfMatrix).ToList());
        buildigs.Add(building);

        foreach (var neighbour in neighbours)
        {
            int indexOfNeighbour = buildigs.IndexOf(neighbour.Key);
            int indexOfBuilding = buildigs.IndexOf(building);
            if(indexOfNeighbour == -1) {
                throw new Exception($"Neighbour not in the list of buildings for {building.name} {building.gridPositions}");
            } 
            weights[indexOfNeighbour][indexOfBuilding] = neighbour.Value.WeightFromNeighbour;
            weights[indexOfBuilding][indexOfNeighbour] = neighbour.Value.WeightToNeighbour;
        }
    }

    void RemoveBuilding(AbstractBuildingType building)
    {
        int indexOfBuilding = buildigs.IndexOf(building);
        for (int i = 0; i < weights.Count; i++)
        {
            weights[i].RemoveAt(indexOfBuilding);
        }

        weights.RemoveAt(indexOfBuilding);
        buildigs.Remove(building);
    }

}
public class NavigationManager : MonoBehaviour
{
    List<List<int>> adjacencyMatrix = new List<List<int>>();
    List<AbstractBuildingType> adjacencyMatrixBuildings = new List<AbstractBuildingType>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


}
