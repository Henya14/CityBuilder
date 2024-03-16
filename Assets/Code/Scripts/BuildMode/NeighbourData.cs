using System.Collections.Generic;

public enum Direction
{
    North,
    South,
    East,
    West
}

public class NeighbourData
{
    public Dictionary<Direction, AbstractBuildingType> neighborDictionary {get;} = new Dictionary<Direction, AbstractBuildingType> {
        {Direction.North, null},
        {Direction.South, null},
        {Direction.East, null},
        {Direction.West, null},
    };

    public NeighbourData(Dictionary<Direction, AbstractBuildingType> neighborDictionary)
    {
        this.neighborDictionary = neighborDictionary;
    }

    public void SetNeighbour(Direction direction, AbstractBuildingType neighbour)
    {
        neighborDictionary[direction] = neighbour;
    }

    public AbstractBuildingType GetNeighbour(Direction direction)
    {
        return neighborDictionary.GetValueOrDefault(direction, null);
    }
}