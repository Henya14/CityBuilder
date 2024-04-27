using System.Collections.Generic;
using UnityEngine;




public class NeighbourData
{

    public Dictionary<Vector3Int, AbstractBuildingType> neighboursForGridPositions {get;} = new Dictionary<Vector3Int, AbstractBuildingType>();

    public NeighbourData(Dictionary<Vector3Int, AbstractBuildingType> neighboursForGridPositions)
    {
        this.neighboursForGridPositions = neighboursForGridPositions;
    }

    public void SetNeighbour(Vector3Int gridPosition, AbstractBuildingType neighbour)
    {
        neighboursForGridPositions[gridPosition] = neighbour;
    }

    public AbstractBuildingType GetNeighbourForGridPosition(Vector3Int direction)
    {
        return neighboursForGridPositions.GetValueOrDefault(direction, null);
    }
}