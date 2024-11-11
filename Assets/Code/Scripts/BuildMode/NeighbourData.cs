using System.Collections.Generic;
using UnityEngine;




public class NeighbourData
{

    public Dictionary<Vector3Int, AbstractBuildingType> NeighboursForGridPositions {get;} = new Dictionary<Vector3Int, AbstractBuildingType>();

    public NeighbourData(Dictionary<Vector3Int, AbstractBuildingType> neighboursForGridPositions)
    {
        NeighboursForGridPositions = neighboursForGridPositions;
    }

    public void SetNeighbour(Vector3Int gridPosition, AbstractBuildingType neighbour)
    {
        NeighboursForGridPositions[gridPosition] = neighbour;
    }

    public AbstractBuildingType GetNeighbourForGridPosition(Vector3Int direction)
    {
        return NeighboursForGridPositions.GetValueOrDefault(direction, null);
    }
}