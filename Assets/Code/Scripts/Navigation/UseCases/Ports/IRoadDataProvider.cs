using System.Collections.Generic;

public interface IRoadDataProvider<T>
{
    float GetRoadWeight(GraphNode<T> node);
    float GetMaxRoadWeight(GraphNode<T> node);
    void RefreshWeight(GraphNode<T> node, float timeDelta);
    IReadOnlyList<GraphNode<T>> GetAllRoads();
}